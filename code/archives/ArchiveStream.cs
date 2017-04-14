using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;


namespace ManagedX.Content
{
	using Design;


	/// <summary>Base class for archive streams.</summary>
	[DebuggerStepThrough]
	public abstract class ArchiveStream : FileStream
	{

		private FileInfo fileInfo;
		private BinaryReader reader;



		#region Constructor, Dispose

		/// <summary>Constructor.</summary>
		/// <param name="path">The full path to the archive file.</param>
		/// <param name="mode">The file mode parameter.</param>
		/// <param name="access">The file access parameter.</param>
		/// <param name="share">The file share parameter.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="ArgumentOutOfRangeException"/>
		/// <exception cref="PathTooLongException"/>
		/// <exception cref="FileNotFoundException"/>
		/// <exception cref="DirectoryNotFoundException"/>
		/// <exception cref="NotSupportedException"/>
		/// <exception cref="UnauthorizedAccessException"/>
		/// <exception cref="System.Security.SecurityException"/>
		/// <exception cref="IOException"/>
		internal ArchiveStream( string path, FileMode mode, FileAccess access, FileShare share )
			: base( path, mode, access, share )
		{
			fileInfo = new FileInfo( base.Name );
			reader = new BinaryReader( this );
		}



		/// <summary>Releases unmanaged and optionally managed resources.</summary>
		/// <param name="disposing">True to release all resources (ie: <see cref="Reader"/>), or false to release only unmanaged resources.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( reader != null )
				{
					try
					{
						reader.Dispose();
					}
					catch( ObjectDisposedException )
					{
					}
					reader = null;
				}
			}
			
			fileInfo = null;
			
			base.Dispose( disposing );
		}

		#endregion Constructor, Dispose



		/// <summary>Gets information about the archive file.</summary>
		protected FileInfo Info { get { return fileInfo; } }


		/// <summary>Gets the reader associated with the archive.</summary>
		protected BinaryReader Reader { get { return reader; } }


		/// <summary>Gets the number of files the archive contains.</summary>
		public abstract int FileCount { get; }


		/// <summary>Gets a read-only collection containing the name of all archived files.</summary>
		public abstract ReadOnlyCollection<string> Files { get; }


		/// <summary>Returns a value indicating whether a file exists.</summary>
		/// <param name="fileName">A file name.</param>
		/// <returns>Returns true if the file exists, otherwise returns false.</returns>
		public abstract bool FileExists( string fileName );


		/// <summary>When overridden, adds an external file to the archive.</summary>
		/// <param name="info"></param>
		public abstract void AddFile( FileInfo info );


		/// <summary>Returns a stream for reading the specified file.</summary>
		/// <param name="fileName">A file name.</param>
		/// <returns>Returns a stream for reading the specified file.</returns>
		public abstract Stream Open( string fileName );


		/// <summary>Tries to remove a file from the archive, given its name.</summary>
		/// <param name="fileName">A valid file name.</param>
		/// <returns></returns>
		public abstract bool Delete( string fileName );


		/// <summary>Tries to rename an archived file.</summary>
		/// <param name="fileName">The original file name.</param>
		/// <param name="newName">The new file name.</param>
		/// <returns>True on success, false otherwise.</returns>
		public abstract bool Rename( string fileName, string newName );


		/// <summary>When overridden, reads the table of contents.</summary>
		public abstract void Initialize();


		/// <summary>Closes the file stream.</summary>
		/// <remarks>This method is sealed to ensure it can't be overridden, according to MS recommendations (link?).</remarks>
		public sealed override void Close()
		{
			base.Close();
		}


		/// <summary>When overridden, saves the archive to a stream.</summary>
		/// <param name="output">A stream for writing the archive to.</param>
		public abstract void SaveTo( Stream output );


		#region IFileDescriptor support

		/// <summary>Returns a stream for reading an archived file, given its descriptor.
		/// <para>Override this method if the codec needs to be configured.</para>
		/// </summary>
		/// <param name="descriptor">A valid file descriptor; must not be null.</param>
		/// <returns>Returns a stream for reading the specified <paramref name="descriptor"/>.</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="FileNotFoundException"/>
		/// <exception cref="InvalidDataException"/>
		protected internal abstract Stream Open( IFileDescriptor descriptor );


		/// <summary>Removes a file descriptor from the archive.</summary>
		/// <param name="descriptor">A valid file descriptor; must not be null.</param>
		/// <returns>Returns true if the descriptor has been removed from the internal list, otherwise returns false (the descriptor doesn't belong to the list, file removal is not supported, etc).</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="FileNotFoundException"/>
		protected internal abstract bool Delete( IFileDescriptor descriptor );


		/// <summary>Renames a file, given its descriptor.</summary>
		/// <param name="descriptor">A valid file descriptor; must not be null.</param>
		/// <param name="newName">[In,Out] The new file name; must be a valid, non-null, relative path.</param>
		/// <returns>Returns true if the file has been renamed, otherwise returns false (name collision, not supported, etc).</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1045:DoNotPassTypesByReference" )]
		protected internal abstract bool Rename( IFileDescriptor descriptor, ref string newName );

		#endregion IFileDescriptor support

		
		/// <summary>Returns the file name and extension of the stream.</summary>
		/// <returns>Returns the file name and extension of the stream.</returns>
		public sealed override string ToString()
		{
			return Path.GetFileName( base.Name );
		}

	}

}