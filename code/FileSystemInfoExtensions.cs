using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;


namespace ManagedX.Content
{

	/// <summary>Provides extension methods to <see cref="FileSystemInfo"/> instances.</summary>
	public static class FileSystemInfoExtensions
	{

		[System.Security.SuppressUnmanagedCodeSecurity]
		private static class SafeNativeMethods
		{

			private const string LibraryName = "Kernel32.dll";


			/// <summary>Creates a symbolic link.</summary>
			/// <param name="symLinkFileName">The symbolic link to be created.</param>
			/// <param name="targetFileName">The name of the target for the symbolic link to be created.
			/// If targetFileName has a device name associated with it, the link is treated as an absolute link; otherwise, the link is treated as a relative link.</param>
			/// <param name="options">Indicates whether the link target, <paramref name="targetFileName"/>, is a directory.</param>
			/// <returns>
			/// If the function succeeds, the return value is nonzero.
			/// If the function fails, the return value is zero. To get extended error information, call GetLastError.
			/// </returns>
			/// <remarks>http://msdn.microsoft.com/en-us/library/aa363866%28v=vs.85%29.aspx</remarks>
			[DllImport( LibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = true, SetLastError = true )]
			[return: MarshalAs( UnmanagedType.Bool )]
			internal static extern bool CreateSymbolicLinkW(
				[In] string symLinkFileName,
				[In] string targetFileName,
				[In] int options
			);


			/// <summary>Retrieves the final path for the specified file.</summary>
			/// <param name="handle">A handle to a file or directory.</param>
			/// <param name="filePath">A pointer to a buffer that receives the path of the file or directory pointed by <paramref name="handle"/>.</param>
			/// <param name="filePathLength">The size of <paramref name="filePath"/>, in TCHARS; doesn't include the NULL termination char.</param>
			/// <param name="options">The type of result to return.</param>
			/// <returns>
			/// If the function succeeds, the return value is the length of the string received by lpszFilePath, in TCHARs. This value does not include the size of the terminating null character.
			/// If the function fails because lpszFilePath is too small to hold the string plus the terminating null character, the return value is the required buffer size, in TCHARs. This value includes the size of the terminating null character.
			/// If the function fails for any other reason, the return value is zero. To get extended error information, call GetLastError.
			/// </returns>
			/// <remarks>https://msdn.microsoft.com/en-us/library/aa364962(v=vs.85).aspx</remarks>
			[DllImport( LibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = true, SetLastError = true )]
			internal static extern int GetFinalPathNameByHandleW(
				[In] IntPtr handle,
				[Out] string filePath,
				[In] int filePathLength,
				[In] int options
			);

		}



		/// <summary>Returns a value indicating whether a file or directory is a symbolic link.</summary>
		/// <param name="self">A <see cref="FileSystemInfo"/> of an existing file/directory; must not be null.</param>
		/// <exception cref="ArgumentNullException"/>
		[Obsolete( "" )]
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


			if( self is FileInfo fileInfo )
			{
				if( !self.Exists )
					throw new FileNotFoundException();

				var buffer = new string( '\0', 512 );
				using( FileStream stream = fileInfo.Open( FileMode.Open, FileAccess.Read ) )
				{
					if( SafeNativeMethods.GetFinalPathNameByHandleW( stream.SafeFileHandle.DangerousGetHandle(), buffer, buffer.Length, 0 ) == 0 )
						throw Marshal.GetExceptionForHR( Marshal.GetLastWin32Error() );
				}

				return Path.GetDirectoryName( buffer.TrimEnd( '\0' ).Replace( @"\\?\", string.Empty ) );
			}


			if( self is DirectoryInfo directoryInfo )
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

			var principal = new WindowsPrincipal( WindowsIdentity.GetCurrent() );
			if( !principal.IsInRole( WindowsBuiltInRole.Administrator ) )
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

			if( !SafeNativeMethods.CreateSymbolicLinkW( linkName, self.FullName, isTargetDirectory ? 1 : 0 ) )
				throw Marshal.GetExceptionForHR( Marshal.GetLastWin32Error() );

			if( isTargetDirectory )
				return Directory.Exists( linkName );
			else
				return File.Exists( linkName );
		}

	}

}