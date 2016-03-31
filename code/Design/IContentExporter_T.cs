using System;
using System.IO;


namespace ManagedX.Content.Design
{

	/// <summary>Defines a method to export content to a file or a stream.</summary>
	public interface IContentExporter<TContent> : IContentPlugin
	{

		/// <summary>Exports content to a stream.</summary>
		/// <param name="content">The content to export.</param>
		/// <param name="assetName">The asset name; this argument is mainly required to retrieve the file extension, and is used for debugging.</param>
		/// <param name="output">A stream to write the content.</param>
		void Export( TContent content, string assetName, Stream output );

	}

}