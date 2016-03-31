using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;


namespace ManagedX.Content
{
	using Design;


	/// <summary>Base class for content plug-ins.
	/// <para>This class can't be instantiated.</para>
	/// </summary>
	public abstract class ContentPlugin : IContentPlugin
	{


		private static string Normalize( string fileExtension )
		{
			if( fileExtension == null )
				throw new ArgumentNullException( "fileExtension" );

			fileExtension = fileExtension.Trim();
			if( fileExtension.Length == 0 || fileExtension.IndexOfAny( Path.GetInvalidFileNameChars() ) > -1 )
				throw new ArgumentException( "Invalid file extension.", "fileExtension" );
			
			if( fileExtension.StartsWith( ".", StringComparison.Ordinal ) )
				fileExtension = fileExtension.TrimStart( '.' );
			
			return fileExtension;
		}



		private Type contentType;
		private List<string> supportedExtensions;



		/// <summary>Internal constructor.</summary>
		/// <param name="supportedContentType">The supported content type; must not be null.</param>
		/// <param name="fileExtension">The default file extension; must not be null.</param>
		/// <param name="fileExtensions">Additional file extensions; can be null.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		internal ContentPlugin( Type supportedContentType, string fileExtension, string[] fileExtensions )
		{
			if( supportedContentType == null )
				throw new ArgumentNullException( "supportedContentType" );

			try
			{
				fileExtension = Normalize( fileExtension );
			}
			catch( ArgumentException )
			{
				throw;
			}

			
			contentType = supportedContentType;
			supportedExtensions = new List<string>();
			supportedExtensions.Add( fileExtension );
			
			if( fileExtensions != null )
			{
				for( var f = 0; f < fileExtensions.Length; f++ )
				{
					try
					{
						supportedExtensions.Add( Normalize( fileExtensions[ f ] ) );
					}
					catch( ArgumentNullException ex )
					{
						throw new ArgumentException( "Invalid file extension.", "fileExtensions", ex );
					}
					catch( ArgumentException ex )
					{
						throw new ArgumentException( "Invalid file extension.", "fileExtensions", ex );
					}
				}
			}
		}



		/// <summary>Gets the supported content type.</summary>
		public Type ContentType { get { return contentType; } }


		/// <summary>Gets a read-only collection of all supported file extensions.</summary>
		public ReadOnlyCollection<string> SupportedExtensions { get { return new ReadOnlyCollection<string>( supportedExtensions ); } }


		/// <summary>Returns a value indicating whether a file extension is supported by the content plug-in.</summary>
		/// <param name="fileExtension">A file extension.</param>
		/// <returns>Returns true if the file extension is supported, otherwise returns false.</returns>
		public bool Supports( string fileExtension )
		{
			if( fileExtension != null )
			{
				try
				{
					var normalized = Normalize( fileExtension );
					for( var x = 0; x < supportedExtensions.Count; x++ )
						if( supportedExtensions[ x ].Equals( normalized, StringComparison.OrdinalIgnoreCase ) )
							return true;
				}
				catch( ArgumentException )
				{
				}
			}
			return false;
		}


		/// <summary>Returns the type name of this <see cref="ContentPlugin"/>.</summary>
		/// <returns>Returns the type name of this <see cref="ContentPlugin"/>.</returns>
		public override string ToString()
		{
			return this.GetType().Name;
		}

	}

}