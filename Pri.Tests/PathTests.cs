// Copyright � Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "PathTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "PathTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:14 PM.

namespace Tests {

	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using NUnit.Framework;
	using Pri.LongPath;
	using Directory = Pri.LongPath.Directory;
	using File = Pri.LongPath.File;
	using Path = Pri.LongPath.Path;

	[TestFixture]
	public class PathTests {

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
		public void GetDirectoryNameOnRelativePath() {
			const String input = @"foo\bar\baz";
			const String expected = @"foo\bar";
			var actual = input.GetDirectoryName();
			Assert.AreEqual( expected, actual );
		}

		[Test]
		public void GetDirectoryNameOnRelativePathWithNoParent() {
			const String input = @"foo";
			const String expected = @"";
			var actual = input.GetDirectoryName();
			Assert.AreEqual( expected, actual );
		}

		[Test]
		public void GetFileNameReturnsNullWithNullParameter() => Assert.IsNull( Path.GetFileName( null ) );

		[Test]
		public void TestAltDirectorySeparatorChar() => Assert.AreEqual( System.IO.Path.AltDirectorySeparatorChar, Path.AltDirectorySeparatorChar );

		[Test]
		public void TestChangeExtension() {
			var filename = longPathDirectory.Combine( "filename.ext" );
			var expectedFilenameWithNewExtension = longPathDirectory.Combine( "filename.txt" );

			Assert.AreEqual( expectedFilenameWithNewExtension, filename.ChangeExtension( ".txt" ) );
		}

		[Test]
		public void TestCombine() {
			const String expected = @"c:\Windows\system32";
			var actual = @"c:\Windows".Combine( "system32" );
			Assert.AreEqual( expected, actual );
		}

		[Test]
		public void TestCombineArray() {
			var strings = new[] {
				longPathDirectory, "subdir1", "subdir2", "filename.ext"
			};

			Assert.AreEqual( longPathDirectory.Combine( "subdir1" ).Combine( "subdir2" ).Combine( "filename.ext" ), Path.Combine( strings ) );
		}

		[Test]
		public void TestCombineArrayNullPath() => Assert.Throws<ArgumentNullException>( () => Path.Combine( null ) );

		[Test]
		public void TestCombineArrayOnePath() {
			var strings = new[] {
				longPathDirectory
			};

			Assert.AreEqual( longPathDirectory, Path.Combine( strings ) );
		}

		[Test]
		public void TestCombineArrayTwoPaths() {
			var strings = new[] {
				longPathDirectory, "filename.ext"
			};

			Assert.AreEqual( longPathDirectory.Combine( "filename.ext" ), Path.Combine( strings ) );
		}

		[Test]
		public void TestCombineFourPaths() =>
			Assert.AreEqual( longPathDirectory.Combine( "subdir1" ).Combine( "subdir2" ).Combine( "filename.ext" ),
				longPathDirectory.Combine( "subdir1", "subdir2", "filename.ext" ) );

		[Test]
		public void TestCombineFourPathsFourNulls() => Assert.Throws<ArgumentNullException>( () => Path.Combine( null, null, null, null ) );

		[Test]
		public void TestCombineFourPathsOneNull() => Assert.Throws<ArgumentNullException>( () => longPathDirectory.Combine( "subdir1", "subdir2", null ) );

		[Test]
		public void TestCombineFourPathsThreeNulls() => Assert.Throws<ArgumentNullException>( () => longPathDirectory.Combine( null, null, null ) );

		[Test]
		public void TestCombineFourPathsTwoNull() => Assert.Throws<ArgumentNullException>( () => longPathDirectory.Combine( "subdir1", null, null ) );

		[Test]
		public void TestCombineRelativePaths() {
			const String expected = @"foo\bar\baz\test";
			var actual = @"foo\bar".Combine( @"baz\test" );
			Assert.AreEqual( expected, actual );
		}

		[Test]
		public void TestCombineThreePaths() =>
			Assert.AreEqual( longPathDirectory.Combine( "subdir1" ).Combine( "filename.ext" ), longPathDirectory.Combine( "subdir1", "filename.ext" ) );

		[Test]
		public void TestCombineThreePathsOneNull() => Assert.Throws<ArgumentNullException>( () => longPathDirectory.Combine( "subdir1", null ) );

		[Test]
		public void TestCombineThreePathsThreeNulls() => Assert.Throws<ArgumentNullException>( () => Path.Combine( null, null, null ) );

		[Test]
		public void TestCombineThreePathsTwoNulls() => Assert.Throws<ArgumentNullException>( () => longPathDirectory.Combine( null, null ) );

		[Test]
		public void TestCombineTwoPathsOneNull() => Assert.Throws<ArgumentNullException>( () => longPathDirectory.Combine( null ) );

		[Test]
		public void TestCombineWithEmpthPath1() => Assert.AreEqual( "test", "test".Combine( String.Empty ) );

		[Test]
		public void TestCombineWithEmpthPath1EndingInSeparator() => Assert.AreEqual( @"C:\test\test2", @"C:\test\".Combine( "test2" ) );

		[Test]
		public void TestCombineWithEmpthPath2() => Assert.AreEqual( @"C:\test", String.Empty.Combine( @"C:\test" ) );

		[Test]
		public void TestCombineWithNull() => Assert.Throws<ArgumentNullException>( () => Path.Combine( null, null ) );

		[Test]
		public void TestDirectorySeparatorChar() => Assert.AreEqual( System.IO.Path.DirectorySeparatorChar, Path.DirectorySeparatorChar );

		[Test]
		public void TestGetDirectoryNameAtRoot() {
			const String path = @"c:\";
			Assert.IsNull( path.GetDirectoryName() );
		}

		[Test]
		public void TestGetDirectoryNameWithNullPath() => Assert.Throws<ArgumentNullException>( () => Path.GetDirectoryName( null ) );

		[Test]
		public void TestGetExtension() {
			var tempLongPathFilename = longPathDirectory.Combine( Path.GetRandomFileName() );
			Assert.AreEqual( tempLongPathFilename.Substring( tempLongPathFilename.Length - 4, 4 ), tempLongPathFilename.GetExtension() );
		}

		[Test]
		public void TestGetFileNameWithoutExtension() {
			var filename = longPathDirectory.Combine( "filename.ext" );

			Assert.AreEqual( "filename", filename.GetFileNameWithoutExtension() );
		}

		[Test]
		public void TestGetInvalidFileNameChars() => Assert.IsTrue( Path.GetInvalidFileNameChars().SequenceEqual( System.IO.Path.GetInvalidFileNameChars() ) );

		[Test]
		public void TestGetInvalidPathChars() => Assert.IsTrue( Path.GetInvalidPathChars().SequenceEqual( System.IO.Path.GetInvalidPathChars() ) );

		[Test]
		public void TestGetParentAtRoot() {
			const String path = @"c:\";
			var parent = Directory.GetParent( path );
			Assert.IsNull( parent );
		}

		[Test]
		public void TestGetPathRoot() {
			var root = longPathDirectory.GetPathRoot();
			Assert.IsNotNull( root );
			Assert.AreEqual( 3, root.Length );
		}

		[Test]
		public void TestGetPathRootWithNullPath() {
			var root = Path.GetPathRoot( null );
			Assert.IsNull( root );
		}

		[Test]
		public void TestGetPathRootWithRelativePath() {
			var root = @"foo\bar\baz".GetPathRoot();
			Assert.IsNotNull( root );
			Assert.AreEqual( 0, root.Length );
		}

		[Test]
		public void TestGetRootLength() => Assert.AreEqual( 3, longPathFilename.GetRootLength() );

		[Test]
		public void TestGetRootLengthWithUnc() => Assert.AreEqual( 23, @"\\servername\sharename\dir\filename.exe".GetRootLength() );

		[Test]
		public void TestGetTempFilename() {
			var filename = Path.GetTempFileName();
			Assert.IsNotNull( filename );
			Assert.IsTrue( filename.Length > 0 );
		}

		[Test]
		public void TestGetTempPath() {
			var path = Path.GetTempPath();
			Assert.IsNotNull( path );
			Assert.IsTrue( path.Length > 0 );
		}

		[Test]
		public void TestHasExtensionWithExtension() => Assert.IsTrue( longPathFilename.HasExtension() );

		[Test]
		public void TestHasExtensionWithoutExtension() => Assert.IsFalse( longPathDirectory.HasExtension() );

		[Test]
		public void TestIsDirectorySeparator() {
			Assert.IsTrue( System.IO.Path.DirectorySeparatorChar.IsDirectorySeparator() );
			Assert.IsTrue( System.IO.Path.AltDirectorySeparatorChar.IsDirectorySeparator() );
		}

		[Test]
		public void TestLongPathDirectoryName() {
			var x =
				@"C:\Vault Data\w\M\Access Midstream\9305 Hopeton Stabilizer Upgrades\08  COMMUNICATION\8.1  Transmittals\9305-005 Access Midstream Hopeton - Electrical Panel Wiring dwgs\TM-9305-005-Access Midstream-Hopeton Stabilizer Upgrades-Electrical Panel Wiring-IFC Revised.msg"
					.GetDirectoryName();
		}

		[Test]
		public void TestLongPathDirectoryNameWithInvalidChars() => Assert.Throws<ArgumentException>( () => ( longPathDirectory + '<' ).GetDirectoryName() );

		[Test]
		public void TestNormalizeLongPath() {
			var result = longPathDirectory.NormalizeLongPath();
			Assert.IsNotNull( result );
		}

		[Test]
		public void TestNormalizeLongPathWith() {
			var result = longPathDirectory.NormalizeLongPath();
			Assert.IsNotNull( result );
		}

		[Test]
		public void TestNormalizeLongPathWithEmptyPath() => Assert.IsFalse( String.Empty.TryNormalizeLongPath( out var path ) );

		[Test]
		public void TestNormalizeLongPathWithHugePath() {
			var path = @"c:\";
			var component = Util.MakeLongComponent( path );
			component = component.Substring( 3, component.Length - 3 );

			while ( path.Length < 32000 ) {
				path = path.Combine( component );
			}

			Assert.Throws<PathTooLongException>( () => path.NormalizeLongPath() );
		}

		[Test]
		public void TestNormalizeLongPathWithJustUncPrefix() => Assert.Throws<ArgumentException>( () => @"\\".NormalizeLongPath() );

		[Test]
		public void TestTryNormalizeLongPat() {
			Assert.IsTrue( longPathDirectory.TryNormalizeLongPath( out var path ) );
			Assert.IsNotNull( path );
		}

		[Test]
		public void TestTryNormalizeLongPathWithJustUncPrefix() => Assert.IsFalse( @"\\".TryNormalizeLongPath( out var path ) );

		[Test]
		public void TestTryNormalizeLongPathWithNullPath() => Assert.IsFalse( Path.TryNormalizeLongPath( null, out var path ) );
	}
}