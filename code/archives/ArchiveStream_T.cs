using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;


namespace ManagedX.Content
{
	using Design;


	/// <summary>A <see cref="FileStream"/> dedicated to archives.</summary>
	/// <typeparam name="TFileDescriptor">File descriptor type.</typeparam>
	public abstract class ArchiveStream<TFileDescriptor> : ArchiveStream, IEnumerable<TFileDescriptor>
		where TFileDescriptor : FileDescriptor
	{

		private Dictionary<string, TFileDescriptor> descriptors;
		//private List<MemoryStream> openStreams;



		#region Constructors, Dispose

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
		/// <exception cref="SecurityException"/>
		/// <exception cref="IOException"/>
		protected ArchiveStream( string path, FileMode mode, FileAccess access, FileShare share )
			: base( path, mode, access, share )
		{
			descriptors = new Dictionary<string, TFileDescriptor>();
			//openStreams = new List<MemoryStream>();
		}


		/// <summary>Constructor.</summary>
		/// <param name="path">The full path of the archive.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="ArgumentOutOfRangeException"/>
		/// <exception cref="PathTooLongException"/>
		/// <exception cref="FileNotFoundException"/>
		/// <exception cref="DirectoryNotFoundException"/>
		/// <exception cref="NotSupportedException"/>
		/// <exception cref="UnauthorizedAccessException"/>
		/// <exception cref="SecurityException"/>
		/// <exception cref="IOException"/>
		protected ArchiveStream( string path )
			: this( path, FileMode.Open, FileAccess.Read, FileShare.Read )
		{
		}



		/// <summary>Releases unmanaged, and optionally managed resources; clears the file list and closes all open streams.</summary>
		/// <param name="disposing">true to release all resources, false to release unmanaged resources only.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				//if( openStreams != null )
				//{
				//	while( openStreams.Count > 0 )
				//	{
				//		try
				//		{
				//			openStreams[ 0 ].Dispose();
				//		}
				//		catch( ObjectDisposedException )
				//		{
				//		}
				//		openStreams.RemoveAt( 0 );
				//	}
				//	openStreams = null;
				//}

				if( descriptors != null )
				{
					descriptors.Clear();
					descriptors = null;
				}
			}

			base.Dispose( disposing );
		}

		#endregion Constructors, Dispose


		
		/// <summary>Gets the descriptor of a file, given its name.</summary>
		/// <param name="fileName">A file name.</param>
		/// <returns>Returns the descriptor of the specified file, or null.</returns>
		public TFileDescriptor this[ string fileName ]
		{
			get
			{
				if( fileName == null )
					return null;

				TFileDescriptor descriptor;
				if( !descriptors.TryGetValue( fileName.ToUpperInvariant(), out descriptor ) )
					descriptor = null;

				return descriptor;
			}
		}


		/// <summary>Returns a read-only collection of file descriptors whose name matches the specified pattern.</summary>
		/// <param name="pattern">A regular expression; must not be null.</param>
		/// <returns>Returns a read-only collection of file descriptors whose name matches the specified pattern.</returns>
		/// <exception cref="ArgumentNullException"/>
		public ReadOnlyCollection<TFileDescriptor> Find( Regex pattern )
		{
			if( pattern == null )
				throw new ArgumentNullException( "pattern" );

			var array = new TFileDescriptor[ descriptors.Count ];
			descriptors.Values.CopyTo( array, 0 );

			var list = new List<TFileDescriptor>();
			for( var d = 0; d < array.Length; d++ )
				if( pattern.IsMatch( array[ d ].Name ) )
					list.Add( array[ d ] );
			
			return new ReadOnlyCollection<TFileDescriptor>( list );
		}


		/// <summary>Adds a file descriptor to the internal list.</summary>
		/// <param name="descriptor">A file descriptor; must not be null.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		protected void AddFile( TFileDescriptor descriptor )
		{
			if( descriptor == null )
				throw new ArgumentNullException( "descriptor" );

			if( descriptor.Name == null )
				throw new ArgumentException( "Invalid file descriptor name.", "descriptor" );
			
			descriptors.Add( descriptor.Name.ToUpperInvariant(), descriptor );
		}


		/// <summary>Gets the number of files the archive contains.</summary>
		public sealed override int FileCount { get { return descriptors.Count; } }


		/// <summary>Gets a read-only collection containing the name of all archived files.</summary>
		public sealed override ReadOnlyCollection<string> Files
		{
			get
			{
				var names = new string[ descriptors.Count ];
				var p = 0;
				foreach( var descriptor in descriptors.Values )
					names[ p++ ] = descriptor.Name;

				return new ReadOnlyCollection<string>( names );
			}
		}


		/// <summary>Returns a value indicating whether a file exists.</summary>
		/// <param name="fileName">A file name.</param>
		/// <returns>Returns true if the file exists, otherwise returns false.</returns>
		public sealed override bool FileExists( string fileName )
		{
			return this[ fileName ] != null;
		}


		/// <summary>Returns a stream for reading the specified file.</summary>
		/// <param name="fileName">A file name.</param>
		/// <returns>Returns a stream for reading the specified file.</returns>
		/// <exception cref="FileNotFoundException"/>
		public sealed override Stream Open( string fileName )
		{
			var descriptor = this[ fileName ];
			if( descriptor == null )
				throw new FileNotFoundException( this.GetType().Name + ".Open", fileName );
			
			return this.Open( descriptor );
		}


		/// <summary>Tries to remove a file from the archive, given its name.</summary>
		/// <param name="fileName">A valid file name.</param>
		/// <returns>Returns true if successful, otherwise returns false.</returns>
		/// <exception cref="FileNotFoundException"/>
		public sealed override bool Delete( string fileName )
		{
			var descriptor = this[ fileName ];
			if( descriptor == null )
				throw new FileNotFoundException( this.GetType().Name + ".Delete", fileName );

			return this.Delete( descriptor );
		}


		/// <summary>Tries to rename an archived file.</summary>
		/// <param name="fileName">The original file name.</param>
		/// <param name="newName">The new file name.</param>
		/// <returns>Returns true if successful, otherwise returns false.</returns>
		/// <exception cref="FileNotFoundException"/>
		public sealed override bool Rename( string fileName, string newName )
		{
			var descriptor = this[ fileName ];
			if( descriptor == null )
				throw new FileNotFoundException( this.GetType().Name + ".Rename", fileName );
			
			return this.Rename( descriptor, ref newName );
		}


		#region IFileDescriptor support

		/// <summary>Returns a stream for reading an archived file, given its descriptor.</summary>
		/// <param name="descriptor">A valid file descriptor; must not be null.</param>
		/// <returns>Returns a stream for reading the specified <paramref name="descriptor"/>.</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="FileNotFoundException"/>
		/// <exception cref="InvalidDataException"/>
		internal protected override Stream Open( IFileDescriptor descriptor )
		{
			if( descriptors == null )
				throw new ObjectDisposedException( this.Name );

			if( descriptor == null )
				throw new ArgumentNullException( "descriptor" );

			var desc = this[ descriptor.Name ];
			if( desc == null )
				throw new ArgumentException( this.GetType().Name + ".Open", "descriptor" );
			if( desc != descriptor )
				throw new FileNotFoundException();

			base.Position = descriptor.Position;
			var length = (int)descriptor.Length;
			var buffer = new byte[ length ];
			if( base.Read( buffer, 0, length ) != length )
				throw new InvalidDataException( this.GetType().Name + ".Open" );

			var output = new MemoryStream( buffer, 0, buffer.Length, false, true );
			//openStreams.Add( output );
			return output;
		}


		/// <summary>Removes a file descriptor from the archive.</summary>
		/// <param name="descriptor">A valid file descriptor; must not be null.</param>
		/// <returns>Returns true if the descriptor has been removed from the internal list, otherwise returns false (the descriptor doesn't belong to the list, file removal is not supported, etc).</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="FileNotFoundException"/>
		internal protected override bool Delete( IFileDescriptor descriptor )
		{
			if( descriptors == null )
				throw new ObjectDisposedException( this.Name );

			if( descriptor == null )
				throw new ArgumentNullException( "descriptor" );

			var fileName = descriptor.Name;
			if( fileName == null )
				throw new ArgumentException( this.GetType().Name + ".Delete", "descriptor" );

			if( this[ fileName ] != descriptor )
				throw new FileNotFoundException( this.GetType().Name + ".Delete", fileName );

			return descriptors.Remove( fileName.ToUpperInvariant() );
		}


		/// <summary>Renames a file, given its descriptor.</summary>
		/// <param name="descriptor">A valid file descriptor; must not be null.</param>
		/// <param name="newName">The new file name; must be a valid, non-null, relative path.</param>
		/// <returns>Returns true if the file has been renamed, otherwise returns false (name collision, not supported, etc).</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		internal protected override bool Rename( IFileDescriptor descriptor, ref string newName )
		{
			if( descriptors == null )
				throw new ObjectDisposedException( this.Name );

			if( descriptor == null )
				throw new ArgumentNullException( "descriptor" );
			
			if( descriptor.Name == null )
				throw new ArgumentException( this.GetType().Name + ".Rename", "descriptor" );

			var desc = this[ descriptor.Name ];
			if( desc == null || desc != descriptor )
				throw new ArgumentException( this.GetType().Name + ".Rename", "descriptor" );

			if( newName == null )
				throw new ArgumentNullException( "newName" );
			
			newName = newName.Trim();
			if( newName.IndexOfAny( Path.GetInvalidPathChars() ) > -1 )
				throw new ArgumentException( this.GetType().Name + ".Rename", "newName" );

			if( this[ newName ] != null )
				return false;

			if( !descriptors.Remove( descriptor.Name.ToUpperInvariant() ) )
				throw new InvalidOperationException( "Bad implementation." );
			
			descriptors.Add( newName.ToUpperInvariant(), desc );
			return true;
		}

		#endregion IFileDescriptor support


		#region IEnumerable
		
		/// <summary>Returns an enumerator which iterates within the file descriptors stored by the archive.</summary>
		/// <exception cref="ObjectDisposedException"/>
		public IEnumerator<TFileDescriptor> GetEnumerator()
		{
			if( descriptors == null )
				throw new ObjectDisposedException( this.Name );
			
			return descriptors.Values.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion IEnumerable

	}

}