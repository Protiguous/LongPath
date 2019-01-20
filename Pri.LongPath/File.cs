// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "File.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.LongPath", "File.cs" was last formatted by Protiguous on 2019/01/12 at 8:26 PM.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "Tests" )]

namespace Pri.LongPath {

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Text;
    using JetBrains.Annotations;
    using Microsoft.Win32.SafeHandles;

    public static class File {

        [NotNull]
        public static Encoding UTF8NoBOM => _UTF8NoBOM ?? ( _UTF8NoBOM = new UTF8Encoding( false, true ) );

        private static Encoding _UTF8NoBOM;

        public static void AppendAllLines( String path, [NotNull] IEnumerable<String> contents ) {
            Common.ThrowIfBlank( ref path );
            AppendAllLines( path, contents, Encoding.UTF8 );
        }

        public static void AppendAllLines( String path, [NotNull] IEnumerable<String> contents, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );
            const Boolean append = true;

            using ( var writer = CreateStreamWriter( path, append, encoding ) ) {
                foreach ( var line in contents ) {
                    writer.WriteLine( line );
                }
            }
        }

        public static void AppendAllText( String path, String contents ) {
            Common.ThrowIfBlank( ref path );
            AppendAllText( path, contents, Encoding.UTF8 );
        }

        public static void AppendAllText( String path, String contents, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );
            const Boolean append = true;

            using ( var writer = CreateStreamWriter( path, append, encoding ) ) {
                writer.Write( contents );
            }
        }

        [NotNull]
        public static StreamWriter AppendText( String path ) {
            Common.ThrowIfBlank( ref path );

            return CreateStreamWriter( path, true );
        }

        public static void Copy( String sourceFileName, String destFileName ) {
            Common.ThrowIfBlank( ref sourceFileName );
            Common.ThrowIfBlank( ref destFileName );
            Copy( sourceFileName, destFileName, false );
        }

        /// <summary>
        ///     Copies the specified file to a specified new file, indicating whether to overwrite an existing file.
        /// </summary>
        /// <param name="sourcePath">
        ///     A <see cref="String" /> containing the path of the file to copy.
        /// </param>
        /// <param name="destinationPath">
        ///     A <see cref="String" /> containing the new path of the file.
        /// </param>
        /// <param name="overwrite">
        ///     <see langword="true" /> if <paramref name="destinationPath" /> should be overwritten
        ///     if it refers to an existing file, otherwise, <see langword="false" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" /> is
        ///     <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" /> is
        ///     an empty string (""), contains only white space, or contains one or more
        ///     invalid characters as defined in <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" />
        ///     contains one or more components that exceed the drive-defined maximum length.
        ///     For example, on Windows-based platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" />
        ///     exceeds the system-defined maximum length. For example, on Windows-based platforms,
        ///     paths must not exceed 32,000 characters.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="sourcePath" /> could not be found.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="sourcePath" /> and/or
        ///     <paramref name="destinationPath" /> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="overwrite" /> is true and <paramref name="destinationPath" /> refers to a
        ///     file that is read-only.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="overwrite" /> is false and <paramref name="destinationPath" /> refers to
        ///     a file that already exists.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" /> is a
        ///     directory.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="overwrite" /> is true and <paramref name="destinationPath" /> refers to
        ///     a file that already exists and is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath" /> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" /> specifies
        ///     a device that is not ready.
        /// </exception>
        public static void Copy( String sourcePath, String destinationPath, Boolean overwrite ) {
            Common.ThrowIfBlank( ref sourcePath );
            Common.ThrowIfBlank( ref destinationPath );

            var normalizedSourcePath = sourcePath.NormalizeLongPath( "sourcePath" );
            var normalizedDestinationPath = destinationPath.NormalizeLongPath( "destinationPath" );

            if ( !NativeMethods.CopyFile( normalizedSourcePath, normalizedDestinationPath, !overwrite ) ) {
                throw Common.GetExceptionFromLastWin32Error();
            }
        }

        [NotNull]
        public static FileStream Create( String path ) {
            Common.ThrowIfBlank( ref path );

            return Create( path, Common.DefaultBufferSize );
        }

        [NotNull]
        public static FileStream Create( String path, Int32 bufferSize ) {
            Common.ThrowIfBlank( ref path );

            return Open( path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, FileOptions.None );
        }

        [NotNull]
        public static FileStream Create( String path, Int32 bufferSize, FileOptions options ) {
            Common.ThrowIfBlank( ref path );

            return Open( path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options );
        }

        [NotNull]
        public static FileStream Create( String path, Int32 bufferSize, FileOptions options, [NotNull] FileSecurity fileSecurity ) {
            Common.ThrowIfBlank( ref path );

            var fileStream = Create( path, bufferSize, options );
            fileStream.SetAccessControl( fileSecurity );

            return fileStream;
        }

        /// <remarks>
        ///     replaces "new StreamReader(path, true|false)"
        /// </remarks>
        [NotNull]
        public static StreamReader CreateStreamReader( String path, [NotNull] Encoding encoding, Boolean detectEncodingFromByteOrderMarks, Int32 bufferSize ) {
            Common.ThrowIfBlank( ref path );
            var fileStream = Open( path, FileMode.Open, FileAccess.Read, FileShare.Read, Common.DefaultBufferSize, FileOptions.SequentialScan );

            return new StreamReader( fileStream, encoding, detectEncodingFromByteOrderMarks, bufferSize );
        }

        /// <remarks>
        ///     replaces "new StreamWriter(path, true|false)"
        /// </remarks>
        [NotNull]
        public static StreamWriter CreateStreamWriter( String path, Boolean append ) {
            Common.ThrowIfBlank( ref path );
            var fileMode = append ? FileMode.Append : FileMode.Create;
            var fileStream = Open( path, fileMode, FileAccess.Write, FileShare.Read, Common.DefaultBufferSize, FileOptions.SequentialScan );

            return new StreamWriter( fileStream, UTF8NoBOM, Common.DefaultBufferSize );
        }

        [NotNull]
        public static StreamWriter CreateStreamWriter( String path, Boolean append, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );
            var fileMode = append ? FileMode.Append : FileMode.Create;
            var fileStream = Open( path, fileMode, FileAccess.Write, FileShare.Read, Common.DefaultBufferSize, FileOptions.SequentialScan );

            return new StreamWriter( fileStream, encoding, Common.DefaultBufferSize );
        }

        [NotNull]
        public static StreamWriter CreateText( String path ) {
            Common.ThrowIfBlank( ref path );

            var fileStream = Open( path, FileMode.Create, FileAccess.Write, FileShare.Read, Common.DefaultBufferSize, FileOptions.SequentialScan );

            return new StreamWriter( fileStream, UTF8NoBOM, Common.DefaultBufferSize );
        }

        [NotNull]
        public static StreamWriter CreateText( String path, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );

            return CreateStreamWriter( path, false, encoding );
        }

        public static void Decrypt( String path ) {
            Common.ThrowIfBlank( ref path );

            var fullPath = path.GetFullPath();
            var normalizedPath = fullPath.NormalizeLongPath();

            if ( NativeMethods.DecryptFile( normalizedPath, 0 ) ) {
                return;
            }

            var errorCode = Marshal.GetLastWin32Error();

            if ( errorCode == NativeMethods.ERROR_ACCESS_DENIED ) {
                var di = new DriveInfo( normalizedPath.GetPathRoot() );

                if ( !String.Equals( "NTFS", di.DriveFormat ) ) {
                    throw new NotSupportedException( "NTFS drive required for file encryption" );
                }
            }

            Common.ThrowIOError( errorCode, fullPath );
        }

        /// <summary>
        ///     Deletes the specified file.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the file to delete.
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
        /// <exception cref="PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="path" /> could not be found.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="path" /> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> refers to a file that is read-only.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> is a directory.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="path" /> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        public static void Delete( String path ) {
            Common.ThrowIfBlank( ref path );

            var normalizedPath = path.NormalizeLongPath();

            if ( !path.Exists() ) {
                return;
            }

            if ( !NativeMethods.DeleteFile( normalizedPath ) ) {
                throw Common.GetExceptionFromLastWin32Error();
            }
        }

        public static void Encrypt( String path ) {
            Common.ThrowIfBlank( ref path );

            var fullPath = path.GetFullPath();
            var normalizedPath = fullPath.NormalizeLongPath();

            if ( NativeMethods.EncryptFile( normalizedPath ) ) {
                return;
            }

            var errorCode = Marshal.GetLastWin32Error();

            if ( errorCode == NativeMethods.ERROR_ACCESS_DENIED ) {
                var di = new DriveInfo( normalizedPath.GetPathRoot() );

                if ( !String.Equals( "NTFS", di.DriveFormat ) ) {
                    throw new NotSupportedException( "NTFS drive required for file encryption" );
                }
            }

            Common.ThrowIOError( errorCode, fullPath );
        }

        /// <summary>
        ///     Returns a value indicating whether the specified path refers to an existing file.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path to check.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="path" /> refers to an existing file;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Note that this method will return false if any error occurs while trying to determine
        ///     if the specified file exists. This includes situations that would normally result in
        ///     thrown exceptions including (but not limited to); passing in a file name with invalid
        ///     or too many characters, an I/O error such as a failing or missing disk, or if the caller
        ///     does not have Windows or Code Access Security (CAS) permissions to to read the file.
        /// </remarks>
        public static Boolean Exists( String path ) {
            Common.ThrowIfBlank( ref path );

            if ( path.Exists( out var isDirectory ) ) {
                return !isDirectory;
            }

            return false;
        }

        [NotNull]
        public static FileSecurity GetAccessControl( String path ) {
            Common.ThrowIfBlank( ref path );
            const AccessControlSections includeSections = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group;

            return GetAccessControl( path, includeSections );
        }

        [NotNull]
        public static FileSecurity GetAccessControl( String path, AccessControlSections includeSections ) {
            Common.ThrowIfBlank( ref path );

            var normalizedPath = path.GetFullPath().NormalizeLongPath();

            var SecurityInfos = includeSections.ToSecurityInfos();

            var errorCode = ( Int32 ) NativeMethods.GetSecurityInfoByName( normalizedPath, ( UInt32 ) ResourceType.FileObject, ( UInt32 ) SecurityInfos, out var SidOwner,
                out var SidGroup, out var Dacl, out var Sacl, out var ByteArray );

            Common.ThrowIfError( errorCode, ByteArray );

            var Length = NativeMethods.GetSecurityDescriptorLength( ByteArray );

            var BinaryForm = new Byte[ Length ];

            Marshal.Copy( ByteArray, BinaryForm, 0, ( Int32 ) Length );

            NativeMethods.LocalFree( ByteArray );
            var fs = new FileSecurity();
            fs.SetSecurityDescriptorBinaryForm( BinaryForm );

            return fs;
        }

        public static FileAttributes GetAttributes( String path ) {
            Common.ThrowIfBlank( ref path );

            return path.GetFileAttributes();
        }

        public static DateTime GetCreationTime( this String path ) {
            Common.ThrowIfBlank( ref path );

            return path.GetCreationTimeUtc().ToLocalTime();
        }

        public static DateTime GetCreationTimeUtc( this String path ) {
            Common.ThrowIfBlank( ref path );

            return new FileInfo( path ).CreationTimeUtc;
        }

        [NotNull]
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "handle is stored by caller" )]
        public static SafeFileHandle GetFileHandle( String normalizedPath, FileMode mode, FileAccess access, FileShare share, FileOptions options ) {
            Common.ThrowIfBlank( ref normalizedPath );
            var append = mode == FileMode.Append;

            if ( append ) {
                mode = FileMode.OpenOrCreate;
            }

            var underlyingAccess = GetUnderlyingAccess( access );

            var handle = NativeMethods.CreateFile( normalizedPath, underlyingAccess, ( UInt32 ) share, IntPtr.Zero, ( UInt32 ) mode, ( UInt32 ) options, IntPtr.Zero );

            if ( handle.IsInvalid ) {
                var ex = Common.GetExceptionFromLastWin32Error();
                Debug.WriteLine( $"error {ex.Message} with {normalizedPath}{Environment.NewLine}{ex.StackTrace}" );
                Debug.WriteLine( $"{mode} {access} {share} {options}" );

                throw ex;
            }

            if ( append ) {
                NativeMethods.SetFilePointer( handle, SeekOrigin.End, 0 );
            }

            return handle;
        }

        public static DateTime GetLastAccessTime( String path ) {
            Common.ThrowIfBlank( ref path );

            return GetLastAccessTimeUtc( path ).ToLocalTime();
        }

        public static DateTime GetLastAccessTimeUtc( String path ) {
            Common.ThrowIfBlank( ref path );

            return new FileInfo( path ).LastAccessTimeUtc;
        }

        public static DateTime GetLastWriteTime( String path ) {
            Common.ThrowIfBlank( ref path );

            return GetLastWriteTimeUtc( path ).ToLocalTime();
        }

        public static DateTime GetLastWriteTimeUtc( String path ) {
            Common.ThrowIfBlank( ref path );

            return new FileInfo( path ).LastWriteTimeUtc;
        }

        public static NativeMethods.EFileAccess GetUnderlyingAccess( FileAccess access ) {
            switch ( access ) {
                case FileAccess.Read: return NativeMethods.EFileAccess.GenericRead;

                case FileAccess.Write: return NativeMethods.EFileAccess.GenericWrite;

                case FileAccess.ReadWrite: return NativeMethods.EFileAccess.GenericRead | NativeMethods.EFileAccess.GenericWrite;

                default: throw new ArgumentOutOfRangeException( nameof( access ) );
            }
        }

        /// <summary>
        ///     Moves the specified file to a new location.
        /// </summary>
        /// <param name="sourcePath">
        ///     A <see cref="String" /> containing the path of the file to move.
        /// </param>
        /// <param name="destinationPath">
        ///     A <see cref="String" /> containing the new path of the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" /> is
        ///     <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" /> is
        ///     an empty string (""), contains only white space, or contains one or more
        ///     invalid characters as defined in <see cref="Path.GetInvalidPathChars()" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" />
        ///     contains one or more components that exceed the drive-defined maximum length.
        ///     For example, on Windows-based platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" />
        ///     exceeds the system-defined maximum length. For example, on Windows-based platforms,
        ///     paths must not exceed 32,000 characters.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="sourcePath" /> could not be found.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="sourcePath" /> and/or
        ///     <paramref name="destinationPath" /> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="destinationPath" /> refers to a file that already exists.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" /> is a
        ///     directory.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath" /> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath" /> and/or <paramref name="destinationPath" /> specifies
        ///     a device that is not ready.
        /// </exception>
        public static void Move( String sourcePath, String destinationPath ) {
            Common.ThrowIfBlank( ref sourcePath );
            Common.ThrowIfBlank( ref destinationPath );

            var normalizedSourcePath = sourcePath.NormalizeLongPath( "sourcePath" );
            var normalizedDestinationPath = destinationPath.NormalizeLongPath( "destinationPath" );

            if ( !NativeMethods.MoveFile( normalizedSourcePath, normalizedDestinationPath ) ) {
                throw Common.GetExceptionFromLastWin32Error();
            }
        }

        [NotNull]
        public static FileStream Open( String path, FileMode mode ) {
            Common.ThrowIfBlank( ref path );

            return Open( path, mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None );
        }

        /// <summary>
        ///     Opens the specified file.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the file to open.
        /// </param>
        /// <param name="access">
        ///     One of the <see cref="FileAccess" /> value that specifies the operations that can be
        ///     performed on the file.
        /// </param>
        /// <param name="mode">
        ///     One of the <see cref="FileMode" /> values that specifies whether a file is created
        ///     if one does not exist, and determines whether the contents of existing files are
        ///     retained or overwritten.
        /// </param>
        /// <returns>
        ///     A <see cref="FileStream" /> that provides access to the file specified in
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
        /// <exception cref="PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="path" /> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> refers to a file that is read-only and <paramref name="access" />
        ///     is not <see cref="FileAccess.Read" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> is a directory.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="path" /> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        [NotNull]
        public static FileStream Open( String path, FileMode mode, FileAccess access ) {
            Common.ThrowIfBlank( ref path );

            return Open( path, mode, access, FileShare.None, 0, FileOptions.None );
        }

        /// <summary>
        ///     Opens the specified file.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the file to open.
        /// </param>
        /// <param name="access">
        ///     One of the <see cref="FileAccess" /> value that specifies the operations that can be
        ///     performed on the file.
        /// </param>
        /// <param name="mode">
        ///     One of the <see cref="FileMode" /> values that specifies whether a file is created
        ///     if one does not exist, and determines whether the contents of existing files are
        ///     retained or overwritten.
        /// </param>
        /// <param name="share">
        ///     One of the <see cref="FileShare" /> values specifying the type of access other threads
        ///     have to the file.
        /// </param>
        /// <returns>
        ///     A <see cref="FileStream" /> that provides access to the file specified in
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
        /// <exception cref="PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="path" /> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> refers to a file that is read-only and <paramref name="access" />
        ///     is not <see cref="FileAccess.Read" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> is a directory.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="path" /> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        [NotNull]
        public static FileStream Open( String path, FileMode mode, FileAccess access, FileShare share ) {
            Common.ThrowIfBlank( ref path );

            return Open( path, mode, access, share, 0, FileOptions.None );
        }

        /// <summary>
        ///     Opens the specified file.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String" /> containing the path of the file to open.
        /// </param>
        /// <param name="access">
        ///     One of the <see cref="FileAccess" /> value that specifies the operations that can be
        ///     performed on the file.
        /// </param>
        /// <param name="mode">
        ///     One of the <see cref="FileMode" /> values that specifies whether a file is created
        ///     if one does not exist, and determines whether the contents of existing files are
        ///     retained or overwritten.
        /// </param>
        /// <param name="share">
        ///     One of the <see cref="FileShare" /> values specifying the type of access other threads
        ///     have to the file.
        /// </param>
        /// <param name="bufferSize">
        ///     An <see cref="Int32" /> containing the number of bytes to buffer for reads and writes
        ///     to the file, or 0 to specified the default buffer size, DefaultBufferSize.
        /// </param>
        /// <param name="options">
        ///     One or more of the <see cref="FileOptions" /> values that describes how to create or
        ///     overwrite the file.
        /// </param>
        /// <returns>
        ///     A <see cref="FileStream" /> that provides access to the file specified in
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
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="bufferSize" /> is less than 0.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     <paramref name="path" /> exceeds the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must not exceed
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="path" /> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> refers to a file that is read-only and <paramref name="access" />
        ///     is not <see cref="FileAccess.Read" />.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> is a directory.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="path" /> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path" /> specifies a device that is not ready.
        /// </exception>
        [NotNull]
        public static FileStream Open( String path, FileMode mode, FileAccess access, FileShare share, Int32 bufferSize, FileOptions options ) {
            Common.ThrowIfBlank( ref path );

            const Int32 defaultBufferSize = Common.DefaultBufferSize;

            if ( bufferSize == 0 ) {
                bufferSize = defaultBufferSize;
            }

            var normalizedPath = path.NormalizeLongPath();

            var handle = GetFileHandle( normalizedPath, mode, access, share, options );

            return new FileStream( handle, access, bufferSize,  options.HasFlag( FileOptions.Asynchronous ) );
        }

        [NotNull]
        public static FileStream OpenRead( String path ) {
            Common.ThrowIfBlank( ref path );

            return Open( path, FileMode.Open, FileAccess.Read, FileShare.Read );
        }

        [NotNull]
        public static StreamReader OpenText( String path, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );

            var stream = Open( path, FileMode.Open, FileAccess.Read, FileShare.Read, Common.DefaultBufferSize, FileOptions.SequentialScan );

            return new StreamReader( stream, encoding, true, Common.DefaultBufferSize );
        }

        [NotNull]
        public static StreamReader OpenText( String path ) {
            Common.ThrowIfBlank( ref path );

            var stream = Open( path, FileMode.Open, FileAccess.Read, FileShare.Read, Common.DefaultBufferSize, FileOptions.SequentialScan );

            return new StreamReader( stream, Encoding.UTF8, true, Common.DefaultBufferSize );
        }

        [NotNull]
        public static FileStream OpenWrite( String path ) {
            Common.ThrowIfBlank( ref path );

            return Open( path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None );
        }

        [NotNull]
        public static Byte[] ReadAllBytes( String path ) {
            Common.ThrowIfBlank( ref path );

            using ( var fileStream = Open( path, FileMode.Open, FileAccess.Read, FileShare.Read ) ) {
                var length = fileStream.Length;

                if ( length > Int32.MaxValue ) {
                    throw new IOException( "File length greater than 2GB." );
                }

                var bytes = new Byte[ length ];
                var offset = 0;

                while ( length > 0 ) {
                    var read = fileStream.Read( bytes, offset, ( Int32 ) length );

                    if ( read == 0 ) {
                        throw new EndOfStreamException( "Read beyond end of file." );
                    }

                    offset += read;
                    length -= read;
                }

                return bytes;
            }
        }

        [NotNull]
        public static IEnumerable<String> ReadAllLines( String path ) {
            Common.ThrowIfBlank( ref path );

            return ReadLines( path ).ToArray();
        }

        [NotNull]
        public static IEnumerable<String> ReadAllLines( String path, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );

            return ReadLines( path, encoding ).ToArray();
        }

        [NotNull]
        public static String ReadAllText( String path ) {
            Common.ThrowIfBlank( ref path );

            return ReadAllText( path, Encoding.UTF8 );
        }

        [NotNull]
        public static String ReadAllText( String path, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );

            using ( var streamReader = OpenText( path, encoding ) ) {
                return streamReader.ReadToEnd();
            }
        }

        [NotNull]
        public static IEnumerable<String> ReadLines( String path ) {
            Common.ThrowIfBlank( ref path );

            return ReadAllLines( path, Encoding.UTF8 );
        }

        [ItemCanBeNull]
        public static IEnumerable<String> ReadLines( String path, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );
            var stream = Open( path, FileMode.Open, FileAccess.Read, FileShare.Read, Common.DefaultBufferSize, FileOptions.SequentialScan );

            using ( var sr = new StreamReader( stream, encoding, true, Common.DefaultBufferSize ) ) {
                while ( !sr.EndOfStream ) {
                    yield return sr.ReadLine();
                }
            }
        }

        public static void Replace( String sourceFileName, String destinationFileName, String destinationBackupFileName ) {
            Common.ThrowIfBlank( ref sourceFileName );
            Common.ThrowIfBlank( ref destinationFileName );
            Common.ThrowIfBlank( ref destinationBackupFileName );

            Replace( sourceFileName, destinationFileName, destinationBackupFileName, false );
        }

        public static void Replace( String sourceFileName, String destinationFileName, String destinationBackupFileName, Boolean ignoreMetadataErrors ) {
            Common.ThrowIfBlank( ref sourceFileName );
            Common.ThrowIfBlank( ref destinationFileName );
            Common.ThrowIfBlank( ref destinationBackupFileName );

            var fullSrcPath = sourceFileName.GetFullPath().NormalizeLongPath();
            var fullDestPath = destinationFileName.GetFullPath().NormalizeLongPath();
            var fullBackupPath = destinationBackupFileName.GetFullPath().NormalizeLongPath();

            var flags = NativeMethods.REPLACEFILE_WRITE_THROUGH;

            if ( ignoreMetadataErrors ) {
                flags |= NativeMethods.REPLACEFILE_IGNORE_MERGE_ERRORS;
            }

            var r = NativeMethods.ReplaceFile( fullDestPath, fullSrcPath, fullBackupPath, flags, IntPtr.Zero, IntPtr.Zero );

            if ( !r ) {
                Common.ThrowIOError( Marshal.GetLastWin32Error(), String.Empty );
            }
        }

        public static void SetAccessControl( [NotNull] String path, [NotNull] FileSecurity fileSecurity ) {
            Common.ThrowIfBlank( ref path );

            if ( fileSecurity == null ) {
                throw new ArgumentNullException( paramName: nameof( fileSecurity ) );
            }

            var name = path.GetFullPath().NormalizeLongPath();

            Common.SetAccessControlExtracted( fileSecurity, name );
        }

        public static void SetAttributes( String path, FileAttributes fileAttributes ) {
            Common.ThrowIfBlank( ref path );

            path.SetAttributes( fileAttributes );
        }

        public static void SetCreationTime( [NotNull] this String path, DateTime creationTime ) {
            Common.ThrowIfBlank( ref path );

            SetCreationTimeUtc( path, creationTime.ToUniversalTime() );
        }

        public static unsafe void SetCreationTimeUtc( [NotNull] this String path, DateTime creationTimeUtc ) {
            Common.ThrowIfBlank( ref path );

            var normalizedPath = path.NormalizeLongPath();

            using ( var handle = GetFileHandle( normalizedPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.None ) ) {
                var fileTime = new NativeMethods.FILE_TIME( creationTimeUtc.ToFileTimeUtc() );
                var r = NativeMethods.SetFileTime( handle, &fileTime, null, null );

                if ( !r ) {
                    var errorCode = Marshal.GetLastWin32Error();
                    Common.ThrowIOError( errorCode, path );
                }
            }
        }

        public static void SetLastAccessTime( String path, DateTime lastAccessTime ) {
            Common.ThrowIfBlank( ref path );
            SetLastAccessTimeUtc( path, lastAccessTime.ToUniversalTime() );
        }

        public static unsafe void SetLastAccessTimeUtc( String path, DateTime lastAccessTimeUtc ) {
            Common.ThrowIfBlank( ref path );

            var normalizedPath = path.NormalizeLongPath();

            using ( var handle = GetFileHandle( normalizedPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.None ) ) {
                var fileTime = new NativeMethods.FILE_TIME( lastAccessTimeUtc.ToFileTimeUtc() );
                var r = NativeMethods.SetFileTime( handle, null, &fileTime, null );

                if ( !r ) {
                    var errorCode = Marshal.GetLastWin32Error();
                    Common.ThrowIOError( errorCode, path );
                }
            }
        }

        public static void SetLastWriteTime( this String path, DateTime lastWriteTime ) {
            Common.ThrowIfBlank( ref path );
            SetLastWriteTimeUtc( path, lastWriteTime.ToUniversalTime() );
        }

        public static unsafe void SetLastWriteTimeUtc( [NotNull] String path, DateTime lastWriteTimeUtc ) {
            Common.ThrowIfBlank( ref path );

            var normalizedPath = path.NormalizeLongPath();

            using ( var handle = GetFileHandle( normalizedPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.None ) ) {
                var fileTime = new NativeMethods.FILE_TIME( lastWriteTimeUtc.ToFileTimeUtc() );
                var r = NativeMethods.SetFileTime( handle, null, null, &fileTime );

                if ( !r ) {
                    var errorCode = Marshal.GetLastWin32Error();
                    Common.ThrowIOError( errorCode, path );
                }
            }
        }

        public static void WriteAllBytes( String path, [NotNull] Byte[] bytes ) {
            Common.ThrowIfBlank( ref path );

            using ( var fileStream = Open( path, FileMode.Create, FileAccess.Write, FileShare.Read ) ) {
                fileStream.Write( bytes, 0, bytes.Length );
            }
        }

        public static void WriteAllLines( String path, [NotNull] String[] contents ) {
            Common.ThrowIfBlank( ref path );

            if ( contents == null ) {
                throw new ArgumentNullException( paramName: nameof( contents ) );
            }

            WriteAllLines( path, contents, Encoding.UTF8 );
        }

        public static void WriteAllLines( String path, [NotNull] String[] contents, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );

            using ( var writer = CreateStreamWriter( path, false, encoding ) ) {
                foreach ( var line in contents ) {
                    writer.WriteLine( line );
                }
            }
        }

        public static void WriteAllLines( String path, [NotNull] IEnumerable<String> contents ) {
            Common.ThrowIfBlank( ref path );
            WriteAllLines( path, contents, Encoding.UTF8 );
        }

        public static void WriteAllLines( String path, [NotNull] IEnumerable<String> contents, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );
            const Boolean doNotAppend = false;

            using ( var writer = CreateStreamWriter( path, doNotAppend, encoding ) ) {
                foreach ( var line in contents ) {
                    writer.WriteLine( line );
                }
            }
        }

        public static void WriteAllText( String path, String contents ) {
            Common.ThrowIfBlank( ref path );
            WriteAllText( path, contents, UTF8NoBOM );
        }

        public static void WriteAllText( String path, String contents, [NotNull] Encoding encoding ) {
            Common.ThrowIfBlank( ref path );
            const Boolean doNotAppend = false;

            using ( var sw = CreateStreamWriter( path, doNotAppend, encoding ) ) {
                sw.Write( contents );
            }
        }
    }
}