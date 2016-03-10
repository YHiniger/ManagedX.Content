using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;


// TODO - définir IContentReader et IContentWriter, puis redéfinir cette classe pour n'être qu'un reader; la partie writer pourra être implémentée par héritage, ou dans une classe à part.

namespace ManagedX.Framework.Content
{
	using Design;

	/// <summary>Base class for <see cref="IContentPlugIn">content plug-ins (importers/exporters)</see>.</summary>
	/// <typeparam name="TContent">Supported content type.</typeparam>
	[DebuggerStepThrough]
	public abstract class ContentPlugIn<TContent> : IContentPlugIn, IDisposable, IEquatable<ContentPlugIn<TContent>>
	{

		private Type contentType;
		private ReadOnlyCollection<string> supportedExtensions;
		private List<IDisposable> disposableContent; // FIXME - content plug-ins aren't responsible of the imported content; remove Disposable support !
		private int hashCode;



		/// <summary>Constructor.</summary>
		/// <param name="fileExtension">The main file extension (not including the leading dot).</param>
		/// <param name="fileExtensions">Additional file extensions (not including the leading dot).</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		protected ContentPlugIn( string fileExtension, params string[] fileExtensions )
		{
			if( fileExtension == null )
				throw new ArgumentNullException( "fileExtension" );

			fileExtension = fileExtension.Trim();
			if( fileExtension.Length == 0 )
				throw new ArgumentException( fileExtension, "fileExtension" );
			
			if( fileExtension.IndexOfAny( Path.GetInvalidFileNameChars() ) > -1 )
				throw new ArgumentException( fileExtension, "fileExtension" );

			var extensions = new List<string>();
			extensions.Add( fileExtension.ToUpperInvariant() );
			if( fileExtensions != null && fileExtensions.Length > 0 )
			{
				string extension;
				for( int e = 0; e < fileExtensions.Length; e++ )
				{
					extension = fileExtensions[ e ];
					if( extension == null )
						continue;

					extension = extension.Trim();
					extension = extension.TrimStart( '.' );
					if( extension.Length == 0 )
						continue;

					extension = extension.ToUpperInvariant();
					if( !extensions.Contains( extension ) )
						extensions.Add( extension );
				}
			}
			supportedExtensions = new ReadOnlyCollection<string>( extensions );

			contentType = typeof( TContent );

			disposableContent = new List<IDisposable>();

			hashCode = contentType.GetHashCode();
			foreach( string extension in supportedExtensions )
				hashCode ^= extension.ToUpperInvariant().GetHashCode();
		}

		
		/// <summary>Destructor.</summary>
		~ContentPlugIn()
		{
			this.Dispose( false );
		}



		/// <summary>Gets the supported content type.</summary>
		public Type ContentType { get { return contentType; } }


		/// <summary>Gets a read-only collection containing all supported file extensions.</summary>
		public ReadOnlyCollection<string> SupportedExtensions { get { return supportedExtensions; } }


		#region Import

		/// <summary>When overridden, imports content from a stream.</summary>
		/// <param name="fileName">The input file name.</param>
		/// <param name="input">A stream for reading the input file.</param>
		/// <returns>A newly imported content.</returns>
		protected abstract TContent ImportInternal( string fileName, Stream input );


		/// <summary>Imports content from a stream.</summary>
		/// <param name="fileName">The input file name.</param>
		/// <param name="input">A stream to read the input file.</param>
		/// <returns>A newly imported content.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="NotSupportedException"/>
		public TContent Import( string fileName, Stream input )
		{
			if( input == null )
				throw new ArgumentNullException( "input" );

			TContent output;
			try
			{
				output = this.ImportInternal( fileName, input );
			}
			catch( Exception ex )
			{
				throw new NotSupportedException( string.Format( CultureInfo.InvariantCulture, "{0}.Import(\"{1}\"): {2}", this.GetType().Name, fileName, ex.Message ), ex );
			}

			//if (!myContentType.IsValueType && output == null) throw new InvalidOperationException(this.GetType().Name + ".Import");

			IDisposable disposable = output as IDisposable;
			if( disposable != null )
				disposableContent.Add( disposable );

			return output;
		}

		#endregion


		#region Export

		/// <summary>Gets a value indicating whether the ContentPlugIn can also <see cref="Export">export content</see>.
		/// When not overridden, returns false.</summary>
		public virtual bool CanExport
		{
			get { return false; }
		}


		/// <summary>When not overridden, throws a <see cref="NotImplementedException" />.</summary>
		/// <param name="content">The content to export.</param>
		/// <param name="fileName">The name of the output file.</param>
		/// <param name="output">A stream to write the content to.</param>
		/// <exception cref="NotImplementedException"/>
		public virtual void Export( TContent content, string fileName, Stream output )
		{
			throw new NotImplementedException();
		}

		#endregion


		#region IContentPlugIn

		
		object IContentPlugIn.Import( string fileName, Stream input )
		{
			return this.Import( fileName, input );
		}


		void IContentPlugIn.ExportContent( object content, string fileName, Stream output )
		{
			if( !this.CanExport )
				throw new InvalidOperationException();

			this.Export( (TContent)content, fileName, output );
		}

		
		#endregion


		#region IDisposable


		/// <summary>Gets a value indicating whether the content importer has been disposed.</summary>
		public bool IsDisposed
		{
			get { return disposableContent == null; }
		}


		/// <summary>Disposes all imported content.</summary>
		/// <param name="disposing">True to dispose all resources, false to dispose only unmanaged resources.</param>
		protected virtual void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( disposableContent != null )
				{
					for( int d = 0; d < disposableContent.Count; d++ )
					{
						try
						{
							if( disposableContent[ d ] != null )
								disposableContent[ d ].Dispose();
						}
						catch( ObjectDisposedException )
						{
						}
					}
					disposableContent.Clear();
					disposableContent = null;
				}
			}
		}


		/// <summary>Disposes all imported content.</summary>
		public void Dispose()
		{
			this.Dispose( true );
			GC.SuppressFinalize( this );
		}


		#endregion



		/// <summary>Returns a hash code based on the supported content type and file extensions.</summary>
		/// <returns>Returns a hash code based on the supported content type and file extensions.</returns>
		public override int GetHashCode()
		{
			return hashCode;
		}


		/// <summary>Returns a value indicating whether this ContentPlugIn is equivalent to an <see cref="IContentPlugIn"/> object.</summary>
		/// <param name="other">A content plug-in.</param>
		/// <returns>Returns true if the <paramref name="other"/> ContentPlugIn is not null, has the same content type, functionalities and supported file extensions as this ContentPlugIn; otherwise, returns false.</returns>
		public bool Equals( IContentPlugIn other )
		{
			if( other == null || contentType != other.ContentType )
				return false;

			var extensions = other.SupportedExtensions;
			
			foreach( var extension in supportedExtensions )
				if( !extensions.Contains( extension ) )
					return false;
			
			foreach( var extension in extensions )
				if( !supportedExtensions.Contains( extension ) )
					return false;
			
			return true;
		}


		/// <summary>Returns a value indicating whether this ContentPlugIn is equivalent to another ContentPlugIn.</summary>
		/// <param name="other">A content plug-in.</param>
		/// <returns>Returns true if the <paramref name="other"/> ContentPlugIn is not null, has the same functionalities and supported file extensions as this ContentPlugIn.</returns>
		public virtual bool Equals( ContentPlugIn<TContent> other )
		{
			return ( other != null ) && this.Equals( (IContentPlugIn)other );
		}


		/// <summary>Returns a value indicating whether this ContentPlugIn is equivalent to an object.</summary>
		/// <param name="obj">An object.</param>
		/// <returns>Returns true if the specified object is a non null ContentPlugIn which has the same content type, functionalities and supported file extensions; otherwise, returns false.</returns>
		public override bool Equals( object obj )
		{
			return this.Equals( obj as IContentPlugIn );
		}


		/// <summary>Returns a string representing this ContentPlugIn.</summary>
		/// <returns>Returns a string representing this ContentPlugIn.</returns>
		public override string ToString()
		{
			return this.GetType().Name;
		}

	}

}