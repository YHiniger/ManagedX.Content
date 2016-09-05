using System;
using System.IO;


namespace ManagedX.Content
{
	using Audio;


	/// <summary>Provides extension methods to <see cref="BinaryReader"/> instances.</summary>
	public static class BinaryReaderExtensions
	{

		/// <summary>Reads a <see cref="Guid"/> structure from a stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns>Returns a <see cref="Guid"/> structure initialized with data from the specified <paramref name="reader"/>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static Guid ReadGuid( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			try
			{
				return new Guid( reader.ReadBytes( 16 ) );
			}
			catch( ArgumentException )
			{
				throw new EndOfStreamException();
			}
		}


		/// <summary>Reads a <see cref="TimeSpan"/> structure from a stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns>Returns a <see cref="TimeSpan"/> structure initialized with data from the specified <paramref name="reader"/>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static TimeSpan ReadTimeSpan( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			try
			{
				return TimeSpan.FromTicks( reader.ReadInt64() );
			}
			catch( Exception )
			{
				throw;
			}
		}


		/// <summary>Reads a <see cref="Vector2"/> from the stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static Vector2 ReadVector2( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			Vector2 result;
			result.X = reader.ReadSingle();
			result.Y = reader.ReadSingle();
			return result;
		}


		/// <summary>Reads a <see cref="Vector3"/> from the stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static Vector3 ReadVector3( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );
			
			Vector3 result;
			result.X = reader.ReadSingle();
			result.Y = reader.ReadSingle();
			result.Z = reader.ReadSingle();
			return result;
		}


		/// <summary>Reads a <see cref="Vector4"/> from the stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static Vector4 ReadVector4( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			Vector4 result;
			result.X = reader.ReadSingle();
			result.Y = reader.ReadSingle();
			result.Z = reader.ReadSingle();
			result.W = reader.ReadSingle();
			return result;
		}


		/// <summary>Reads a <see cref="BoundingBox"/> from the stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static BoundingBox ReadBoundingBox( this BinaryReader reader )
		{
			try
			{
				BoundingBox box;
				box.Min = ReadVector3( reader );
				box.Max = ReadVector3( reader );
				return box;
			}
			catch( Exception )
			{
				throw;
			}
		}


		/// <summary>Reads a <see cref="Ray"/> from the stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static Ray ReadRay( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			Ray result;
			result.Position = reader.ReadVector3();
			result.Direction = reader.ReadVector3();
			return result;
		}


		/// <summary>Reads a <see cref="Quaternion"/> from the stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static Quaternion ReadQuaternion( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			Quaternion result;
			result.X = reader.ReadSingle();
			result.Y = reader.ReadSingle();
			result.Z = reader.ReadSingle();
			result.W = reader.ReadSingle();
			return result;
		}


		/// <summary>Reads a <see cref="Matrix"/> from the stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static Matrix ReadMatrix( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			Matrix result;

			result.M11 = reader.ReadSingle();
			result.M12 = reader.ReadSingle();
			result.M13 = reader.ReadSingle();
			result.M14 = reader.ReadSingle();

			result.M21 = reader.ReadSingle();
			result.M22 = reader.ReadSingle();
			result.M23 = reader.ReadSingle();
			result.M24 = reader.ReadSingle();

			result.M31 = reader.ReadSingle();
			result.M32 = reader.ReadSingle();
			result.M33 = reader.ReadSingle();
			result.M34 = reader.ReadSingle();

			result.M41 = reader.ReadSingle();
			result.M42 = reader.ReadSingle();
			result.M43 = reader.ReadSingle();
			result.M44 = reader.ReadSingle();

			return result;
		}



		/// <summary>Reads a <see cref="WaveFormat"/> structure from a stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns>Returns a <see cref="WaveFormat"/> structure initialized with data from the specified <paramref name="reader"/>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		public static WaveFormat ReadWaveFormat( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			var formatTag = reader.ReadInt16();
			var channelCount = reader.ReadUInt16();
			var samplesPerSecond = reader.ReadInt32();
			var averageBytesPerSecond = reader.ReadInt32();
			var blockAlign = reader.ReadUInt16();

			return new WaveFormat( formatTag, (int)channelCount, samplesPerSecond, averageBytesPerSecond, (int)blockAlign );
		}


		/// <summary>Reads a <see cref="WaveFormatEx"/> structure from a stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns>Returns a <see cref="WaveFormatEx"/> structure initialized with data from the specified <paramref name="reader"/>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
		public static WaveFormatEx ReadWaveFormatEx( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			var baseFormat = ReadWaveFormat( reader );
			var bitsPerSample = reader.ReadUInt16();
			var extraInfoSize = reader.ReadUInt16();

			return new WaveFormatEx( baseFormat, bitsPerSample, extraInfoSize );
		}


		/// <summary>Reads a <see cref="WaveFormatExtensible"/> structure from a stream and returns it.</summary>
		/// <param name="reader">A <see cref="BinaryReader"/>; must not be null.</param>
		/// <returns>Returns a <see cref="WaveFormatExtensible"/> structure initialized with data from the specified <paramref name="reader"/>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="EndOfStreamException"/>
		/// <exception cref="IOException"/>
		/// <exception cref="InvalidDataException"/>
		public static WaveFormatExtensible ReadWaveFormatExtensible( this BinaryReader reader )
		{
			if( reader == null )
				throw new ArgumentNullException( "reader" );

			var fmt = ReadWaveFormatEx( reader );
			if( fmt.FormatTag != (short)WaveFormatTag.Extensible )
				throw new InvalidDataException();

			return new WaveFormatExtensible( fmt, reader.ReadUInt16(), (AudioChannels)reader.ReadInt32(), reader.ReadGuid() );
		}

	}

}
