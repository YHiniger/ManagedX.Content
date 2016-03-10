using System;
using System.IO;


namespace ManagedX.Content
{
	using Audio;


	/// <summary>Provides extension methods to <see cref="BinaryWriter"/> instances.</summary>
	public static class BinaryWriterExtensions
	{

		/// <summary></summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		public static void Write( this BinaryWriter writer, Guid value )
		{
			if( writer == null )
				throw new ArgumentNullException( "writer" );

			writer.Write( value.ToByteArray() );
		}


		/// <summary></summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		public static void Write( this BinaryWriter writer, Vector2 value )
		{
			if( writer == null )
				throw new ArgumentNullException( "writer" );

			writer.Write( value.X );
			writer.Write( value.Y );
		}


		/// <summary></summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		public static void Write( this BinaryWriter writer, Vector3 value )
		{
			if( writer == null )
				throw new ArgumentNullException( "writer" );

			writer.Write( value.X );
			writer.Write( value.Y );
			writer.Write( value.Z );
		}


		/// <summary></summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		public static void Write( this BinaryWriter writer, Vector4 value )
		{
			if( writer == null )
				throw new ArgumentNullException( "writer" );

			writer.Write( value.X );
			writer.Write( value.Y );
			writer.Write( value.Z );
			writer.Write( value.W );
		}


		/// <summary></summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		public static void Write( this BinaryWriter writer, Quaternion value )
		{
			if( writer == null )
				throw new ArgumentNullException( "writer" );

			writer.Write( value.X );
			writer.Write( value.Y );
			writer.Write( value.Z );
			writer.Write( value.W );
		}


		/// <summary></summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		public static void Write( this BinaryWriter writer, Matrix value )
		{
			if( writer == null )
				throw new ArgumentNullException( "writer" );

			writer.Write( value.M11 );
			writer.Write( value.M12 );
			writer.Write( value.M13 );
			writer.Write( value.M14 );

			writer.Write( value.M21 );
			writer.Write( value.M22 );
			writer.Write( value.M23 );
			writer.Write( value.M24 );

			writer.Write( value.M31 );
			writer.Write( value.M32 );
			writer.Write( value.M33 );
			writer.Write( value.M34 );

			writer.Write( value.M41 );
			writer.Write( value.M42 );
			writer.Write( value.M43 );
			writer.Write( value.M44 );
		}

	}

}
