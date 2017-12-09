using System;
using System.Collections.Generic;


namespace ManagedX.Content.Design
{

    /// <summary>Defines methods to properly implement a content plug-in service.</summary>
    public interface IContentPluginManager : IEnumerable<Type>
	{

		/// <summary>Returns an array of content plug-ins for the specified type.</summary>
		/// <param name="contentType">The content type; must not be null.</param>
		/// <returns>Returns an array of content plug-ins for the specified type.</returns>
		/// <exception cref="ArgumentNullException"/>
		IContentPlugin[] GetPlugins( Type contentType );

		/// <summary>Returns a read-only collection containing all <see cref="IContentPlugin"/> supporting the specified file extension.</summary>
		/// <param name="fileExtension">A file extension.</param>
		/// <returns>Returns a read-only collection containing all plug-ins for a given content type.</returns>
		IContentPlugin[] GetPlugins( string fileExtension );

		/// <summary>Returns an array of content plug-ins for the specified type.</summary>
		/// <param name="contentType">The content type; must not be null.</param>
		/// <param name="fileExtension">A file extension; must not be null.</param>
		/// <returns>Returns an array of content plug-ins for the specified type.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		IContentPlugin[] GetPlugins( Type contentType, string fileExtension );
		
		
		/// <summary>Returns an array of content importers for the specified type.</summary>
		/// <typeparam name="TContent">Content type.</typeparam>
		/// <returns>Returns an array of content importers for the specified type.</returns>
		ContentImporter<TContent>[] GetImporters<TContent>();

		/// <summary>Returns an array of content importers for the specified type.</summary>
		/// <typeparam name="TContent">Content type.</typeparam>
		/// <param name="fileExtension">A file extension.</param>
		/// <returns>Returns an array of content importers for the specified type.</returns>
		ContentImporter<TContent>[] GetImporters<TContent>( string fileExtension );



		/// <summary>Adds a content plug-in to the manager.</summary>
		/// <param name="plugin">A content plug-in.</param>
		void Add( IContentPlugin plugin );

		
		/// <summary>Removes a content plug-in from the manager.</summary>
		/// <param name="plugin">A content plug-in.</param>
		void Remove( IContentPlugin plugin );

	}

}