using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;


namespace ManagedX.Content
{

	/// <summary>Provides extension methods to <see cref="FileSystemInfo"/> instances.</summary>
	public static class FileSystemInfoExtensions
	{

		[System.Security.SuppressUnmanagedCodeSecurity]
		private static class NativeMethods
		{

			private const string LibraryName = "kernel32.dll";


			/// <summary>Creates a symbolic link.</summary>
			/// <param name="symLinkFileName">The symbolic link to be created.</param>
			/// <param name="targetFileName">The name of the target for the symbolic link to be created.
			/// If targetFileName has a device name associated with it, the link is treated as an absolute link; otherwise, the link is treated as a relative link.</param>
			/// <param name="flags">Indicates whether the link target, <paramref name="targetFileName"/>, is a directory.</param>
			/// <remarks>http://msdn.microsoft.com/en-us/library/aa363866%28v=vs.85%29.aspx</remarks>
			[DllImport( LibraryName, EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode, SetLastError = true )]
			[return: MarshalAs( UnmanagedType.Bool )]
			internal static extern bool CreateSymbolicLink(
				[In]string symLinkFileName,
				[In]string targetFileName,
				[In]int flags
			);


			/// <summary>Retrieves the final path for the specified file.</summary>
			/// <param name="handle">A handle to a file or directory.</param>
			/// <param name="filePath">A pointer to a buffer that receives the path of the file or directory pointed by <paramref name="handle"/>.</param>
			/// <param name="filePathLength">The size of <paramref name="filePath"/>, in TCHARS; doesn't include the NULL termination char.</param>
			/// <param name="flags">The type of result to return.</param>
			/// <returns>Returns an HRESULT.</returns>
			[DllImport( LibraryName, EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true )]
			internal static extern int GetFinalPathNameByHandle(
				[In]IntPtr handle,
				[Out]string filePath,
				[In]int filePathLength,
				[In]int flags
			);

		}



		/// <summary>Returns a value indicating whether a file or directory is a symbolic link.</summary>
		/// <param name="self">A <see cref="FileSystemInfo"/> of an existing file/directory; must not be null.</param>
		/// <exception cref="ArgumentNullException"/>
		public static bool IsSymbolicLink( this FileSystemInfo self )
		{
			if( self == null )
				throw new ArgumentNullException( "self" );

			return self.Attributes.HasFlag( FileAttributes.ReparsePoint );
		}


		/// <summary>Returns the target of a symbolic link.</summary>
		/// <param name="self">A <see cref="FileSystemInfo"/> of an existing file/directory which carries the <see cref="FileAttributes.ReparsePoint"/> attribute; must not be null.</param>
		/// <returns>Returns the target of a symbolic link.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="FileNotFoundException"/>
		/// <exception cref="DirectoryNotFoundException"/>
		/// <exception cref="Exception"/>
		public static string GetSymbolicLinkTarget( this FileSystemInfo self )
		{
			if( self == null )
				throw new ArgumentNullException( "self" );

			if( !self.Attributes.HasFlag( FileAttributes.ReparsePoint ) )
				throw new ArgumentException( "The specified file/directory is not a symbolic link.", "self" );


			var fileInfo = self as FileInfo;
			if( fileInfo != null )
			{
				if( !self.Exists )
					throw new FileNotFoundException();

				var buffer = new string( '\0', 512 );
				int errCode = 0;
				using( FileStream stream = fileInfo.Open( FileMode.Open, FileAccess.Read ) )
				{
					if( NativeMethods.GetFinalPathNameByHandle( stream.SafeFileHandle.DangerousGetHandle(), buffer, buffer.Length, 0 ) == 0 )
						errCode = Marshal.GetLastWin32Error();
				}
				if( errCode != 0 )
				{
					var exception = Marshal.GetExceptionForHR( errCode );
					if( exception != null )
						throw exception;
					else
						throw new InvalidOperationException();
				}

				return Path.GetDirectoryName( buffer.TrimEnd( '\0' ).Replace( @"\\?\", string.Empty ) );
			}


			var directoryInfo = self as DirectoryInfo;
			if( directoryInfo != null )
			{
				if( !self.Exists )
					throw new DirectoryNotFoundException();

				string target;
				var files = directoryInfo.GetFiles();

				if( files.Length > 0 )
					target = GetSymbolicLinkTarget( files[ 0 ] );
				else
				{
					var fInfo = new FileInfo( Path.Combine( self.FullName, "__dummy__" ) );
					using( FileStream stream = fInfo.Create() )
					{
					}
					target = GetSymbolicLinkTarget( fInfo );
					fInfo.Delete();
				}
				return target;
			}


			throw new ArgumentException( "Unknown filesystem info type.", "self" );
		}


		/// <summary>Creates a symbolic link to the file/directory.</summary>
		/// <param name="self">A <see cref="FileSystemInfo"/> of an existing file/directory; must not be null.</param>
		/// <param name="linkName">The name of the symbolic link to create.</param>
		/// <returns>Returns true on success, otherwise returns false.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="NotSupportedException"/>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="DirectoryNotFoundException"/>
		/// <exception cref="FileNotFoundException"/>
		/// <exception cref="Exception"/>
		public static bool CreateSymbolicLink( this FileSystemInfo self, string linkName )
		{
			if( linkName == null )
				throw new ArgumentNullException( "linkName" );

			linkName = linkName.Trim();
			if( linkName.Length == 0 || linkName.IndexOfAny( Path.GetInvalidPathChars() ) > -1 )
				throw new ArgumentException( "Invalid link name.", "linkName" );


			if( self == null )
				throw new ArgumentNullException( "self" );


			if( Environment.OSVersion.Version.Major < 6 )
				throw new NotSupportedException( "Windows Vista or newer required." );

			var principal = new System.Security.Principal.WindowsPrincipal( System.Security.Principal.WindowsIdentity.GetCurrent() );
			if( !principal.IsInRole( System.Security.Principal.WindowsBuiltInRole.Administrator ) )
				throw new InvalidOperationException( "The application must be run with administrator privileges." );

			var isTargetDirectory = self is DirectoryInfo;

			if( !self.Exists )
			{
				if( isTargetDirectory )
					throw new DirectoryNotFoundException();
				else
					throw new FileNotFoundException();
			}


			if( isTargetDirectory )
			{
				if( Directory.Exists( linkName ) )
					return false;
			}
			else if( File.Exists( linkName ) )
				return false;


			if( !NativeMethods.CreateSymbolicLink( linkName, self.FullName, isTargetDirectory ? 1 : 0 ) )
			{
				var errCode = Marshal.GetLastWin32Error();
				var exception = Marshal.GetExceptionForHR( errCode );
				throw exception ?? new InvalidOperationException();
			}


			if( isTargetDirectory )
				return Directory.Exists( linkName );
			else
				return File.Exists( linkName );
		}

	}

}