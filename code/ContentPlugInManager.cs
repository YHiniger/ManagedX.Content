using System;
using System.Collections.Generic;


namespace ManagedX.Content
{
	using Design;


	/// <summary>A content plug-in (see <see cref="IContentPlugIn"/>) manager.</summary>
	public sealed class ContentPlugInManager : IContentPlugInManager, IDisposable
	{

		private sealed class PlugInList : List<IContentPlugIn>
		{

			internal PlugInList()
				: base()
			{
			}

		}

		private sealed class ImporterList : List<IContentImporter>
		{

			internal ImporterList()
				: base()
			{
			}

		}



		private Dictionary<Type, PlugInList> plugIns;
		private Dictionary<Type, ImporterList> importers;



		#region Constructor, finalizer
		
		/// <summary>Instantiates a new <see cref="ContentPlugInManager"/>.</summary>
		public ContentPlugInManager()
		{
			plugIns = new Dictionary<Type, PlugInList>();
			importers = new Dictionary<Type, ImporterList>();
		}


		/// <summary>Finalizer.</summary>
		~ContentPlugInManager()
		{
			this.Dispose( false );
		}

		#endregion Constructor, finalizer



		#region GetPlugIns

		/// <summary>Returns an array of content plug-ins for the specified type.</summary>
		/// <param name="contentType">The content type; must not be null.</param>
		/// <returns>Returns an array of content plug-ins for the specified type.</returns>
		/// <exception cref="ArgumentNullException"/>
		public IContentPlugIn[] GetPlugIns( Type contentType )
		{
			if( contentType == null )
				throw new ArgumentNullException( "contentType" );

			var list = new PlugInList();

			ImporterList importerList;
			if( importers.TryGetValue( contentType, out importerList ) && importerList != null )
			{
				for( var p = 0; p < importerList.Count; p++ )
					list.Add( importerList[ p ] );
			}

			var output = new IContentPlugIn[ list.Count ];
			list.CopyTo( output, 0 );
			list.Clear();
			return output;
		}


		/// <summary>Returns a read-only collection containing all registered plug-ins for the specified file extension.</summary>
		/// <param name="fileExtension">A file extension.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		public IContentPlugIn[] GetPlugIns( string fileExtension )
		{
			if( fileExtension == null )
				throw new ArgumentNullException( "fileExtension" );
			
			var list = new List<IContentPlugIn>();
			foreach( var plugInList in plugIns.Values )
			{
				for( var i = 0; i < plugInList.Count; i++ )
				{
					try
					{
						if( plugInList[ i ].Supports( fileExtension ) )
							list.Add( plugInList[ i ] );
					}
					catch( ArgumentException ex )
					{
						throw new ArgumentException( "Invalid file extension.", "fileExtension", ex );
					}
				}
			}

			var output = new IContentPlugIn[ list.Count ];
			list.CopyTo( output, 0 );
			list.Clear();
			return output;
		}


		/// <summary>Returns an array of content plug-ins for the specified type.</summary>
		/// <param name="contentType">The content type; must not be null.</param>
		/// <param name="fileExtension">A file extension; must not be null.</param>
		/// <returns>Returns an array of content plug-ins for the specified type.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		public IContentPlugIn[] GetPlugIns( Type contentType, string fileExtension )
		{
			if( contentType == null )
				throw new ArgumentNullException( "contentType" );

			if( fileExtension == null )
				throw new ArgumentNullException( "fileExtension" );

			var list = new PlugInList();

			ImporterList importerList;
			if( importers.TryGetValue( contentType, out importerList ) && importerList != null )
			{
				for( var p = 0; p < importerList.Count; p++ )
				{
					try
					{
						if( importerList[ p ].Supports( fileExtension ) )
							list.Add( importerList[ p ] );
					}
					catch( ArgumentException ex )
					{
						throw new ArgumentException( "Invalid file extension.", "fileExtension", ex );
					}
				}
			}

			var output = new IContentPlugIn[ list.Count ];
			list.CopyTo( output, 0 );
			list.Clear();
			return output;
		}

		#endregion GetPlugIns


		#region GetImporters

		/// <summary>Returns an array of content importers for the specified type.</summary>
		/// <typeparam name="TContent">Content type.</typeparam>
		/// <returns>Returns an array of content importers for the specified type.</returns>
		public IContentImporter<TContent>[] GetImporters<TContent>()
		{
			var list = new List<IContentImporter<TContent>>();
			
			ImporterList importerList;
			if( importers.TryGetValue( typeof( TContent ), out importerList ) && importerList != null )
			{
				for( var p = 0; p < importerList.Count; p++ )
				{
					var importer = importerList[ p ] as IContentImporter<TContent>;
					if( importer != null )
						list.Add( importer );
				}
			}

			var output = new IContentImporter<TContent>[ list.Count ];
			list.CopyTo( output, 0 );
			list.Clear();
			return output;
		}


		/// <summary>Returns an array of content importers for the specified type.</summary>
		/// <typeparam name="TContent">Content type.</typeparam>
		/// <param name="fileExtension">A file extension.</param>
		/// <returns>Returns an array of content importers for the specified type.</returns>
		public IContentImporter<TContent>[] GetImporters<TContent>( string fileExtension )
		{
			var list = new List<IContentImporter<TContent>>();

			ImporterList importerList;
			if( importers.TryGetValue( typeof( TContent ), out importerList ) && importerList != null )
			{
				for( var p = 0; p < importerList.Count; p++ )
				{
					var importer = importerList[ p ] as IContentImporter<TContent>;
					if( importer != null && importer.Supports( fileExtension ) )
						list.Add( importer );
				}
			}

			var output = new IContentImporter<TContent>[ list.Count ];
			list.CopyTo( output, 0 );
			list.Clear();
			return output;
		}

		#endregion GetImporters


		/// <summary>Registers a content plug-in.</summary>
		/// <param name="plugIn">A content plug-in.</param>
		/// <exception cref="ArgumentNullException"/>
		public void Add( IContentPlugIn plugIn )
		{
			if( plugIn == null )
				throw new ArgumentNullException( "plugIn" );

			PlugInList list;
			if( !plugIns.TryGetValue( plugIn.ContentType, out list ) || list == null )
			{
				list = new PlugInList();
				plugIns.Add( plugIn.ContentType, list );
			}

			if( !list.Contains( plugIn ) )
				list.Add( plugIn );

			var importer = plugIn as IContentImporter;
			if( importer != null )
			{
				ImporterList importerList;
				if( !importers.TryGetValue( importer.ContentType, out importerList ) || importerList == null )
				{
					importerList = new ImporterList();
					importers.Add( importer.ContentType, importerList );
				}

				if( !importerList.Contains( importer ) )
					importerList.Add( importer );
			}
		}


		/// <summary>Removes a content plug-in from the container.</summary>
		/// <param name="plugIn">A content plug-in.</param>
		/// <exception cref="ArgumentNullException"/>
		public void Remove( IContentPlugIn plugIn )
		{
			if( plugIn == null )
				throw new ArgumentNullException( "plugIn" );

			PlugInList list;
			if( plugIns.TryGetValue( plugIn.ContentType, out list ) && list != null )
			{
				list.Remove( plugIn );

				if( list.Count == 0 )
					plugIns.Remove( plugIn.ContentType );

				var importer = plugIn as IContentImporter;
				if( importer != null )
				{
					ImporterList importerList;
					if( importers.TryGetValue( importer.ContentType, out importerList ) && importerList != null )
					{
						importerList.Remove( importer );
						if( importerList.Count == 0 )
							importers.Remove( importer.ContentType );
					}
				}
			}
		}


		#region IEnumerable

		/// <summary>Returns an enumerator which iterates through the list of supported content types.</summary>
		public IEnumerator<Type> GetEnumerator()
		{
			return plugIns.Keys.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion IEnumerable


		#region IDisposable

		/// <summary>Releases unmanaged, and optionally managed resources allocated by this <see cref="ContentPlugInManager"/>.</summary>
		/// <param name="disposing">true to dispose all resources, false to release unmanaged resources only.</param>
		private void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( importers != null )
				{
					importers.Clear();
					importers = null;
				}

				if( plugIns != null )
				{
					IDisposable disposable;
					foreach( var list in plugIns.Values )
					{
						while( list.Count > 0 )
						{
							disposable = list[ 0 ] as IDisposable;
							if( disposable != null )
							{
								try
								{
									disposable.Dispose();
								}
								catch( ObjectDisposedException )
								{
								}
							}
							list.RemoveAt( 0 );
						}
					}
					plugIns.Clear();
					plugIns = null;
				}
			}
		}


		/// <summary>Releases all resources allocated by this <see cref="ContentPlugInManager"/>.</summary>
		public void Dispose()
		{
			this.Dispose( true );
			GC.SuppressFinalize( this );
		}

		#endregion IDisposable

	}

}