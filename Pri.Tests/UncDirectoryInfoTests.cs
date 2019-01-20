// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "UncDirectoryInfoTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "UncDirectoryInfoTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:15 PM.

namespace Tests {

	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Security.AccessControl;
	using System.Security.Principal;
	using System.Text;
	using NUnit.Framework;
	using Pri.LongPath;
	using Directory = System.IO.Directory;
	using DirectoryInfo = Pri.LongPath.DirectoryInfo;
	using File = System.IO.File;
	using FileInfo = Pri.LongPath.FileInfo;
	using Path = Pri.LongPath.Path;

	[TestFixture]
	public class UncDirectoryInfoTests {

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
				Pri.LongPath.File.Delete( filePath );
			}
			catch ( Exception e ) {
				Trace.WriteLine( "Exception {0} deleting \"filePath\"", e.ToString() );

				throw;
			}
			finally {
				Pri.LongPath.Directory.Delete( directory, true );
			}
		}

		private const String Filename = "filename.ext";

		private static String directory;

		private static String filePath;

		private static String uncDirectory;

		private static String uncFilePath;

		[Test]
		public void TestConstructorWithNullPath() => Assert.Throws<ArgumentNullException>( () => new DirectoryInfo( null ) );

		[Test]
		public void TestCreate() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			var di = new DirectoryInfo( tempLongPathFilename );
			di.Create();

			try {
				Assert.IsTrue( di.Exists );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestCreateInvalidSubdirectory() {
			var di = new DirectoryInfo( uncDirectory );

			Assert.Throws<ArgumentException>( () => {
				var newDi = di.CreateSubdirectory( @"\" );
			} );
		}

		[Test]
		public void TestCreateSubdirectory() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				Assert.IsNotNull( newDi );
				Assert.IsTrue( di.Exists );
			}
			finally {
				newDi.Delete();
			}
		}

		/// <remarks>
		///     TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[Test]
		public void TestCreateSubdirectoryWithFileSecurity() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			DirectoryInfo newDi = null;

			try {
				newDi = di.CreateSubdirectory( randomFileName, new DirectorySecurity() );
				Assert.IsNotNull( newDi );
				Assert.IsTrue( di.Exists );
			}
			finally {
				newDi?.Delete();
			}
		}

		/// <remarks>
		///     TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[Test]
		public void TestCreateWithFileSecurity() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			var di = new DirectoryInfo( tempLongPathFilename );

			try {
				di.Create( new DirectorySecurity() );
				Assert.IsTrue( tempLongPathFilename.Exists() );
			}
			finally {
				di.Delete();
			}
		}

		[Test]
		public void TestEnumerateDirectories() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.EnumerateDirectories().ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesSearchWithNoResults() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				Assert.AreEqual( 0, di.EnumerateDirectories( "gibberish*" ).Count() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearch() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.EnumerateDirectories( "*" ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchAndOption() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.EnumerateDirectories( "*", SearchOption.TopDirectoryOnly ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchRecursive() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.EnumerateDirectories( "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchRecursiveNoResults() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				Assert.AreEqual( 0, di.EnumerateDirectories( "gibberish*", SearchOption.AllDirectories ).Count() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchWithNoResults() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.EnumerateDirectories( "gibberish" ).ToArray();
				Assert.AreEqual( 0, dirs.Length );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithWildcardSearchAndOptionNoResults() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				Assert.AreEqual( 0, di.EnumerateDirectories( "gibberish*", SearchOption.TopDirectoryOnly ).Count() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateFiles() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 1, di.EnumerateFiles().Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFilesRecursiveWithSearch() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 2, di.EnumerateFiles( "*", SearchOption.AllDirectories ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFilesSearchWithNoResults() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				Assert.AreEqual( 0, di.EnumerateFiles( "gibberish*" ).Count() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateFilesSearchWithResults() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var files = di.EnumerateFiles( "*" ).ToArray();
				Assert.AreEqual( 1, files.Length );
				Assert.IsTrue( files.Any( f => f.Name == Filename ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearch() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 1, newDi.EnumerateFiles( "*" ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearchAndOption() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 1, di.EnumerateFiles( "*", SearchOption.TopDirectoryOnly ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearchAndOptionNoResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 0, di.EnumerateFiles( "gibberish", SearchOption.TopDirectoryOnly ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearchNoResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 0, newDi.EnumerateFiles( "gibberish" ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearchRecursiveAndOption() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 0, di.EnumerateFiles( "gibberish", SearchOption.AllDirectories ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfos() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 1, newDi.EnumerateFileSystemInfos().Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearch() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 2, di.EnumerateFileSystemInfos( "*" ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearchAndOptionMultipleResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 2, di.EnumerateFileSystemInfos( "*" ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearchAndOptionNoResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 0, di.EnumerateFileSystemInfos( "gibberish" ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearchNoResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 0, di.EnumerateFileSystemInfos( "gibberish" ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestEnumerateFileSystemInfosWithSearchRecursiveNoResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 0, di.EnumerateFileSystemInfos( "gibberish", SearchOption.AllDirectories ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestExistsNonexistentDirectory() {
			var di = new DirectoryInfo( "gibberish" );
			Assert.IsFalse( di.Exists );
		}

		[Test]
		public void TestExistsNonexistentParentDirectory() {
			var fi = new FileInfo( @"C:\.w\.y" );

			Assert.IsFalse( fi.Directory.Exists );
		}

		[Test]
		public void TestExistsOnExistantDirectory() => Assert.IsTrue( new DirectoryInfo( uncDirectory ).Exists );

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControl() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( tempLongPathFilename );
				var security = di.GetAccessControl();
				Assert.IsNotNull( security );
				Assert.AreEqual( typeof( FileSystemRights ), security.AccessRightType );
				Assert.AreEqual( typeof( FileSystemAccessRule ), security.AccessRuleType );
				Assert.AreEqual( typeof( FileSystemAuditRule ), security.AuditRuleType );
				Assert.IsTrue( security.AreAccessRulesCanonical );
				Assert.IsTrue( security.AreAuditRulesCanonical );
				Assert.IsFalse( security.AreAccessRulesProtected );
				Assert.IsFalse( security.AreAuditRulesProtected );
				var perm = security.GetAccessRules( true, true, typeof( NTAccount ) );
				var ntAccount = new NTAccount( WindowsIdentity.GetCurrent().Name );
				var rule = perm.Cast<FileSystemAccessRule>().SingleOrDefault( e => ntAccount == e.IdentityReference );
				Assert.IsNotNull( rule );
				Assert.IsTrue( ( rule.FileSystemRights & FileSystemRights.FullControl ) != 0 );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControlSections() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( tempLongPathFilename );
				var security = di.GetAccessControl( AccessControlSections.Access );
				Assert.IsNotNull( security );
				Assert.AreEqual( typeof( FileSystemRights ), security.AccessRightType );
				Assert.AreEqual( typeof( FileSystemAccessRule ), security.AccessRuleType );
				Assert.AreEqual( typeof( FileSystemAuditRule ), security.AuditRuleType );
				Assert.IsTrue( security.AreAccessRulesCanonical );
				Assert.IsTrue( security.AreAuditRulesCanonical );
				Assert.IsFalse( security.AreAccessRulesProtected );
				Assert.IsFalse( security.AreAuditRulesProtected );
				var securityGetAccessRules = security.GetAuditRules( true, true, typeof( NTAccount ) ).Cast<FileSystemAccessRule>();
				Assert.AreEqual( 0, securityGetAccessRules.Count() );
				var perm = security.GetAccessRules( true, true, typeof( NTAccount ) );
				var ntAccount = new NTAccount( WindowsIdentity.GetCurrent().Name );
				var rule = perm.Cast<FileSystemAccessRule>().SingleOrDefault( e => ntAccount == e.IdentityReference );
				Assert.IsNotNull( rule );
				Assert.IsTrue( ( rule.FileSystemRights & FileSystemRights.FullControl ) != 0 );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetDirectories() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.GetDirectories();
				Assert.AreEqual( 1, dirs.Count() );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetDirectoriesWithAllSearch() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.GetDirectories( "*" ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetDirectoriesWithMultipleResultSubsetSearch() {
			const String randomFileName = "TestGetDirectoriesWithMultipleResultSubsetSearch";
			const String randomFileName2 = "ATestGetDirectoriesWithMultipleResultSubsetSearch";
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			var tempLongPathFilename2 = uncDirectory.Combine( randomFileName2 );
			tempLongPathFilename.CreateDirectory();
			tempLongPathFilename2.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.GetDirectories( "A*" ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName2 ) );
				Assert.IsFalse( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
				Pri.LongPath.Directory.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestGetDirectoriesWithSingleResultSubsetSearch() {
			const String randomFileName = "TestGetDirectoriesWithSubsetSearch";
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.GetDirectories( "A*" ).ToArray();
				Assert.AreEqual( 0, dirs.Length );
				Assert.IsFalse( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetFiles() {
			var di = new DirectoryInfo( uncDirectory );
			var files = di.GetFiles().ToArray();
			Assert.AreEqual( 1, files.Length );
			Assert.IsTrue( files.Any( f => f.Name == Filename ) );
		}

		[Test]
		public void TestGetFilesWithSearch() {
			var di = new DirectoryInfo( uncDirectory );
			var files = di.GetFiles( "*" ).ToArray();
			Assert.AreEqual( 1, files.Length );
		}

		[Test]
		public void TestGetFilesWithSearchWithNoResults() {
			var di = new DirectoryInfo( uncDirectory );
			var files = di.GetFiles( "giberish" ).ToArray();
			Assert.AreEqual( 0, files.Length );
		}

		[Test]
		public void TestGetFileSystemInfos() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 1, newDi.GetFileSystemInfos().Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestGetFileSystemInfosWithSearch() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 2, di.GetFileSystemInfos( "*" ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestGetFileSystemInfosWithSearchAndOptionMultipleResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 2, di.GetFileSystemInfos( "*", SearchOption.TopDirectoryOnly ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestGetFileSystemInfosWithSearchAndOptionNoResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 0, di.GetFileSystemInfos( "gibberish", SearchOption.TopDirectoryOnly ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestGetFileSystemInfosWithSearchNoResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 0, di.GetFileSystemInfos( "gibberish" ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestGetFileSystemInfosWithSearchRecursiveNoResults() {
			var di = new DirectoryInfo( uncDirectory );
			var randomFileName = Path.GetRandomFileName();
			var newDi = di.CreateSubdirectory( randomFileName );

			try {
				var fi = new FileInfo( newDi.FullName.Combine( "filename" ) );

				using ( fi.Create() ) { }

				try {
					Assert.AreEqual( 0, di.GetFileSystemInfos( "gibberish", SearchOption.AllDirectories ).Count() );
				}
				finally {
					fi.Delete();
				}
			}
			finally {
				newDi.Delete( true );
			}
		}

		[Test]
		public void TestGetRecursiveFilesWithAllSearch() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var newEmptyFile = Util.CreateNewEmptyFile( tempLongPathFilename );

				try {
					var randomFileName = newEmptyFile.GetFileName();

					var di = new DirectoryInfo( uncDirectory );
					var files = di.GetFiles( "*", SearchOption.AllDirectories ).ToArray();
					Assert.AreEqual( 2, files.Length );
					Assert.IsTrue( files.Any( f => f.Name == Filename ) );
					Assert.IsTrue( files.Any( f => f.Name == randomFileName ) );
				}
				finally {
					Pri.LongPath.File.Delete( newEmptyFile );
				}
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestGetRecursiveFilesWithSubsetSearch() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var newEmptyFile1 = Util.CreateNewEmptyFile( tempLongPathFilename, "A-file" );
				var newEmptyFile2 = Util.CreateNewEmptyFile( tempLongPathFilename, "B-file" );

				try {
					var randomFileName = newEmptyFile1.GetFileName();

					var di = new DirectoryInfo( uncDirectory );
					var files = di.GetFiles( "A*", SearchOption.AllDirectories ).ToArray();
					Assert.AreEqual( 1, files.Length );
					Assert.IsTrue( files.Any( f => f.Name == newEmptyFile1.GetFileName() && f.DirectoryName == newEmptyFile1.GetDirectoryName() ) );
					Assert.IsFalse( files.Any( f => f.Name == newEmptyFile2.GetFileName() && f.DirectoryName == newEmptyFile2.GetDirectoryName() ) );
					Assert.IsFalse( files.Any( f => f.Name == Filename.GetFileName() && f.DirectoryName == Filename.GetDirectoryName() ) );
				}
				finally {
					Pri.LongPath.File.Delete( newEmptyFile1 );
					Pri.LongPath.File.Delete( newEmptyFile2 );
				}
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestInstantiateWithDrive() {
			var di = new DirectoryInfo( @"C:" );
			Assert.AreEqual( ".", di.Name );
		}

		[Test]
		public void TestMoveTo() {
			var randomFileName = Path.GetRandomFileName();
			var randomFileName2 = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			var tempLongPathFilename2 = uncDirectory.Combine( randomFileName2 );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( tempLongPathFilename );
				di.MoveTo( tempLongPathFilename2 );
				di = new DirectoryInfo( uncDirectory );
				var dirs = di.EnumerateDirectories( "*", SearchOption.TopDirectoryOnly ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName2 ) );
				Assert.IsFalse( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename2 );
			}

			Assert.IsFalse( tempLongPathFilename.Exists() );
		}

		[Test]
		public void TestMoveToDifferentRoot() {
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = uncDirectory.Combine( randomDirectoryName );
			var di = new DirectoryInfo( tempLongPathDirectory );

			Assert.Throws<IOException>( () => di.MoveTo( @"D:\" ) );
		}

		[Test]
		public void TestMoveToEmptyPath() {
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = uncDirectory.Combine( randomDirectoryName );
			var di = new DirectoryInfo( tempLongPathDirectory );

			Assert.Throws<ArgumentException>( () => di.MoveTo( String.Empty ) );
		}

		[Test]
		public void TestMoveToNullPath() {
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = uncDirectory.Combine( randomDirectoryName );
			var di = new DirectoryInfo( tempLongPathDirectory );

			Assert.Throws<ArgumentNullException>( () => di.MoveTo( null ) );
		}

		[Test]
		public void TestMoveToSamePath() {
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = uncDirectory.Combine( randomDirectoryName );
			var di = new DirectoryInfo( tempLongPathDirectory );

			Assert.Throws<IOException>( () => di.MoveTo( tempLongPathDirectory ) );
		}

		[Test]
		public void TestMoveToSamePathWithSlash() {
			var randomDirectoryName = Path.GetRandomFileName();
			var tempLongPathDirectory = uncDirectory.Combine( randomDirectoryName ) + @"\";
			var di = new DirectoryInfo( tempLongPathDirectory );

			Assert.Throws<IOException>( () => di.MoveTo( tempLongPathDirectory ) );
		}

		[Test]
		public void TestParent() {
			var di = new DirectoryInfo( uncDirectory );
			var parent = di.Parent;
			Assert.IsNotNull( parent );
			Assert.AreEqual( uncDirectory.GetDirectoryName(), parent.FullName );
		}

		[Test]
		public void TestParentOnRoot() {
			var di = new DirectoryInfo( @"C:\" );
			var parent = di.Parent;
			Assert.IsNull( parent );
		}

		[Test]
		public void TestParentPathEndingWithSlash() {
			var di = new DirectoryInfo( uncDirectory + @"\" );
			var parent = di.Parent;
			Assert.IsNotNull( parent );
			Assert.AreEqual( uncDirectory.GetDirectoryName(), parent.FullName );
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithSearch() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.EnumerateDirectories( "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithSearchNoResults() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.EnumerateDirectories( "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, dirs.Length );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestRecursiveGetDirectoriesWithSearch() {
			var randomFileName = Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var di = new DirectoryInfo( uncDirectory );
				var dirs = di.GetDirectories( "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Any( f => f.Name == randomFileName ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestRoot() {
			var di = new DirectoryInfo( uncDirectory );
			var root = di.Root;
			Assert.IsNotNull( root );
			Assert.AreEqual( new System.IO.DirectoryInfo( uncDirectory ).Root.FullName, root.FullName );
		}

		[Test]
		public void TestSetCreationTime() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				var di = new DirectoryInfo( filename ) {
					CreationTime = dateTime
				};

				Assert.AreEqual( dateTime, filename.GetCreationTime() );
			}
			finally {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestSetCreationTimeMissingFile() {
			var filename = uncDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.Now.AddDays( 1 );
			var di = new DirectoryInfo( filename );
			Assert.Throws<FileNotFoundException>( () => di.CreationTime = dateTime );
		}

		[Test]
		public void TestSetCreationTimeUtc() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );

				var di = new DirectoryInfo( filename ) {
					CreationTimeUtc = dateTime
				};

				Assert.AreEqual( dateTime, filename.GetCreationTimeUtc() );
			}
			finally {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestSetCreationTimeUtcMissingFile() {
			var filename = uncDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			var di = new DirectoryInfo( filename );
			Assert.Throws<FileNotFoundException>( () => di.CreationTimeUtc = dateTime );
		}

		[Test]
		public void TestSetLastAccessTime() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				var di = new DirectoryInfo( filename ) {
					LastAccessTime = dateTime
				};

				Assert.AreEqual( dateTime, Pri.LongPath.File.GetLastAccessTime( filename ) );
			}
			finally {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastAccessTimeMissingFile() {
			var filename = uncDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.Now.AddDays( 1 );
			var di = new DirectoryInfo( filename );
			Assert.Throws<FileNotFoundException>( () => di.LastAccessTime = dateTime );
		}

		[Test]
		public void TestSetLastAccessTimeUtc() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );

				var di = new DirectoryInfo( filename ) {
					LastAccessTimeUtc = dateTime
				};

				Assert.AreEqual( dateTime, Pri.LongPath.File.GetLastAccessTimeUtc( filename ) );
			}
			finally {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastAccessTimeUtcMissingFile() {
			var filename = uncDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			var di = new DirectoryInfo( filename );
			Assert.Throws<FileNotFoundException>( () => di.LastAccessTimeUtc = dateTime );
		}

		[Test]
		public void TestSetLastWriteTime() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				var di = new DirectoryInfo( filename ) {
					LastWriteTime = dateTime
				};

				Assert.AreEqual( dateTime, Pri.LongPath.File.GetLastWriteTime( filename ) );
			}
			finally {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastWriteTimeMissingFile() {
			var filename = uncDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.Now.AddDays( 1 );
			var di = new DirectoryInfo( filename );
			Assert.Throws<FileNotFoundException>( () => di.LastWriteTime = dateTime );
		}

		[Test]
		public void TestSetLastWriteTimeUtc() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );

				var di = new DirectoryInfo( filename ) {
					LastWriteTimeUtc = dateTime
				};

				Assert.AreEqual( dateTime, Pri.LongPath.File.GetLastWriteTimeUtc( filename ) );
			}
			finally {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastWriteTimeUtcMissingFile() {
			var filename = uncDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			var di = new DirectoryInfo( filename );
			Assert.Throws<FileNotFoundException>( () => di.LastWriteTimeUtc = dateTime );
		}

		[Test]
		public void TestToString() {
			var fi = new DirectoryInfo( uncDirectory );

			Assert.AreEqual( fi.DisplayPath, fi.ToString() );
		}
	}
}