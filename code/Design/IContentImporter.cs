using System;
using System.IO;


namespace ManagedX.Content.Design
{

	/// <summary>Defines a method to import content from a file or a stream.</summary>
	public interface IContentImporter : IContentPlugIn
	{

		/// <summary>Imports content from a stream and returns it.</summary>
		/// <param name="assetName">The asset name, required to retrieve the file extension, and used for debugging.</param>
		/// <param name="input">A stream to read the content.</param>
		/// <returns>Returns the imported content.</returns>
		object Import( string assetName, Stream input );

	}


	/// <summary>Defines a method to import content from a file or a stream.</summary>
	public interface IContentImporter<TContent> : IContentImporter
	{

		/// <summary>Imports content from a stream and returns it.</summary>
		/// <param name="assetName">The asset name, required to retrieve the file extension, and used for debugging.</param>
		/// <param name="input">A stream to read the content.</param>
		/// <returns>Returns the imported content.</returns>
		new TContent Import( string assetName, Stream input );

	}

}