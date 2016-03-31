using System;
using System.Collections.ObjectModel;


namespace ManagedX.Content.Design
{

	/// <summary>Defines properties and a method to properly implement a content plug-in.</summary>
	public interface IContentPlugin
	{

		/// <summary>Gets the supported content type.</summary>
		Type ContentType { get; }


		/// <summary>Gets a read-only collection containing all supported file extensions.</summary>
		ReadOnlyCollection<string> SupportedExtensions { get; }


		/// <summary>Returns a value indicating whether the content plug-in supports a file extension.</summary>
		/// <param name="fileExtension">A file extension.</param>
		/// <returns></returns>
		bool Supports( string fileExtension );

	}

}