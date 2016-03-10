using System;
using System.Drawing;
using System.IO;


namespace Erosion.Content.Pipeline
{
	using Erosion.Content.Images;
	
	
	/// <summary></summary>
	public class ImagePlugIn : ContentPlugIn<Image<Color>>
	{

		#region Static

		
		private static Image<Color> ImportBMP( Stream input )
		{
			var reader = new BinaryReader( input );		// TODO - use a ContentReader or similar

			var bmpHeader = reader.ReadBmpFileHeader();


			var dibHeader = reader.ReadDibHeader();

			if( dibHeader.Planes != 1 )
				throw new NotSupportedException( "Not supported: plane count = " + dibHeader.Planes );

			if( dibHeader.BitsPerPixel != 24 )
				throw new NotSupportedException( "Not supported: bits per pixel = " + dibHeader.BitsPerPixel );

			if( dibHeader.Width < 0 || dibHeader.Height < 0 )
				throw new InvalidDataException( "Invalid BMP image size." );

			if( input.Seek( bmpHeader.Offset, SeekOrigin.Begin ) != bmpHeader.Offset )
				throw new InvalidDataException( "Invalid BMP image file." );

			var output = new Image<Color>( dibHeader.Width, dibHeader.Height );
			int x, y;
			for( y = 0; y < dibHeader.Height; y++ )
			{
				for( x = 0; x < dibHeader.Width; x++ )
					output.Pixels[ y * dibHeader.Width + x ] = input.ReadColorBgr();
			}
			return output;
		}


		/// <summary>Decodes a Run-Length Encoded stream.</summary>
		/// <param name="input">A stream for reading RLE packets from.</param>
		/// <param name="elementSize">The size, in byte, of a pixel.</param>
		/// <param name="count">The number of pixels.</param>
		/// <param name="buffer">An array to receive the decoded data.</param>
		/// <returns></returns>
		private static void ReadRLEPackets( Stream input, int elementSize, int count, byte[] buffer )
		{
			int imgLen = elementSize * count;
			var rlePacket = new byte[ elementSize ];
			int data, repeatCount, j, o;

			o = 0;

			for( int i = 0; i < imgLen; i++ )
			{
				data = input.ReadByte();
				repeatCount = ( data & 0x7F ) + 1;

				if( ( data & 0x80 ) == 0 )
				{
					// lecture de [repeatCount] paquets bruts:
					j = elementSize * repeatCount;
					if( input.Read( buffer, o, j ) != j )
						throw new InvalidDataException( "Failed to read RLE packet." );
					o += j;
				}
				else
				{
					// RLE: on va lire un élément, et le copier [repeatCount] fois dans le flux de sortie:
					if( input.Read( rlePacket, 0, elementSize ) != elementSize )
						throw new InvalidDataException( "Failed to read RLE element." );

					for( j = 1; j <= repeatCount; j++ )
					{
						Array.Copy( rlePacket, 0, buffer, o, elementSize );
						o += elementSize;
					}
				}

				i += elementSize * repeatCount - 1;
				// la décrémentation est requise afin de compenser l'incrémentation automatique de [i] par la boucle For.
			}
		}

		/// <summary></summary>
		/// <param name="input">A stream for reading the TGA image data from.</param>
		/// <param name="preMultiplyAlpha"></param>
		/// <returns></returns>
		private static Image<Color> ImportTGA( Stream input, bool preMultiplyAlpha )
		{
			var reader = new BinaryReader( input );

			var header = reader.ReadTgaHeader();

			if( header.ColorMapType != (byte)TgaColorMapType.None )
				throw new NotSupportedException( "Not supported: color map type = " + header.ColorMapType );

			if( header.ImageType != (byte)TgaImageType.Rgb && header.ImageType != (byte)TgaImageType.RleRgb )
				throw new NotSupportedException( "Not supported: image type = " + header.ImageType );

			if( header.ColorDepth != 24 && header.ColorDepth != 32 )
				throw new NotSupportedException( "Not supported: color depth = " + header.ColorDepth );


			reader.BaseStream.Seek( header.IdLength, SeekOrigin.Current ); // skip ID


			int pixelCount = (int)header.Width * (int)header.Height;
			int pixelSize = (int)header.ColorDepth >> 3;
			int imgLength = pixelCount * pixelSize;

			var buffer = new byte[ imgLength ];
			if( header.ImageType == (byte)TgaImageType.Rgb )
			{
				if( reader.Read( buffer, 0, imgLength ) != imgLength )
					throw new InvalidDataException( "Invalid image data." );
			}
			else
				ReadRLEPackets( input, pixelSize, pixelCount, buffer );

			//	int color;
			Image<Color> image = new Image<Color>( header.Width, header.Height );
			int p, i;
			if( pixelSize == 3 )
			{
				for( p = 0; p < pixelCount; p++ )
				{
					i = p * 3;
					image.Pixels[ p ] = Color.FromArgb( 255, buffer[ i ], buffer[ i + 1 ], buffer[ i + 2 ] );
				}
			}
			else // pixelSize = 4
			{
				Color pixel;
				for( p = 0; p < pixelCount; p++ )
				{
					i = p * 4;
					pixel = Color.FromArgb( buffer[ i + 3 ], buffer[ i ], buffer[ i + 1 ], buffer[ i + 2 ] );
					if( preMultiplyAlpha )
						image.Pixels[ p ] = pixel.ToPreMultipliedAlpha();
					else
						image.Pixels[ p ] = pixel;
				}
			}

			if( ( header.Descriptor & (byte)TgaHeaderDescriptors.Mirror ) == (byte)TgaHeaderDescriptors.Mirror )
				image.Mirror();

			if( ( header.Descriptor & (byte)TgaHeaderDescriptors.UpSideDown ) == 0 )
				image.Flip();

			return image;
		}
	


		private static readonly ImagePlugIn instance = new ImagePlugIn();


		/// <summary></summary>
		public static ImagePlugIn Default
		{
			get { return instance; }
		}


		#endregion



		private static string[] Add( string fileExtension, params string[] fileExtensions )
		{
			var list = new System.Collections.Generic.List<string>();
			list.Add( fileExtension );
			for( int x = 0; x < fileExtensions.Length; x++ )
				if( fileExtensions[x] != null && !list.Contains( fileExtensions[x] ) )
					list.Add( fileExtensions[ x ] );
			var output = new string[ list.Count ];
			list.CopyTo( output, 0 );
			list.Clear();
			return output;
		}


		/// <summary>Constructor.</summary>
		/// <param name="fileExtensions"></param>
		protected ImagePlugIn( params string[] fileExtensions )
			: base( "bmp", Add( "tga", fileExtensions ) )
		{
		}
		

		
		/// <summary></summary>
		/// <param name="fileName">The name of the <paramref name="input"/> stream; mainly used for debugging.</param>
		/// <param name="input">A stream for reading the image from.</param>
		/// <returns></returns>
		protected override Image<Color> ImportInternal( string fileName, System.IO.Stream input )
		{
			var extension = Path.GetExtension( fileName ).TrimStart( '.' );
			
			if( extension.Equals( "bmp", StringComparison.OrdinalIgnoreCase ) )
				return ImportBMP( input );

			if( extension.Equals( "tga", StringComparison.OrdinalIgnoreCase ) )
				return ImportTGA( input, false );

			throw new NotSupportedException();
		}


		/// <summary>When not overridden, throws a <see cref="NotImplementedException"/>.</summary>
		/// <param name="content">The image to export.</param>
		/// <param name="fileName">The name of the <paramref name="output"/> stream; mainly used for debugging.</param>
		/// <param name="output">A stream for writing the image to.</param>
		public override void Export( Image<Color> content, string fileName, Stream output )
		{
			throw new NotImplementedException();
		}


		/// <summary>When not overridden, returns false: the <see cref="ImagePlugIn"/> can't export images.</summary>
		public override bool CanExport
		{
			get { return false; }
		}

	}

}
