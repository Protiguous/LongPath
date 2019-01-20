// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "FileSystemInfoTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "FileSystemInfoTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:13 PM.

namespace Tests {

    using System;
    using System.Diagnostics;
    using System.Text;
    using NUnit.Framework;
    using Pri.LongPath;

    [TestFixture]
	public class FileSystemInfoTests {

		[SetUp]
		public void SetUp() {
			rootTestDir = TestContext.CurrentContext.TestDirectory;
			longPathDirectory = Util.MakeLongPath( rootTestDir );

			longPathRoot = longPathDirectory.Substring( 0,
				TestContext.CurrentContext.TestDirectory.Length + 1 + longPathDirectory.Substring( rootTestDir.Length + 1 ).IndexOf( '\\' ) );

			longPathDirectory.CreateDirectory();
			Debug.Assert( longPathDirectory.Exists() );
			longPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( Filename ).ToString();

			using ( var writer = File.CreateText( longPathFilename ) ) {
				writer.WriteLine( "test" );
			}

			Debug.Assert( File.Exists( longPathFilename ) );
		}

		[TearDown]
		public void TearDown() {
			try {
				if ( File.Exists( longPathFilename ) ) {
					File.Delete( longPathFilename );
				}
			}
			catch ( Exception e ) {
				Trace.WriteLine( "Exception {0} deleting \"longPathFilename\"", e.ToString() );

				throw;
			}
			finally {
				if ( longPathRoot.Exists() ) {
					Directory.Delete( longPathRoot, true );
				}
			}
		}

		private const String Filename = "filename.ext";

		private static String longPathDirectory;

		private static String longPathFilename;

		private static String longPathRoot;

		private static String rootTestDir;

		[Test]
		public void TestExtension() {
			var fi = new FileInfo( longPathFilename );
			Assert.AreEqual( ".ext", fi.Extension );
		}
	}
}