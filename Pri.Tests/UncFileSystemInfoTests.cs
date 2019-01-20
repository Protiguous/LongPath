// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "UncFileSystemInfoTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "UncFileSystemInfoTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:16 PM.

namespace Tests {

	using System;
	using System.Diagnostics;
	using System.Text;
	using NUnit.Framework;
	using Pri.LongPath;
	using Directory = System.IO.Directory;
	using File = System.IO.File;

	[TestFixture]
	public class UncFileSystemInfoTests {

		[SetUp]
		public void SetUp() {
			directory = TestContext.CurrentContext.TestDirectory.Combine( "subdir" );
			Directory.CreateDirectory( directory );

			try {
				uncDirectory = UncHelper.GetUncFromPath( directory );
				filePath = new StringBuilder( directory ).Append( @"\" ).Append( Filename ).ToString();
				uncFilePath = UncHelper.GetUncFromPath( filePath );

				using ( var writer = File.CreateText( filePath ) ) {
					writer.WriteLine( "test" );
				}

				Debug.Assert( Pri.LongPath.File.Exists( uncFilePath ) );
			}
			catch ( Exception ) {
				if ( Directory.Exists( directory ) ) {
					Directory.Delete( directory, true );
				}

				throw;
			}
		}

		[TearDown]
		public void TearDown() {
			try {
				if ( Pri.LongPath.File.Exists( filePath ) ) {
					Pri.LongPath.File.Delete( filePath );
				}
			}
			catch ( Exception e ) {
				Trace.WriteLine( "Exception {0} deleting \"filePath\"", e.ToString() );

				throw;
			}
			finally {
				if ( directory.Exists() ) {
					Pri.LongPath.Directory.Delete( directory, true );
				}
			}
		}

		private const String Filename = "filename.ext";

		private static String directory;

		private static String filePath;

		private static String uncDirectory;

		private static String uncFilePath;

		[Test]
		public void TestExtension() {
			var fi = new FileInfo( filePath );
			Assert.AreEqual( ".ext", fi.Extension );
		}
	}
}