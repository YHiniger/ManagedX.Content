using System;


namespace ManagedX.Content
{
	using Design;


	/// <summary>Base class for content importers.
	/// <para>Inherits from <see cref="ContentPlugin"/>.</para>
	/// </summary>
	/// <typeparam name="TContent">Supported content type.</typeparam>
	public abstract class ContentImporter<TContent> : ContentPlugin, IContentImporter<TContent>
	{


		/// <summary>Initializes a new content importer.</summary>
		/// <param name="fileExtension">The default file extension; must not be null.</param>
		/// <param name="fileExtensions">Additional file extensions; can be null.</param>
		/// <exception cref="ArgumentNullException"/>
		protected ContentImporter( string fileExtension, params string[] fileExtensions )
			: base( typeof( TContent ), fileExtension, fileExtensions )
		{
		}



		/// <summary>Imports content from a stream or a file.</summary>
		/// <param name="assetName">The asset name; must not be null.</param>
		/// <param name="input">The input stream; must not be null.</param>
		/// <returns>Returns the imported content.</returns>
		public abstract TContent Import( string assetName, System.IO.Stream input );


		object IContentImporter.Import( string assetName, System.IO.Stream input )
		{
			return this.Import( assetName, input );
		}

	}

}