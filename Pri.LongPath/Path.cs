// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "Path.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.LongPath", "Path.cs" was last formatted by Protiguous on 2019/01/12 at 8:28 PM.

namespace Pri.LongPath {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using JetBrains.Annotations;

    public static class Path {

        public const String LongPathPrefix = @"\\?\";

        public const String UNCLongPathPrefix = @"\\?\UNC\";

        public const Char VolumeSeparatorChar = ':';

        public static readonly Char AltDirectorySeparatorChar = System.IO.Path.AltDirectorySeparatorChar;

        public static readonly Char DirectorySeparatorChar = System.IO.Path.DirectorySeparatorChar;

        public static readonly Char[] InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();

        public static readonly Char[] InvalidPathChars = System.IO.Path.GetInvalidPathChars();

        public static readonly Char PathSeparator = System.IO.Path.PathSeparator;

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="ArgumentException">Thrown if any invalid chars found in path.</exception>
        private static void CheckInvalidPathChars( [NotNull] this String path ) {
            Common.ThrowIfBlank( ref path );

            if ( path.HasIllegalCharacters() ) {
                throw new ArgumentException( "Invalid characters in path", nameof( path ) );
            }
        }

        private static Int32 GetUncRootLength( [NotNull] this String path ) {
            Common.ThrowIfBlank( ref path );

            var components = path.Split( new[] {
                DirectorySeparatorChar
            }, StringSplitOptions.RemoveEmptyEntries );

            return $@"\\{components[ 0 ]}\{components[ 1 ]}\".Length;
        }

        [NotNull]
        public static String AddLongPathPrefix( [NotNull] this String path ) {
            Common.ThrowIfBlank( ref path );

            if ( path.StartsWith( LongPathPrefix ) ) {
                return path;
            }

            // http://msdn.microsoft.com/en-us/library/aa365247.aspx
            if ( path.StartsWith( @"\\" ) ) {

                // UNC.
                return $"{UNCLongPathPrefix}{path.Substring( 2 )}";
            }

            return $"{LongPathPrefix}{path}";
        }

        [NotNull]
        public static String ChangeExtension( [NotNull] this String filename, [CanBeNull] String extension ) {
            Common.ThrowIfBlank( ref filename );

            return System.IO.Path.ChangeExtension( filename, extension );
        }

        [NotNull]
        public static String CheckAddLongPathPrefix( [NotNull] this String path ) {
            Common.ThrowIfBlank( ref path );

            if ( path.StartsWith( LongPathPrefix ) ) {
                return path;
            }

            var maxPathLimit = NativeMethods.MAX_PATH;

            if ( Uri.TryCreate( path, UriKind.Absolute, out var uri ) && uri.IsUnc ) {

                // What's going on here?  Empirical evidence shows that Windows has trouble dealing with UNC paths
                // longer than MAX_PATH *minus* the length of the "\\hostname\" prefix.  See the following tests:
                //  - UncDirectoryTests.TestDirectoryCreateNearMaxPathLimit
                //  - UncDirectoryTests.TestDirectoryEnumerateDirectoriesNearMaxPathLimit
                var rootPathLength = 3 + uri.Host.Length;
                maxPathLimit -= rootPathLength;
            }

            if ( path.Length < maxPathLimit ) {
                return path;
            }

            return path.AddLongPathPrefix();
        }

        [NotNull]
        public static String Combine( [NotNull] this String path1, [NotNull] String path2 ) {
            Common.ThrowIfBlank( ref path1 );
            Common.ThrowIfBlank( ref path2 );

            path1.CheckInvalidPathChars();

            path2.CheckInvalidPathChars();

            if ( path2.Length == 0 ) {
                return path1;
            }

            if ( path1.Length == 0 || path2.IsPathRooted() ) {
                return path2;
            }

            var ch = path1[ path1.Length - 1 ];

            if ( ch.IsDirectorySeparator() || ch == VolumeSeparatorChar ) {
                return $"{path1}{path2}";
            }

            return $"{path1}{DirectorySeparatorChar}{path2}";
        }

        [NotNull]
        public static String Combine( [NotNull] this String path1, [NotNull] String path2, [NotNull] String path3 ) {
            Common.ThrowIfBlank( ref path1 );
            Common.ThrowIfBlank( ref path2 );
            Common.ThrowIfBlank( ref path3 );

            return Combine( path1, path2 ).Combine( path3 );
        }

        [NotNull]
        public static String Combine( [NotNull] this String path1, [NotNull] String path2, [NotNull] String path3, [NotNull] String path4 ) {
            Common.ThrowIfBlank( ref path1 );
            Common.ThrowIfBlank( ref path2 );
            Common.ThrowIfBlank( ref path3 );
            Common.ThrowIfBlank( ref path4 );

            return Combine( path1.Combine( path2 ), path3 ).Combine( path4 );
        }

        [NotNull]
        public static String Combine( [NotNull] params String[] paths ) {
            if ( paths == null ) {
                throw new ArgumentNullException( paramName: nameof( paths ) );
            }

            switch ( paths.Length ) {
                case 0: return String.Empty;
                case 1: {
                    paths[ 0 ].CheckInvalidPathChars();

                    return paths[ 0 ];
                }
                default: {
                    paths[ 0 ].CheckInvalidPathChars();
                    var path = paths[ 0 ];
                    Common.ThrowIfBlank( ref path );

                    for ( var i = 1; i < paths.Length; ++i ) {
                        path = path.Combine( paths[ i ] );
                    }

                    return path;
                }
            }
        }

        [NotNull]
        public static String GetDirectoryName( [NotNull] this String path ) {
            Common.ThrowIfBlank( ref path );

            path.CheckInvalidPathChars();
            String basePath = null;

            if ( !path.IsPathRooted() ) {
                basePath = System.IO.Directory.GetCurrentDirectory();
            }

            path = path.NormalizeLongPath().RemoveLongPathPrefix();
            var rootLength = path.GetRootLength();

            if ( path.Length <= rootLength ) {
                return String.Empty;
            }

            var length = path.Length;

            do { } while ( length > rootLength && !path[ --length ].IsDirectorySeparator() );

            if ( basePath == null ) {
                return path.Substring( 0, length );
            }

            path = path.Substring( basePath.Length + 1 );
            length = length - basePath.Length - 1;

            if ( length < 0 ) {
                length = 0;
            }

            return path.Substring( 0, length );
        }

        [CanBeNull]
        public static String GetExtension( this String path ) {
            Common.ThrowIfBlank( ref path );

            return System.IO.Path.GetExtension( path );
        }

        [NotNull]
        public static String GetFileName( [NotNull] this String path ) {
            Common.ThrowIfBlank( ref path );

            return System.IO.Path.GetFileName( path.NormalizeLongPath() );
        }

        [CanBeNull]
        public static String GetFileNameWithoutExtension( [NotNull] this String path ) {
            Common.ThrowIfBlank( ref path );

            return System.IO.Path.GetFileNameWithoutExtension( path );
        }

        [NotNull]
        public static String GetFullPath( this String path ) {
            Common.ThrowIfBlank( ref path );

            return path.IsPathUnc() ? path : path.NormalizeLongPath().RemoveLongPathPrefix();
        }

        public static IEnumerable<Char> GetInvalidFileNameChars() => InvalidFileNameChars;

        public static IEnumerable<Char> GetInvalidPathChars() => InvalidPathChars;

        [NotNull]
        public static String GetPathRoot( [NotNull] this String path ) {
            Common.ThrowIfBlank( ref path );

            if ( !path.IsPathRooted() ) {
                return String.Empty;
            }

            if ( !path.IsPathUnc() ) {
                path = path.NormalizeLongPath().RemoveLongPathPrefix();
            }

            return path.Substring( 0, path.GetRootLength() );
        }

        [NotNull]
        public static String GetRandomFileName() => System.IO.Path.GetRandomFileName(); //TODO implement my own in the new Document class. longer/guidy

        public static Int32 GetRootLength( this String path ) {
            Common.ThrowIfBlank( ref path );

            if ( path.IsPathUnc() ) {
                return path.GetUncRootLength();
            }

            path = path.GetFullPath();
            path.CheckInvalidPathChars();
            var rootLength = 0;
            var length = path.Length;

            if ( length >= 1 && path[ 0 ].IsDirectorySeparator() ) {
                rootLength = 1;

                if ( length >= 2 && path[ 1 ].IsDirectorySeparator() ) {
                    rootLength = 2;
                    var num = 2;

                    while ( rootLength >= length ||
                            ( path[ rootLength ] == System.IO.Path.DirectorySeparatorChar || path[ rootLength ] == System.IO.Path.AltDirectorySeparatorChar ) && --num <= 0 ) {
                        ++rootLength;
                    }
                }
            }
            else if ( length >= 2 && path[ 1 ] == System.IO.Path.VolumeSeparatorChar ) {
                rootLength = 2;

                if ( length >= 3 && path[ 2 ].IsDirectorySeparator() ) {
                    ++rootLength;
                }
            }

            return rootLength;
        }

        [NotNull]
        public static String GetTempFileName() => System.IO.Path.GetTempFileName();

        [NotNull]
        public static String GetTempPath() => System.IO.Path.GetTempPath();

        public static Boolean HasExtension( this String path ) => System.IO.Path.HasExtension( path );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Boolean HasIllegalCharacters( [NotNull] this String path ) => path.Any( InvalidPathChars.Contains );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsDirectorySeparator( this Char c ) => c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;

        [Pure]
        public static Boolean IsPathRooted( this String path ) => System.IO.Path.IsPathRooted( path );

        /// <summary>
        ///     Normalizes path (can be longer than MAX_PATH) and adds \\?\ long path prefix
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        [NotNull]
        public static String NormalizeLongPath( [NotNull] this String path, String parameterName = "path" ) {
            Common.ThrowIfBlank( ref path );

            if ( path.IsPathUnc() ) {
                return path.CheckAddLongPathPrefix();
            }

            var buffer = new StringBuilder( path.Length + 1 ); // Add 1 for NULL
            var length = NativeMethods.GetFullPathNameW( path, ( UInt32 ) buffer.Capacity, buffer, IntPtr.Zero );

            if ( length > buffer.Capacity ) {

                // Resulting path longer than our buffer, so increase it

                buffer.Capacity = ( Int32 ) length;
                length = NativeMethods.GetFullPathNameW( path, length, buffer, IntPtr.Zero );
            }

            if ( length == 0 ) {
                throw Common.GetExceptionFromLastWin32Error( parameterName );
            }

            if ( length > NativeMethods.MAX_LONG_PATH ) {
                throw Common.GetExceptionFromWin32Error( NativeMethods.ERROR_FILENAME_EXCED_RANGE, parameterName );
            }

            if ( length > 1 && buffer[ 0 ].IsDirectorySeparator() && buffer[ 1 ].IsDirectorySeparator() ) {
                if ( length < 2 ) {
                    throw new ArgumentException( @"The UNC path should be of the form \\server\share." );
                }

                var parts = buffer.ToString().Split( new[] {
                    DirectorySeparatorChar
                }, StringSplitOptions.RemoveEmptyEntries );

                if ( parts.Length < 2 ) {
                    throw new ArgumentException( @"The UNC path should be of the form \\server\share." );
                }
            }

            return buffer.ToString().AddLongPathPrefix();
        }

        [NotNull]
        public static String RemoveLongPathPrefix( [NotNull] this String normalizedPath ) {

            if ( String.IsNullOrWhiteSpace( normalizedPath ) || !normalizedPath.StartsWith( LongPathPrefix ) ) {
                return normalizedPath;
            }

            if ( normalizedPath.StartsWith( UNCLongPathPrefix, StringComparison.Ordinal ) ) {
                return $@"\\{normalizedPath.Substring( UNCLongPathPrefix.Length )}";
            }

            return normalizedPath.Substring( LongPathPrefix.Length );
        }

        public static Boolean TryNormalizeLongPath( [NotNull] this String path, [CanBeNull] out String result ) {
            if ( !String.IsNullOrWhiteSpace( value: path ) ) {
                try {
                    result = path.NormalizeLongPath();

                    return true;
                }
                catch ( ArgumentException ) { }
                catch ( PathTooLongException ) { }
            }

            result = null;

            return false;
        }
    }
}