using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;


namespace ManagedX.Content
{

    /// <summary>A <see cref="ContentDirectory" /> with archive support.</summary>
    /// <typeparam name="TFileDescriptor">Archived file descriptor type.</typeparam>
    /// <typeparam name="TArchive">Archive type.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class ContentDirectory<TFileDescriptor, TArchive> : ContentDirectory, IEnumerable<TArchive>
		where TFileDescriptor : FileDescriptor
		where TArchive : ArchiveStream<TFileDescriptor>
	{


		private Dictionary<string, TArchive> archives;



		/// <summary>Constructor.</summary>
		/// <param name="serviceProvider">The service provider; must not be null.</param>
		/// <param name="baseDirectoryPath">The path to the base directory; must exist.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="DirectoryNotFoundException"/>
		protected ContentDirectory( IServiceProvider serviceProvider, string baseDirectoryPath )
			: base( serviceProvider, baseDirectoryPath )
		{
			archives = new Dictionary<string, TArchive>();
		}


		/// <summary>Releases unmanaged, and optionally managed resources allocated by this <see cref="ContentDirectory"/>.</summary>
		/// <param name="disposing">true to release all resources, false to release unmanaged resources only.</param>
		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
			archives = null;
		}



		#region Archives

		/// <summary>Gets an archive, given its file name.</summary>
		/// <param name="fileName">An archive file name.</param>
		public TArchive this[ string fileName ]
		{
			get
			{
				if( fileName == null )
					return null;
				
				fileName = fileName.Trim();
				if( fileName.Length == 0 || fileName.IndexOfAny( Path.GetInvalidPathChars() ) > -1 )
					return null;

				archives.TryGetValue( fileName.ToUpperInvariant(), out TArchive output );
				return output;
			}
		}


		/// <summary>Gets the number of currently loaded archives.</summary>
		public int ArchiveCount { get { return archives.Count; } }


		/// <summary>Returns the first archive that contains the specified file.</summary>
		/// <param name="fileName">A file name.</param>
		/// <exception cref="ArgumentNullException"/>
		public TArchive Find( string fileName )
		{
			if( fileName == null )
				throw new ArgumentNullException( "fileName" );

			foreach( var archive in archives.Values )
			{
				if( archive.FileExists( fileName ) )
					return archive;
			}
			return null;
		}


		/// <summary>Returns a read-only collection of all archives containing the specified file.</summary>
		/// <param name="fileName">A file name.</param>
		/// <exception cref="ArgumentNullException"/>
		public ReadOnlyCollection<TArchive> FindAll( string fileName )
		{
			if( fileName == null )
				throw new ArgumentNullException( "fileName" );

			var output = new List<TArchive>();
			foreach( var archive in archives.Values )
			{
				if( archive.FileExists( fileName ) )
					output.Add( archive );
			}
			return new ReadOnlyCollection<TArchive>( output );
		}


		/// <summary>Returns a read-only collection of all file descriptors whose name matches the specified pattern.</summary>
		/// <param name="pattern">A regular expression.</param>
		/// <exception cref="ArgumentNullException"/>
		public ReadOnlyCollection<TFileDescriptor> FindAll( Regex pattern )
		{
			if( pattern == null )
				throw new ArgumentNullException( "pattern" );

			var output = new List<TFileDescriptor>();
			foreach( var archive in archives.Values )
				output.AddRange( archive.Find( pattern ) );

			return new ReadOnlyCollection<TFileDescriptor>( output );
		}


		/// <summary>Adds an archive to this content directory.</summary>
		/// <param name="archive">An initialized archive; must not be null.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="InvalidOperationException"/>
		protected void AddArchive( TArchive archive )
		{
			if( archive == null )
				throw new ArgumentNullException( "archive" );

			var key = Path.GetFileName( archive.Name ).ToUpperInvariant();
			if( archives.ContainsKey( key ) )
				throw new InvalidOperationException( "An archive with the same name already exists." );
			
			archives.Add( key, archive );
		}


		/// <summary>Closes all open archives.</summary>
		protected override void Unload()
		{
			if( archives == null )
				return;

			foreach( var archive in archives.Values )
			{
				if( archive != null )
					try
					{
						archive.Dispose();
					}
					catch( ObjectDisposedException )
					{
					}
			}
			archives.Clear();
		}

		#endregion Archives


		/// <summary>Initializes this <see cref="ContentDirectory"/>.
		/// <para>When overridden, opens all archives present in the directory and reads their table of contents.</para>
		/// </summary>
		public abstract override void Initialize();



		/// <summary>Returns a stream for reading a file.
		/// <para>Archives are ignored.</para>
		/// </summary>
		/// <param name="fileName">A file name.</param>
		/// <returns>Returns a stream for reading a file.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="FileNotFoundException"/>
		public virtual Stream OpenFile( string fileName )
		{
			return base.Open( fileName );
		}


		/// <summary>Returns a stream for reading a file.
		/// <para>Archives are supported.</para>
		/// </summary>
		/// <param name="fileName">A file name.</param>
		/// <returns>Returns a stream for reading the file.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="FileNotFoundException"/>
		public sealed override Stream Open( string fileName )
		{
			if( fileName == null )
				throw new ArgumentNullException( "fileName" );
			
			if( fileName.IndexOfAny( Path.GetInvalidPathChars() ) > -1 )
				throw new ArgumentException( fileName, "fileName" );

			var archive = this.Find( fileName );
			if( archive != null )
				return archive.Open( fileName );

			return this.OpenFile( fileName );
		}


		#region IEnumerable

		/// <summary>Returns an enumerator which iterates through the list of loaded archives.</summary>
		/// <returns>Returns an enumerator which iterates through the list of loaded archives.</returns>
		public IEnumerator<TArchive> GetEnumerator()
		{
			return archives.Values.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return archives.Values.GetEnumerator();
		}

		#endregion IEnumerable

	}

}