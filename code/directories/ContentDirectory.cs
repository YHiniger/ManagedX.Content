using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;


namespace ManagedX.Content
{
	using Design;

	// Chaque dossier de contenu dispose de son propre gestionnaire de plug-ins ?
	// Ou alors tous les dossiers de contenu partagent le même gestionnaire ?
	// L'implémentation actuelle supporte les 2 cas: si un gestionnaire de plug-ins est présent dans les services, le dossier l'utilise, sinon il crée son propre gestionnaire.


	/// <summary>Base class for content directories.</summary>
	[DebuggerStepThrough]
	public abstract class ContentDirectory : IDisposable
	{


		private IServiceProvider services;
		private string baseDirectory;
		private IContentPluginManager plugInManager;
		private IDisposable ownPlugInManager;
		private bool disposed;



		/// <summary>Instantiates a new <see cref="ContentDirectory"/>.</summary>
		/// <param name="serviceProvider">The service provider; must not be null.
		/// <para>If an <see cref="IContentPluginManager"/> service is present, the <see cref="ContentDirectory"/> will use it, otherwise it will use its own.</para>
		/// </param>
		/// <param name="baseDirectoryPath">The path to the base directory; must exist.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="DirectoryNotFoundException"/>
		[SuppressMessage( "Microsoft.Reliability", "CA2000" )]
		protected ContentDirectory( IServiceProvider serviceProvider, string baseDirectoryPath )
		{
			if( serviceProvider == null )
				throw new ArgumentNullException( "serviceProvider" );

			if( baseDirectoryPath == null )
				throw new ArgumentNullException( "baseDirectoryPath" );

			baseDirectoryPath = baseDirectoryPath.Trim();
			if( baseDirectoryPath.Length == 0 || baseDirectoryPath.IndexOfAny( Path.GetInvalidPathChars() ) > -1 )
				throw new ArgumentException( baseDirectoryPath, "baseDirectoryPath" );

			// THINKABOUTME: when baseDirectoryPath is empty, use the current one

			if( !Directory.Exists( baseDirectoryPath ) )
				throw new DirectoryNotFoundException();

			services = serviceProvider;
			baseDirectory = baseDirectoryPath;
			//openStreams = new List<Stream>();
			//cache = new Dictionary<string, object>();

			plugInManager = services.GetService( typeof( IContentPluginManager ) ) as IContentPluginManager;
			if( plugInManager == null )
			{
				plugInManager = new ContentPluginManager();
				ownPlugInManager = plugInManager as IDisposable;
			}
		}


		/// <summary>Finalizer.</summary>
		~ContentDirectory()
		{
			this.Dispose( false );
		}


		#region IDisposable

		/// <summary>Gets a value indicating whether the <see cref="ContentDirectory"/> has been disposed.</summary>
		public bool IsDisposed { get { return disposed; } }


		/// <summary>Closes all open streams and clears the list of content importers.</summary>
		/// <param name="disposing"></param>
		[SuppressMessage( "Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "plugInManager", Justification = "The plug-in manager might be shared with other content directories." )]
		protected virtual void Dispose( bool disposing )
		{
			this.Unload();

			if( disposing && !disposed )
			{
				if( ownPlugInManager != null )
				{
					ownPlugInManager.Dispose();
					ownPlugInManager = null;
				}
				plugInManager = null;
				disposed = true;
			}
		}


		/// <summary>Releases all resources allocated by the <see cref="ContentDirectory"/>.</summary>
		public void Dispose()
		{
			this.Dispose( true );
			GC.SuppressFinalize( this );
		}

		#endregion IDisposable



		/// <summary>Gets the service provider associated with the <see cref="ContentDirectory"/>.</summary>
		protected IServiceProvider Services { get { return services; } }


		/// <summary>Gets the path to the base directory (usually where executable binaries are located).</summary>
		public string BaseDirectoryPath { get { return string.Copy( baseDirectory ); } }


		/// <summary>Gets the directory where data files are stored.
		/// <para>Defaults to the <see cref="BaseDirectoryPath"/>.</para>
		/// </summary>
		public virtual string DataDirectoryPath { get { return string.Copy( baseDirectory ); } }


		/// <summary>When overridden, gets a value indicating whether the </summary>
		public abstract bool IsDataDirectorySymbolicLink { get; }


		/// <summary>Initializes this <see cref="ContentDirectory"/>.</summary>
		public abstract void Initialize();


		/// <summary>Called when this <see cref="ContentDirectory"/> is reset or disposing.</summary>
		protected virtual void Unload()
		{
		}


		/// <summary>Resets this <see cref="ContentDirectory"/>.
		/// <para>Calls <see cref="Unload"/>, followed by <see cref="Initialize"/>.</para>
		/// </summary>
		public virtual void Reset()
		{
			this.Unload();
			this.Initialize();
		}


		/// <summary>Returns a stream for reading a file.</summary>
		/// <param name="fileName">A file name.</param>
		/// <returns>Returns a stream for reading a file.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="FileNotFoundException"/>
		public virtual Stream Open( string fileName )
		{
			if( fileName == null )
				throw new ArgumentNullException( "fileName" );

			fileName = fileName.Trim();
			if( fileName.Length == 0 || fileName.IndexOfAny( Path.GetInvalidPathChars() ) > -1 )
				throw new ArgumentException( fileName, "fileName" );

			// FIXME - only files located in the base directory (or its subdirectories) must be supported !

			if( !File.Exists( fileName ) )
				throw new FileNotFoundException( this.GetType().Name + ".Open: " + fileName, fileName );

			return new FileStream( fileName, FileMode.Open, FileAccess.Read, FileShare.Read );
		}


		#region Content

		/// <summary>Gets the content plug-in manager associated with the <see cref="ContentDirectory"/>.
		/// <para>Beware that it might come from the services, and thus be shared with other content directories.</para>
		/// </summary>
		public IContentPluginManager ContentPlugins { get { return plugInManager; } }


		///// <summary>Imports content and returns it.</summary>
		///// <param name="contentType">The content type.</param>
		///// <param name="fileName">The name of the file to read content from.</param>
		///// <returns>The imported content.</returns>
		///// <exception cref="ArgumentNullException"/>
		///// <exception cref="ArgumentException"/>
		///// <exception cref="NotSupportedException"/>
		//[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		//public object Import( Type contentType, string fileName )
		//{
		//	if( contentType == null )
		//		throw new ArgumentNullException( "contentType" );

		//	if( fileName == null )
		//		throw new ArgumentNullException( "fileName" );

		//	fileName = fileName.Trim();
		//	if( fileName.Length == 0 || fileName.IndexOfAny( Path.GetInvalidPathChars() ) > -1 )
		//		throw new ArgumentException( fileName, "fileName" );

		//	bool loaded = false;
		//	object output = null;
		//	var input = this.Open( fileName );
		//	if( input != null )
		//	{
		//		foreach( var importer in plugInManager.GetPlugIns( contentType, Path.GetExtension( fileName ) ) )
		//		{
		//			try
		//			{
		//				output = importer.Import( fileName, input );
		//				loaded = true;
		//				break;
		//			}
		//			catch( Exception )
		//			{
		//				if( input.CanSeek )
		//				{
		//					input.Position = 0L;
		//				}
		//				else
		//				{
		//					openStreams.Remove( input );
		//					input.Dispose();
		//					input = this.Open( fileName );
		//				}
		//			}
		//		}
		//		openStreams.Remove( input );
		//		input.Dispose();
		//	}

		//	if( !loaded )
		//		throw new NotSupportedException();

		//	return output;
		//}


		/// <summary>Imports content from a file, and returns it.</summary>
		/// <typeparam name="TContent">Content type.</typeparam>
		/// <param name="fileName">The name of the file to read content from.</param>
		/// <returns>The imported content.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="NotSupportedException"/>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		public TContent Import<TContent>( string fileName )
		{
			if( fileName == null )
				throw new ArgumentNullException( "fileName" );

			fileName = fileName.Trim();
			if( fileName.Length == 0 || fileName.IndexOfAny( Path.GetInvalidPathChars() ) > -1 )
				throw new ArgumentException( fileName, "fileName" );

			var loaded = false;
			var output = default( TContent );
			var input = this.Open( fileName );

			foreach( var importer in plugInManager.GetImporters<TContent>( Path.GetExtension( fileName ) ) )
			{
				try
				{
					output = importer.Import( fileName, input );
					loaded = true;
					break;
				}
				catch( Exception )
				{
					if( input.CanSeek )
						input.Position = 0L;
					else
					{
						input.Dispose();
						input = this.Open( fileName );
					}
				}
			}
			input.Dispose();

			if( !loaded )
				throw new NotSupportedException( "Unsupported content type." );

			return output;
		}


#if false
        /// <summary>
        /// ...
        /// </summary>
        /// <typeparam name="TContent">Content type.</typeparam>
        /// <param name="fileName">The name of the file to read content from.</param>
        public TContent Load<TContent>(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            string key = fileName.ToUpperInvariant();
            if (myCache.ContainsKey(key)) return (TContent)myCache[key];
            TContent output = this.Import<TContent>(fileName);
            myCache.Add(key, output);
            return output;
        }
#endif

		#endregion Content



		/// <summary>Returns a hash code based on the <see cref="BaseDirectoryPath"/>.</summary>
		/// <returns>Returns a hash code based on the <see cref="BaseDirectoryPath"/>.</returns>
		public override int GetHashCode()
		{
			return baseDirectory.ToUpperInvariant().GetHashCode();
		}


		/// <summary>Returns a value indicating whether the <see cref="ContentDirectory"/> is equivalent to an object.</summary>
		/// <param name="obj">The object being compared.</param>
		/// <returns>A value indicating whether the <see cref="ContentDirectory"/> is equivalent to an object.</returns>
		public override bool Equals( object obj )
		{
			if( obj == null )
				return false;

			var cd = obj as ContentDirectory;
			if( cd == null )
				return false;
			
			return cd.BaseDirectoryPath.Equals( baseDirectory, StringComparison.OrdinalIgnoreCase );
		}


		/// <summary>Returns the <see cref="BaseDirectoryPath">base directory path</see>.</summary>
		/// <returns>The <see cref="BaseDirectoryPath">base directory path</see>.</returns>
		public override string ToString()
		{
			return string.Copy( baseDirectory );
		}


	}

}