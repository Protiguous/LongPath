// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "Util.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "Util.cs" was last formatted by Protiguous on 2019/01/12 at 8:18 PM.

namespace Tests {

	using System;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using File = Pri.LongPath.File;
	using Path = Pri.LongPath.Path;

	internal static class Util {

		public static String CreateNewEmptyFile( String longPathDirectory ) {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( Path.DirectorySeparatorChar ).Append( Path.GetRandomFileName() ).ToString();

			using ( File.Create( tempLongPathFilename ) ) { }

			return tempLongPathFilename;
		}

		public static String CreateNewEmptyFile( String longPathDirectory, String filename ) {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( Path.DirectorySeparatorChar ).Append( filename ).ToString();

			using ( File.Create( tempLongPathFilename ) ) { }

			return tempLongPathFilename;
		}

		public static String CreateNewFile( String longPathDirectory ) {
			var tempLongPathFilename = CreateNewEmptyFile( longPathDirectory );

			using ( var streamWriter = File.AppendText( tempLongPathFilename ) ) {
				streamWriter.WriteLine( "beginning of file" );
			}

			return tempLongPathFilename;
		}

		public static String CreateNewFileUnicode( String longPathDirectory ) {
			var tempLongPathFilename = CreateNewEmptyFile( longPathDirectory );
			var fileStream = File.Open( tempLongPathFilename, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan );

			using ( var streamWriter = new StreamWriter( fileStream, Encoding.Unicode, 4096, false ) ) {
				streamWriter.WriteLine( "beginning of file" );
			}

			return tempLongPathFilename;
		}

		public static String MakeLongComponent( String path ) {
			var volname = new StringBuilder( 261 );
			var fsname = new StringBuilder( 261 );

			NativeMethods.GetVolumeInformation( System.IO.Path.GetPathRoot( path ), volname, volname.Capacity, out var sernum, out var maxlen, out var flags, fsname,
				fsname.Capacity );

			var componentText = Enumerable.Repeat( "0123456789", ( Int32 ) ( ( maxlen + 10 ) / 10 ) ).Aggregate( ( c, n ) => c + n ).Substring( 0, ( Int32 ) maxlen );
			Debug.Assert( componentText.Length == maxlen );
			var directorySeparatorText = Path.DirectorySeparatorChar.ToString( CultureInfo.InvariantCulture );
			var endsWith = path.EndsWith( directorySeparatorText );

			var resultPath = new StringBuilder( path ).Append( endsWith ? String.Empty : directorySeparatorText ).Append( componentText ).Append( Path.DirectorySeparatorChar )
				.Append( componentText ).ToString();

			Debug.Assert( resultPath.Length > 260 );

			return resultPath;
		}

		public static String MakeLongPath( String path ) {
			var volname = new StringBuilder( 261 );
			var fsname = new StringBuilder( 261 );

			if ( !NativeMethods.GetVolumeInformation( System.IO.Path.GetPathRoot( path ), volname, volname.Capacity, out var sernum, out var maxlen, out var flags, fsname,
				fsname.Capacity ) ) {
				maxlen = 255;
			}

			var componentText = Enumerable.Repeat( "0123456789", ( Int32 ) ( ( maxlen + 10 ) / 10 ) ).Aggregate( ( c, n ) => c + n ).Substring( 0, ( Int32 ) maxlen );
			Debug.Assert( componentText.Length == maxlen );
			var directorySeparatorText = Path.DirectorySeparatorChar.ToString( CultureInfo.InvariantCulture );
			var endsWith = path.EndsWith( directorySeparatorText );

			var resultPath = new StringBuilder( path ).Append( endsWith ? String.Empty : directorySeparatorText ).Append( componentText ).Append( Path.DirectorySeparatorChar )
				.Append( componentText ).ToString();

			Debug.Assert( resultPath.Length > 260 );

			return resultPath;
		}

		public static Boolean VerifyContentsOfNewFile( String path ) {
			var contents = File.ReadAllText( path );

			return "beginning of file" + Environment.NewLine == contents;
		}
	}
}