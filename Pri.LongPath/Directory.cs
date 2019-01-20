// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "Directory.cs" belongs to Protiguous@Protiguous.com and
// Rick@AIBrain.org unless otherwise specified or the original license has
// been overwritten by formatting.
// (We try to avoid it from happening, but it does accidentally happen.)
//
// Any unmodified portions of source code gleaned from other projects still retain their original
// license and our thanks goes to those Authors. If you find your code in this source code, please
// let us know so we can properly attribute you and include the proper license and/or copyright.
//
// If you want to use any of our code, you must contact Protiguous@Protiguous.com or
// Sales@AIBrain.org for permission and a quote.
//
// Donations are accepted (for now) via
//     bitcoin:1Mad8TxTqxKnMiHuZxArFvX8BuFEB9nqX2
//     paypal@AIBrain.Org
//     (We're still looking into other solutions! Any ideas?)
//
// =========================================================
// Disclaimer:  Usage of the source code or binaries is AS-IS.
//    No warranties are expressed, implied, or given.
//    We are NOT responsible for Anything You Do With Our Code.
//    We are NOT responsible for Anything You Do With Our Executables.
//    We are NOT responsible for Anything You Do With Your Computer.
// =========================================================
//
// Contact us by email if you have any questions, helpful criticism, or if you would like to use our code in your project(s).
// For business inquiries, please contact me at Protiguous@Protiguous.com
//
// Our website can be found at "https://Protiguous.com/"
// Our software can be found at "https://Protiguous.Software/"
// Our GitHub address is "https://github.com/Protiguous".
// Feel free to browse any source code we *might* make available.
//
// Project: "Pri.LongPath", "Directory.cs" was last formatted by Protiguous on 2019/01/12 at 8:25 PM.

namespace Pri.LongPath {

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using JetBrains.Annotations;
    using Microsoft.Win32.SafeHandles;

    public static class Directory {

        [NotNull]
        private static DirectoryInfo CreateDirectoryUnc( [NotNull] String path ) {
            if ( String.IsNullOrWhiteSpace( value: path ) ) {
                throw new ArgumentException( message: "Value cannot be null or whitespace.", paramName: nameof( path ) );
            }

            var length = path.Length;

            if ( length >= 2 && path[ length - 1 ].IsDirectorySeparator() ) {
                --length;
            }

            var rootLength = path.GetRootLength();

            var pathComponents = new List<String>();

            if ( length > rootLength ) {
                for ( var index = length - 1; index >= rootLength; --index ) {
                    var subPath = path.Substring( 0, index + 1 );

                    if ( !subPath.Exists() ) {
                        pathComponents.Add( subPath );
                    }

                    while ( index > rootLength && path[ index ] != System.IO.Path.DirectorySeparatorChar && path[ index ] != System.IO.Path.AltDirectorySeparatorChar ) {
                        --index;
                    }
                }
            }

            while ( pathComponents.Count > 0 ) {
                var str = pathComponents[ pathComponents.Count - 1 ].NormalizeLongPath();
                pathComponents.RemoveAt( pathComponents.Count - 1 );

                if ( str.Exists() || str.CreateDirectory( IntPtr.Zero ) ) {
                    continue;
                }

                // To mimic Directory.CreateDirectory, we don't throw if the directory (not a file) already exists
                var errorCode = Marshal.GetLastWin32Error();

                if ( errorCode != NativeMethods.ERROR_ALREADY_EXISTS || !path.Exists() ) {
                    throw Common.GetExceptionFromWin32Error( errorCode );
                }
            }

            return new DirectoryInfo( path );
        }

        [ItemNotNull]
        private static IEnumerable<String> EnumerateFileSystemIterator( [NotNull] String normalizedPath, [NotNull] String normalizedSearchPattern, Boolean includeDirectories,
            Boolean includeFiles ) {
            if ( String.IsNullOrWhiteSpace( value: normalizedPath ) ) {
                throw new ArgumentException( message: "Value cannot be null or whitespace.", paramName: nameof( normalizedPath ) );
            }

            // NOTE: Any exceptions thrown from this method are thrown on a call to IEnumerator<string>.MoveNext()

            var path = normalizedPath.IsPathUnc() ? normalizedPath : normalizedPath.RemoveLongPathPrefix();

            using ( var handle = BeginFind( normalizedPath.Combine( normalizedSearchPattern ), out var findData ) ) {
                if ( handle == null ) {
                    yield break;
                }

                do {
                    if ( findData.dwFileAttributes.IsDirectory() ) {
                        if ( findData.cFileName.IsCurrentOrParentDirectory() ) {
                            continue;
                        }

                        if ( includeDirectories ) {
                            yield return path.RemoveLongPathPrefix().Combine( findData.cFileName );
                        }
                    }
                    else {
                        if ( includeFiles ) {
                            yield return path.RemoveLongPathPrefix().Combine( findData.cFileName );
                        }
                    }
                } while ( handle.FindNextFile( out findData ) );

                var errorCode = Marshal.GetLastWin32Error();

                if ( errorCode != NativeMethods.ERROR_NO_MORE_FILES ) {
                    throw Common.GetExceptionFromWin32Error( errorCode );
                }
            }
        }

        private static IEnumerable<String> EnumerateFileSystemIteratorRecursive( String normalizedPath, String normalizedSearchPattern, Boolean includeDirectories,
            Boolean includeFiles ) {

            // NOTE: Any exceptions thrown from this method are thrown on a call to IEnumerator<string>.MoveNext()
            var pendingDirectories = new Queue<String>();
            pendingDirectories.Enqueue( normalizedPath );

            while ( pendingDirectories.Count > 0 ) {
                normalizedPath = pendingDirectories.Dequeue();

                // get all subdirs to recurse in the next iteration
                foreach ( var subdir in EnumerateNormalizedFileSystemEntries( true, false, SearchOption.TopDirectoryOnly, normalizedPath, "*" ) ) {
                    pendingDirectories.Enqueue( subdir.NormalizeLongPath() );
                }

                var path = normalizedPath.IsPathUnc() ? normalizedPath : normalizedPath.RemoveLongPathPrefix();

                using ( var handle = BeginFind( normalizedPath.Combine( normalizedSearchPattern ), out var findData ) ) {
                    if ( handle == null ) {
                        continue;
                    }

                    do {
                        var fullPath = path.Combine( findData.cFileName );

                        if ( findData.dwFileAttributes.IsDirectory() ) {
                            if ( findData.cFileName.IsCurrentOrParentDirectory() ) {
                                continue;
                            }

                            var fullNormalizedPath = normalizedPath.Combine( findData.cFileName );

                            Debug.Assert( fullPath.Exists() );
                            Debug.Assert( ( fullNormalizedPath.IsPathUnc() ? fullNormalizedPath : fullNormalizedPath.RemoveLongPathPrefix() ).Exists() );

                            if ( includeDirectories ) {
                                yield return fullPath.RemoveLongPathPrefix();
                            }
                        }
                        else if ( includeFiles ) {
                            yield return fullPath.RemoveLongPathPrefix();
                        }
                    } while ( handle.FindNextFile( out findData ) );

                    var errorCode = Marshal.GetLastWin32Error();

                    if ( errorCode != NativeMethods.ERROR_NO_MORE_FILES ) {
                        throw Common.GetExceptionFromWin32Error( errorCode );
                    }
                }
            }
        }

        private static IEnumerable<String> EnumerateNormalizedFileSystemEntries( Boolean includeDirectories, Boolean includeFiles, SearchOption option,
            [NotNull] String normalizedPath, String normalizedSearchPattern ) {

            // First check whether the specified path refers to a directory and exists
            var errorCode = normalizedPath.TryGetDirectoryAttributes( out var attributes );

            if ( errorCode != 0 ) {
                throw Common.GetExceptionFromWin32Error( errorCode );
            }

            if ( option == SearchOption.AllDirectories ) {
                return EnumerateFileSystemIteratorRecursive( normalizedPath, normalizedSearchPattern, includeDirectories, includeFiles );
            }

            return EnumerateFileSystemIterator( normalizedPath, normalizedSearchPattern, includeDirectories, includeFiles );
        }

        private static Boolean IsCurrentOrParentDirectory( [NotNull] this String directoryName ) {
            if ( String.IsNullOrEmpty( value: directoryName ) ) {
                throw new ArgumentException( message: "Value cannot be null or empty.", paramName: nameof( directoryName ) );
            }

            return directoryName.Equals( ".", StringComparison.OrdinalIgnoreCase ) || directoryName.Equals( "..", StringComparison.OrdinalIgnoreCase );
        }

        [CanBeNull]
        public static SafeFindHandle BeginFind( String normalizedPathWithSearchPattern, out NativeMethods.WIN32_FIND_DATA findData ) {
            normalizedPathWithSearchPattern = normalizedPathWithSearchPattern.TrimEnd( '\\' );
            var handle = NativeMethods.FindFirstFile( normalizedPathWithSearchPattern, out findData );

            if ( !handle.IsInvalid ) {
                return handle;
            }

            var errorCode = Marshal.GetLastWin32Error();

            if ( errorCode != NativeMethods.ERROR_FILE_NOT_FOUND && errorCode != NativeMethods.ERROR_PATH_NOT_FOUND && errorCode != NativeMethods.ERROR_NOT_READY ) {
                throw Common.GetExceptionFromWin32Error( errorCode );
            }

            return null;
        }

        /// <summary>
        ///     Creates the specified directory.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the directory to create.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white
        ///     space, or contains one or more invalid characters as defined in
        ///     <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///     <paramref name="path" /> contains one or more directories that could not be
        ///     found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///     <paramref name="path" /> is a file.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        /// <remarks>
        ///     Note: Unlike <see cref="Directory.CreateDirectory(System.String)" />, this method only creates
        ///     the last directory in <paramref name="path" />.
        /// </remarks>
        [NotNull]
        public static DirectoryInfo CreateDirectory( [NotNull] this String path ) {
            if ( String.IsNullOrWhiteSpace( value: path ) ) {
                throw new ArgumentException( message: "Value cannot be null or whitespace.", paramName: nameof( path ) );
            }

            if ( path.IsPathUnc() ) {
                return CreateDirectoryUnc( path );
            }

            var normalizedPath = path.NormalizeLongPath();
            var fullPath = normalizedPath.RemoveLongPathPrefix();

            var length = fullPath.Length;

            if ( length >= 2 && fullPath[ length - 1 ].IsDirectorySeparator() ) {
                --length;
            }

            var rootLength = fullPath.GetRootLength();

            var pathComponents = new List<String>();

            if ( length > rootLength ) {
                for ( var index = length - 1; index >= rootLength; --index ) {
                    var subPath = fullPath.Substring( 0, index + 1 );

                    if ( !subPath.Exists() ) {
                        pathComponents.Add( subPath.NormalizeLongPath() );
                    }

                    while ( index > rootLength && fullPath[ index ].IsDirectorySeparator() ) {
                        --index;
                    }
                }
            }

            while ( pathComponents.Count > 0 ) {
                var str = pathComponents[ pathComponents.Count - 1 ];
                pathComponents.RemoveAt( pathComponents.Count - 1 );

                if ( str.CreateDirectory( IntPtr.Zero ) ) {
                    continue;
                }

                // To mimic Directory.CreateDirectory, we don't throw if the directory (not a file) already exists
                var errorCode = Marshal.GetLastWin32Error();

                if ( errorCode != NativeMethods.ERROR_ALREADY_EXISTS || !path.Exists() ) {
                    throw Common.GetExceptionFromWin32Error( errorCode );
                }
            }

            return new DirectoryInfo( fullPath );
        }

        [NotNull]
        public static DirectoryInfo CreateDirectory( String path, [NotNull] DirectorySecurity directorySecurity ) {
            if ( directorySecurity == null ) {
                throw new ArgumentNullException( paramName: nameof( directorySecurity ) );
            }

            Common.ThrowIfBlank( ref path );
            path.CreateDirectory();
            SetAccessControl( path, directorySecurity );

            return new DirectoryInfo( path );
        }

        public static void Delete( String path, Boolean recursive ) {

            /* MSDN: https://msdn.microsoft.com/en-us/library/fxeahc5f.aspx
			   The behavior of this method differs slightly when deleting a directory that contains a reparse point,
			   such as a symbolic link or a mount point.
			   (1) If the reparse point is a directory, such as a mount point, it is unmounted and the mount point is deleted.
			   This method does not recurse through the reparse point.
			   (2) If the reparse point is a symbolic link to a file, the reparse point is deleted and not the target of
			   the symbolic link.
			*/

            try {
                const FileAttributes reparseFlags = FileAttributes.Directory | FileAttributes.ReparsePoint;
                var isDirectoryReparsePoint = path.GetAttributes().HasFlag( reparseFlags );

                if ( isDirectoryReparsePoint ) {
                    Delete( path );

                    return;
                }
            }
            catch ( FileNotFoundException ) {

                // ignore: not there when we try to delete, it doesn't matter
            }

            if ( recursive == false ) {
                Delete( path );

                return;
            }

            try {
                foreach ( var file in EnumerateFileSystemEntries( path, "*", false, true, SearchOption.TopDirectoryOnly ) ) {
                    File.Delete( file );
                }
            }
            catch ( FileNotFoundException ) {

                // ignore: not there when we try to delete, it doesn't matter
            }

            try {
                foreach ( var subPath in EnumerateFileSystemEntries( path, "*", true, false, SearchOption.TopDirectoryOnly ) ) {
                    Delete( subPath, true );
                }
            }
            catch ( FileNotFoundException ) {

                // ignore: not there when we try to delete, it doesn't matter
            }

            try {
                Delete( path );
            }
            catch ( FileNotFoundException ) {

                // ignore: not there when we try to delete, it doesn't matter
            }
        }

        /// <summary>
        ///     Deletes the specified empty directory.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the directory to delete.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white
        ///     space, or contains one or more invalid characters as defined in
        ///     <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///     <paramref name="path" /> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> refers to a directory that is read-only.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///     <paramref name="path" /> is a file.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> refers to a directory that is not empty.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> refers to a directory that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        public static void Delete( [NotNull] String path ) {

            var normalizedPath = path.NormalizeLongPath();

            if ( !NativeMethods.RemoveDirectory( normalizedPath ) ) {
                throw Common.GetExceptionFromLastWin32Error();
            }
        }

        /// <summary>
        ///     Returns a enumerable containing the directory names of the specified directory.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the directory to search.
        /// </param>
        /// <returns>
        ///     A <see cref="IEnumerable{T}" /> containing the directory names within <paramref name="path" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white
        ///     space, or contains one or more invalid characters as defined in
        ///     <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///     <paramref name="path" /> contains one or more directories that could not be
        ///     found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///     <paramref name="path" /> is a file.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        public static IEnumerable<String> EnumerateDirectories( [NotNull] String path ) => EnumerateFileSystemEntries( path, "*", true, false, SearchOption.TopDirectoryOnly );

        /// <summary>
        ///     Returns a enumerable containing the directory names of the specified directory that
        ///     match the specified search pattern.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the directory to search.
        /// </param>
        /// <param name="searchPattern">
        ///     A <see cref="String" /> containing search pattern to match against the names of the
        ///     directories in <paramref name="path" />, otherwise, <see langword="null" /> or an empty
        ///     string ("") to use the default search pattern, "*".
        /// </param>
        /// <returns>
        ///     A <see cref="IEnumerable{T}" /> containing the directory names within <paramref name="path" />
        ///     that match <paramref name="searchPattern" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white
        ///     space, or contains one or more invalid characters as defined in
        ///     <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///     <paramref name="path" /> contains one or more directories that could not be
        ///     found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///     <paramref name="path" /> is a file.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        public static IEnumerable<String> EnumerateDirectories( [NotNull] String path, [NotNull] String searchPattern ) =>
            EnumerateFileSystemEntries( path, searchPattern, true, false, SearchOption.TopDirectoryOnly );

        public static IEnumerable<String> EnumerateDirectories( [NotNull] String path, [NotNull] String searchPattern, SearchOption options ) =>
            EnumerateFileSystemEntries( path, searchPattern, true, false, options );

        /// <summary>
        ///     Returns a enumerable containing the file names of the specified directory.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the directory to search.
        /// </param>
        /// <returns>
        ///     A <see cref="IEnumerable{T}" /> containing the file names within <paramref name="path" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white
        ///     space, or contains one or more invalid characters as defined in
        ///     <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///     <paramref name="path" /> contains one or more directories that could not be
        ///     found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///     <paramref name="path" /> is a file.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        public static IEnumerable<String> EnumerateFiles( [NotNull] String path ) => EnumerateFileSystemEntries( path, "*", false, true, SearchOption.TopDirectoryOnly );

        public static IEnumerable<String> EnumerateFiles( [NotNull] String path, [NotNull] String searchPattern, SearchOption options ) =>
            EnumerateFileSystemEntries( path, searchPattern, false, true, options );

        /// <summary>
        ///     Returns a enumerable containing the file names of the specified directory that
        ///     match the specified search pattern.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the directory to search.
        /// </param>
        /// <param name="searchPattern">
        ///     A <see cref="String" /> containing search pattern to match against the names of the
        ///     files in <paramref name="path" />, otherwise, <see langword="null" /> or an empty
        ///     string ("") to use the default search pattern, "*".
        /// </param>
        /// <returns>
        ///     A <see cref="IEnumerable{T}" /> containing the file names within <paramref name="path" />
        ///     that match <paramref name="searchPattern" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white
        ///     space, or contains one or more invalid characters as defined in
        ///     <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///     <paramref name="path" /> contains one or more directories that could not be
        ///     found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///     <paramref name="path" /> is a file.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        public static IEnumerable<String> EnumerateFiles( [NotNull] String path, [NotNull] String searchPattern ) =>
            EnumerateFileSystemEntries( path, searchPattern, false, true, SearchOption.TopDirectoryOnly );

        /// <summary>
        ///     Returns a enumerable containing the file and directory names of the specified directory.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the directory to search.
        /// </param>
        /// <returns>
        ///     A <see cref="IEnumerable{T}" /> containing the file and directory names within
        ///     <paramref name="path" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white
        ///     space, or contains one or more invalid characters as defined in
        ///     <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///     <paramref name="path" /> contains one or more directories that could not be
        ///     found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///     <paramref name="path" /> is a file.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        public static IEnumerable<String> EnumerateFileSystemEntries( [NotNull] String path ) =>
            EnumerateFileSystemEntries( path, null, true, true, SearchOption.TopDirectoryOnly );

        /// <summary>
        ///     Returns a enumerable containing the file and directory names of the specified directory
        ///     that match the specified search pattern.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the directory to search.
        /// </param>
        /// <param name="searchPattern">
        ///     A <see cref="String" /> containing search pattern to match against the names of the
        ///     files and directories in <paramref name="path" />, otherwise, <see langword="null" />
        ///     or an empty string ("") to use the default search pattern, "*".
        /// </param>
        /// <returns>
        ///     A <see cref="IEnumerable{T}" /> containing the file and directory names within
        ///     <paramref name="path" />that match <paramref name="searchPattern" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white
        ///     space, or contains one or more invalid characters as defined in
        ///     <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///     <paramref name="path" /> contains one or more directories that could not be
        ///     found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///     <paramref name="path" /> is a file.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        public static IEnumerable<String> EnumerateFileSystemEntries( [NotNull] String path, [NotNull] String searchPattern ) =>
            EnumerateFileSystemEntries( path, searchPattern, true, true, SearchOption.TopDirectoryOnly );

        public static IEnumerable<String> EnumerateFileSystemEntries( [NotNull] String path, [NotNull] String searchPattern, SearchOption options ) =>
            EnumerateFileSystemEntries( path, searchPattern, true, true, options );

        public static IEnumerable<String> EnumerateFileSystemEntries( [NotNull] String path, [NotNull] String searchPattern, Boolean includeDirectories, Boolean includeFiles,
            SearchOption option ) {
            var normalizedSearchPattern = searchPattern.NormalizeSearchPattern();
            var normalizedPath = path.NormalizeLongPath();

            return EnumerateNormalizedFileSystemEntries( includeDirectories, includeFiles, option, normalizedPath, normalizedSearchPattern );
        }

        /// <summary>
        ///     Returns a value indicating whether the specified path refers to an existing directory.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path to check.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="path" /> refers to an existing directory;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Note that this method will return false if any error occurs while trying to determine
        ///     if the specified directory exists. This includes situations that would normally result in
        ///     thrown exceptions including (but not limited to); passing in a directory name with invalid
        ///     or too many characters, an I/O error such as a failing or missing disk, or if the caller
        ///     does not have Windows or Code Access Security (CAS) permissions to to read the directory.
        /// </remarks>
        public static Boolean Exists( [NotNull] this String path ) {
            if ( String.IsNullOrEmpty( value: path ) ) {
                throw new ArgumentException( message: "Value cannot be null or empty.", paramName: nameof( path ) );
            }

            return path.Exists( out var isDirectory ) && isDirectory;
        }

        [NotNull]
        public static DirectorySecurity GetAccessControl( String path ) {
            const AccessControlSections includeSections = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group;

            return GetAccessControl( path, includeSections );
        }

        [NotNull]
        public static DirectorySecurity GetAccessControl( String path, AccessControlSections includeSections ) {

            var normalizedPath = path.GetFullPath().NormalizeLongPath();
            var securityInfos = includeSections.ToSecurityInfos();

            var errorCode = ( Int32 ) NativeMethods.GetSecurityInfoByName( normalizedPath, ( UInt32 ) ResourceType.FileObject, ( UInt32 ) securityInfos, out var sidOwner,
                out var sidGroup, out var dacl, out var sacl, out var byteArray );

            Common.ThrowIfError( errorCode, byteArray );

            var length = NativeMethods.GetSecurityDescriptorLength( byteArray );

            var binaryForm = new Byte[ length ];

            Marshal.Copy( byteArray, binaryForm, 0, ( Int32 ) length );

            NativeMethods.LocalFree( byteArray );
            var ds = new DirectorySecurity();
            ds.SetSecurityDescriptorBinaryForm( binaryForm );

            return ds;
        }

        public static FileAttributes GetAttributes( String path ) {
            Common.ThrowIfBlank( ref path );

            return path.GetAttributes();
        }

        public static DateTime GetCreationTime( [NotNull] String path ) => GetCreationTimeUtc( path ).ToLocalTime();

        public static DateTime GetCreationTimeUtc( [NotNull] String path ) {
            var di = new DirectoryInfo( path );

            return di.CreationTimeUtc;
        }

        [NotNull]
        public static String GetCurrentDirectory() => ".".NormalizeLongPath().RemoveLongPathPrefix();

        [NotNull]
        public static IEnumerable<String> GetDirectories( [NotNull] this String path, [NotNull] String searchPattern, SearchOption searchOption ) {
            if ( String.IsNullOrWhiteSpace( value: path ) ) {
                throw new ArgumentException( message: "Value cannot be null or whitespace.", paramName: nameof( path ) );
            }

            return EnumerateFileSystemEntries( path, searchPattern, true, false, searchOption );
        }

        public static IEnumerable<String> GetDirectories( [NotNull] String path ) => EnumerateFileSystemEntries( path, "*", true, false, SearchOption.TopDirectoryOnly );

        public static IEnumerable<String> GetDirectories( [NotNull] String path, [NotNull] String searchPattern ) =>
            EnumerateFileSystemEntries( path, searchPattern, true, false, SearchOption.TopDirectoryOnly );

        [NotNull]
        public static SafeFileHandle GetDirectoryHandle( this String normalizedPath ) {
            var handle = NativeMethods.CreateFile( normalizedPath, NativeMethods.EFileAccess.GenericWrite, ( UInt32 ) ( FileShare.Write | FileShare.Delete ), IntPtr.Zero,
                ( Int32 ) FileMode.Open, NativeMethods.FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero );

            if ( !handle.IsInvalid ) {
                return handle;
            }

            var ex = Common.GetExceptionFromLastWin32Error();
            Debug.WriteLine( "error {0} with {1}\n{2}", ex.Message, normalizedPath, ex.StackTrace );

            throw ex;
        }

        [NotNull]
        public static String GetDirectoryRoot( String path ) {
            var fullPath = path.GetFullPath();

            return fullPath.Substring( 0, fullPath.GetRootLength() );
        }

        [NotNull]
        public static IEnumerable<String> GetFiles( [NotNull] this String path ) {
            if ( String.IsNullOrWhiteSpace( value: path ) ) {
                throw new ArgumentException( message: "Value cannot be null or whitespace.", paramName: nameof( path ) );
            }

            return EnumerateFileSystemEntries( path, "*", false, true, SearchOption.TopDirectoryOnly );
        }

        public static IEnumerable<String> GetFiles( [NotNull] String path, [NotNull] String searchPattern ) =>
            EnumerateFileSystemEntries( path, searchPattern, false, true, SearchOption.TopDirectoryOnly );

        public static IEnumerable<String> GetFiles( [NotNull] String path, [NotNull] String searchPattern, SearchOption options ) =>
            EnumerateFileSystemEntries( path, searchPattern, false, true, options );

        public static IEnumerable<String> GetFileSystemEntries( [NotNull] String path ) => EnumerateFileSystemEntries( path, null, true, true, SearchOption.TopDirectoryOnly );

        public static IEnumerable<String> GetFileSystemEntries( [NotNull] String path, [NotNull] String searchPattern ) =>
            EnumerateFileSystemEntries( path, searchPattern, true, true, SearchOption.TopDirectoryOnly );

        public static IEnumerable<String> GetFileSystemEntries( [NotNull] String path, [NotNull] String searchPattern, SearchOption options ) =>
            EnumerateFileSystemEntries( path, searchPattern, true, true, options );

        public static DateTime GetLastAccessTime( [NotNull] String path ) => GetLastAccessTimeUtc( path ).ToLocalTime();

        public static DateTime GetLastAccessTimeUtc( [NotNull] String path ) {

            var di = new DirectoryInfo( path );

            return di.LastAccessTimeUtc;
        }

        public static DateTime GetLastWriteTime( [NotNull] String path ) => GetLastWriteTimeUtc( path ).ToLocalTime();

        public static DateTime GetLastWriteTimeUtc( [NotNull] String path ) {

            var di = new DirectoryInfo( path );

            return di.LastWriteTimeUtc;
        }

        [NotNull]
        public static IEnumerable<String> GetLogicalDrives() => System.IO.Directory.GetLogicalDrives();

        [NotNull]
        public static DirectoryInfo GetParent( [NotNull] String path ) {
            Common.ThrowIfBlank( ref path );

            return new DirectoryInfo( path.GetDirectoryName() );
        }

        public static Boolean IsDirectory( this FileAttributes attributes ) => attributes.HasFlag( FileAttributes.Directory );

        public static void Move( [NotNull] String sourcePath, [NotNull] String destinationPath ) {
            if ( String.IsNullOrEmpty( value: sourcePath ) ) {
                throw new ArgumentException( message: "Value cannot be null or empty.", paramName: nameof( sourcePath ) );
            }

            if ( String.IsNullOrEmpty( value: destinationPath ) ) {
                throw new ArgumentException( message: "Value cannot be null or empty.", paramName: nameof( destinationPath ) );
            }

            var normalizedSourcePath = sourcePath.NormalizeLongPath( "sourcePath" );
            var normalizedDestinationPath = destinationPath.NormalizeLongPath( "destinationPath" );

            if ( NativeMethods.MoveFile( normalizedSourcePath, normalizedDestinationPath ) ) {
                return;
            }

            var lastWin32Error = Marshal.GetLastWin32Error();

            if ( lastWin32Error == NativeMethods.ERROR_ACCESS_DENIED ) {
                throw new IOException( $"Access to the path '{sourcePath}'is denied.", NativeMethods.MakeHRFromErrorCode( lastWin32Error ) );
            }

            throw Common.GetExceptionFromWin32Error( lastWin32Error, "path" );
        }

        public static void SetAccessControl( [NotNull] String path, [NotNull] DirectorySecurity directorySecurity ) {
            if ( directorySecurity == null ) {
                throw new ArgumentNullException( paramName: nameof( directorySecurity ) );
            }

            if ( String.IsNullOrWhiteSpace( value: path ) ) {
                throw new ArgumentException( message: "Value cannot be null or whitespace.", paramName: nameof( path ) );
            }

            var name = path.GetFullPath().NormalizeLongPath();

            Common.SetAccessControlExtracted( directorySecurity, name );
        }

        public static void SetAttributes( String path, FileAttributes fileAttributes ) {
            Common.ThrowIfBlank( ref path );
            path.SetAttributes( fileAttributes );
        }

        public static void SetCreationTime( String path, DateTime creationTime ) {
            Common.ThrowIfBlank( ref path );
            SetCreationTimeUtc( path, creationTime.ToUniversalTime() );
        }

        public static void SetCreationTimeUtc( this String path, DateTime creationTimeUtc ) {

            var normalizedPath = path.GetFullPath().NormalizeLongPath();

            using ( var handle = normalizedPath.GetDirectoryHandle() ) {
                unsafe {
                    var fileTime = new NativeMethods.FILE_TIME( creationTimeUtc.ToFileTimeUtc() );

                    if ( NativeMethods.SetFileTime( handle, &fileTime, null, null ) ) {
                        return;
                    }

                    var errorCode = Marshal.GetLastWin32Error();
                    Common.ThrowIOError( errorCode, path );
                }
            }
        }

        /// <summary>
        ///     Author's remark: NotSupportedException("Windows does not support setting the current directory to a long path");
        /// </summary>
        /// <param name="path"></param>
        public static void SetCurrentDirectory( String path ) {
            var normalizedPath = path.GetFullPath().NormalizeLongPath();

            if ( !NativeMethods.SetCurrentDirectory( normalizedPath ) ) {
                var lastWin32Error = Marshal.GetLastWin32Error();

                if ( lastWin32Error == NativeMethods.ERROR_FILE_NOT_FOUND ) {
                    lastWin32Error = NativeMethods.ERROR_PATH_NOT_FOUND;
                }

                Common.ThrowIOError( lastWin32Error, normalizedPath );
            }
        }

        public static Boolean SetFileTimes( IntPtr hFile, DateTime creationTime, DateTime accessTime, DateTime writeTime ) =>
            NativeMethods.SetFileTime( hFile, creationTime.ToFileTimeUtc(), accessTime.ToFileTimeUtc(), writeTime.ToFileTimeUtc() );

        public static void SetLastAccessTime( String path, DateTime lastAccessTime ) => SetLastAccessTimeUtc( path, lastAccessTime.ToUniversalTime() );

        public static unsafe void SetLastAccessTimeUtc( String path, DateTime lastWriteTimeUtc ) {

            var normalizedPath = path.GetFullPath().NormalizeLongPath();

            using ( var handle = normalizedPath.GetDirectoryHandle() ) {
                var fileTime = new NativeMethods.FILE_TIME( lastWriteTimeUtc.ToFileTimeUtc() );

                if ( NativeMethods.SetFileTime( handle, null, &fileTime, null ) ) {
                    return;
                }

                var errorCode = Marshal.GetLastWin32Error();
                Common.ThrowIOError( errorCode, path );
            }
        }

        public static void SetLastWriteTime( String path, DateTime lastWriteTimeUtc ) {

            unsafe {
                var normalizedPath = path.GetFullPath().NormalizeLongPath();

                using ( var handle = normalizedPath.GetDirectoryHandle() ) {
                    var fileTime = new NativeMethods.FILE_TIME( lastWriteTimeUtc.ToFileTimeUtc() );
                    var r = NativeMethods.SetFileTime( handle, null, null, &fileTime );

                    if ( r ) {
                        return;
                    }

                    var errorCode = Marshal.GetLastWin32Error();
                    Common.ThrowIOError( errorCode, path );
                }
            }
        }

        public static unsafe void SetLastWriteTimeUtc( String path, DateTime lastWriteTimeUtc ) {

            var normalizedPath = path.GetFullPath().NormalizeLongPath();

            using ( var handle = normalizedPath.GetDirectoryHandle() ) {
                var fileTime = new NativeMethods.FILE_TIME( lastWriteTimeUtc.ToFileTimeUtc() );

                if ( NativeMethods.SetFileTime( handle, null, null, &fileTime ) ) {
                    return;
                }

                var errorCode = Marshal.GetLastWin32Error();
                Common.ThrowIOError( errorCode, path );
            }
        }
    }
}