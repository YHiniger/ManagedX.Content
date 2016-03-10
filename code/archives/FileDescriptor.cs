using System;
using System.Diagnostics;
using System.IO;


namespace ManagedX.Content
{
	using Design;


	/// <summary>Default implementation of the <see cref="IFileDescriptor"/> interface.</summary>
	[DebuggerStepThrough]
	public class FileDescriptor : IFileDescriptor, IEquatable<FileDescriptor>
	{

		private Archive archive;
		private string fileName;
		private long length;
		private long position;



		/// <summary>Initializes a new <see cref="FileDescriptor"/>.</summary>
		/// <param name="archive">The archive the file belongs to; must not be null.</param>
		/// <param name="name">The name of the file; must not be null.</param>
		/// <param name="length">The file length, in bytes.</param>
		/// <param name="position">The position of the file, relative to the beginning of the archive stream.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public FileDescriptor( Archive archive, string name, long length, long position )
		{
			if( archive == null )
				throw new ArgumentNullException( "archive" );

			if( name == null )
				throw new ArgumentNullException( "name" );
			
			name = name.Trim();
			if( name.Length == 0 || name.IndexOfAny( Path.GetInvalidPathChars() ) != -1 || name.Contains( ":" ) )
				throw new ArgumentException( "Invalid file name.", "name" );

			if( length < 0L )
				throw new ArgumentOutOfRangeException( "length" );

			if( position < 0L )
				throw new ArgumentOutOfRangeException( "position" );

			this.archive = archive;
			this.fileName = name;
			this.length = length;
			this.position = position;
		}



		/// <summary>Gets the name of the file.</summary>
		public string Name { get { return string.Copy( fileName ); } }


		/// <summary>Gets the length, in bytes, of the file in the archive.
		/// <para>If the file is compressed, this is the compressed size.</para>
		/// </summary>
		public long Length { get { return length; } }


		/// <summary>Gets the position of the file in the archive.</summary>
		public long Position { get { return position; } }


		/// <summary>Returns a stream for reading the file.</summary>
		/// <returns>Returns a stream for reading the file.</returns>
		/// <exception cref="InvalidOperationException"/>
		public Stream Open()
		{
			try
			{
				return archive.Open( this );
			}
			catch( ObjectDisposedException ex )
			{
				throw new InvalidOperationException( "Archive is disposed.", ex );
			}
		}


		/// <summary>Tries to delete the file.</summary>
		/// <returns>Returns true on success, false otherwise.</returns>
		/// <exception cref="InvalidOperationException"/>
		public bool Delete()
		{
			try
			{
				return archive.Delete( this );
			}
			catch( ObjectDisposedException ex )
			{
				throw new InvalidOperationException( "Archive is disposed.", ex );
			}
		}


		/// <summary>Tries to rename the file.</summary>
		/// <param name="newName">The new file name; must not be null.</param>
		/// <returns>Returns true on success, false otherwise.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="InvalidOperationException"/>
		public bool Rename( string newName )
		{
			if( newName == null )
				throw new ArgumentNullException( "newName" );

			try
			{
				return archive.Rename( this, ref newName );
			}
			catch( ObjectDisposedException ex )
			{
				throw new InvalidOperationException( "Archive is disposed.", ex );
			}
		}



		/// <summary>Returns a hash code for this <see cref="FileDescriptor"/>.</summary>
		/// <returns>Returns a hash code for this <see cref="FileDescriptor"/>.</returns>
		public override int GetHashCode()
		{
			return archive.GetHashCode() ^ fileName.ToUpperInvariant().GetHashCode() ^ length.GetHashCode() ^ position.GetHashCode();
		}


		/// <summary>Returns a value indicating whether this <see cref="FileDescriptor"/> equals another <see cref="FileDescriptor"/>.</summary>
		/// <param name="other">A file descriptor.</param>
		/// <returns></returns>
		public virtual bool Equals( FileDescriptor other )
		{
			return ( other != null ) && ( archive == other.archive ) &&
				( position == other.position ) && ( length == other.length ) &&
				fileName.Equals( other.fileName, StringComparison.OrdinalIgnoreCase );
		}


		/// <summary>Returns a value indicating whether this <see cref="FileDescriptor"/> is equivalent to an object.</summary>
		/// <param name="obj">An object.</param>
		/// <returns>Returns true if the specified object is a <see cref="FileDescriptor"/> which equals this <see cref="FileDescriptor"/>, otherwise returns false.</returns>
		public sealed override bool Equals( object obj )
		{
			return this.Equals( obj as FileDescriptor );
		}


		/// <summary>Returns the <see cref="Name"/> of the associated file.</summary>
		/// <returns>Returns the <see cref="Name"/> of the associated file.</returns>
		public sealed override string ToString()
		{
			return string.Copy( fileName );
		}

	}

}
