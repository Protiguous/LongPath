// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "DirectoryTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "DirectoryTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:13 PM.

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
	using Directory = Pri.LongPath.Directory;
	using DirectoryInfo = Pri.LongPath.DirectoryInfo;
	using File = Pri.LongPath.File;
	using Path = System.IO.Path;

	[TestFixture]
	public class DirectoryTests {

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

		private static String rootTestDir;

		private static String longPathDirectory;

		private static String longPathFilename;

		private static String longPathRoot;

		private const String Filename = "filename.ext";

		[Test]
		public void PathGetDirectoryNameReturnsSameResultAsBclForRelativePath() {
			var text = Path.GetDirectoryName( @"foo\bar\baz" );
			Assert.AreEqual( @"foo\bar", text );
		}

		/// <remarks>
		///     TODO: test some error conditions.
		/// </remarks>
		[Test]
		public void TestCreateDirectory() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			var di = tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsNotNull( di );
				Assert.IsTrue( tempLongPathFilename.Exists() );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestCreateDirectoryThatAlreadyExists() {
			var di = longPathDirectory.CreateDirectory();
			Assert.IsNotNull( di );
			Assert.IsTrue( longPathDirectory.Exists() );
		}

		[Test]
		public void TestCreateDirectoryThatEndsWithSlash() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() + @"\" );
			var di = tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsNotNull( di );
				Assert.IsTrue( tempLongPathFilename.Exists() );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		/// <remarks>
		///     TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[Test]
		public void TestCreateWithFileSecurity() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );

			try {
				Directory.CreateDirectory( tempLongPathFilename, new DirectorySecurity() );
				Assert.IsTrue( tempLongPathFilename.Exists() );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestCurrentDirectory() {
			var di = new DirectoryInfo( "." );
			Assert.AreEqual( di.FullName, Directory.GetCurrentDirectory() );
		}

		[Test]
		public void TestDeleteDirectory() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename.GetFullPath().Exists() );
			Directory.Delete( tempLongPathFilename );
			Assert.IsFalse( tempLongPathFilename.GetFullPath().Exists() );
		}

		/// <summary>
		///     Tests the Directory.Delete where 'path' is a junction point.
		/// </summary>
		[Test]
		public void TestDeleteDirectory_JunctionPoint() {
			var targetFolder = rootTestDir.Combine( "ADirectory" );
			var junctionPoint = rootTestDir.Combine( "SymLink" );

			targetFolder.CreateDirectory();

			try {

				var targetFile = targetFolder.Combine( "AFile" );

				File.Create( targetFile ).Close();

				try {
					JunctionPoint.Create( junctionPoint, targetFolder, overwrite: false );
					Assert.IsTrue( File.Exists( targetFolder.Combine( "AFile" ) ), "File should be accessible." );
					Assert.IsTrue( File.Exists( junctionPoint.Combine( "AFile" ) ), "File should be accessible via the junction point." );

					Directory.Delete( junctionPoint, false );

					Assert.IsTrue( File.Exists( targetFolder.Combine( "AFile" ) ), "File should be accessible." );
					Assert.IsFalse( JunctionPoint.Exists( junctionPoint ), "Junction point should not exist now." );
					Assert.IsTrue( !File.Exists( junctionPoint.Combine( "AFile" ) ), "File should not be accessible via the junction point." );
				}
				finally {
					File.Delete( targetFile );
				}
			}
			finally {
				Directory.Delete( targetFolder );
			}
		}

        /// <remarks>
        ///     Tests <see cref="Pri.LongPath.Directory.EnumerateDirectories(String)" />, depends on
        ///     <see cref="Directory.CreateDirectory(string)" />
        /// </remarks>
        [Test]
		public void TestEnumerateDirectories() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = longPathDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Directory.EnumerateDirectories( longPathDirectory ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearch() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = longPathDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Directory.EnumerateDirectories( longPathDirectory, "*" ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchWithNoResults() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = longPathDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Directory.EnumerateDirectories( longPathDirectory, "gibberish" ).ToArray();
				Assert.AreEqual( 0, dirs.Length );
				Assert.IsFalse( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

        /// <remarks>
        ///     Tests <see cref="Pri.LongPath.Directory.EnumerateDirectories(String)" />, depends on
        ///     <see cref="Directory.CreateDirectory(string)" />
        /// </remarks>
        [Test]
		public void TestEnumerateFiles() {
			var files = Directory.EnumerateFiles( longPathDirectory ).ToArray();
			Assert.AreEqual( 1, files.Length );
			Assert.IsTrue( files.Contains( longPathFilename ) );
		}

		[Test]
		public void TestEnumerateFilesRecursiveWithSearchWithNoResults() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Directory.EnumerateFiles( longPathDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, files.Length );
				Assert.IsFalse( files.Contains( longPathFilename ) );
				Assert.IsFalse( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearch() {
			var files = Directory.EnumerateFiles( longPathDirectory, "*" ).ToArray();
			Assert.AreEqual( 1, files.Length );
			Assert.IsTrue( files.Contains( longPathFilename ) );
		}

		[Test]
		public void TestEnumerateFilesWithSearchWithNoResults() {
			var files = Directory.EnumerateFiles( longPathDirectory, "giberish" ).ToArray();
			Assert.AreEqual( 0, files.Length );
			Assert.IsFalse( files.Contains( longPathFilename ) );
		}

		[Test]
		public void TestEnumerateFileSystemEntries() {
			var entries = Directory.EnumerateFileSystemEntries( longPathDirectory ).ToArray();
			Assert.AreEqual( 1, entries.Length );
			Assert.IsTrue( entries.Contains( longPathFilename ) );
		}

		[Test]
		public void TestEnumerateFileSystemEntriesRecursiveWithSearchWithNoResults() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Directory.EnumerateFileSystemEntries( longPathDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, files.Length );
				Assert.IsFalse( files.Contains( longPathFilename ) );
				Assert.IsFalse( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestEnumerateFileSystemEntriesWithSearch() {
			var entries = Directory.EnumerateFileSystemEntries( longPathDirectory, "*" ).ToArray();
			Assert.AreEqual( 1, entries.Length );
			Assert.IsTrue( entries.Contains( longPathFilename ) );
		}

		[Test]
		public void TestEnumerateFileSystemEntriesWithSearchWithNoResults() {
			var entries = Directory.EnumerateFileSystemEntries( longPathDirectory, "giberish" ).ToArray();
			Assert.AreEqual( 0, entries.Length );
			Assert.IsFalse( entries.Contains( longPathFilename ) );
		}

		[Test]
		public void TestEnumerateRecursiveFilesWithSearch() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Directory.EnumerateFiles( longPathDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 2, files.Length );
				Assert.IsTrue( files.Contains( longPathFilename ) );
				Assert.IsTrue( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestEnumerateRecursiveFileSystemEntriesWithSearch() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var entries = Directory.EnumerateFileSystemEntries( longPathDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 3, entries.Length );
				Assert.IsTrue( entries.Contains( longPathFilename ) );
				Assert.IsTrue( entries.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestExists() => Assert.IsTrue( longPathDirectory.Exists() );

		[Test]
		public void TestExistsOnFile() => Assert.IsFalse( new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "does-not-exist" ).ToString().Exists() );

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControl() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var security = Directory.GetAccessControl( tempLongPathFilename );
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
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControlSections() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var security = Directory.GetAccessControl( tempLongPathFilename, AccessControlSections.Access );
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
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetCreationTime() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Directory.GetCreationTime( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.CreationTime, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		/// <remarks>
		///     TODO: test some error conditions.
		/// </remarks>
		[Test]
		public void TestGetCreationTimeUTc() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Directory.GetCreationTimeUtc( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.CreationTimeUtc, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetDirectories() => Assert.AreEqual( 0, Directory.GetDirectories( longPathDirectory ).Count() );

		[Test]
		public void TestGetDirectoriesWithAnySearch() {
			var tempLongPathFilename = longPathDirectory.Combine( "TestGetDirectoriesWithAnySearch" );
			tempLongPathFilename.CreateDirectory();
			var tempLongPathFilename2 = longPathDirectory.Combine( "ATestGetDirectoriesWithAnySearch" );
			tempLongPathFilename2.CreateDirectory();

			try {
				Assert.AreEqual( 2, Directory.GetDirectories( longPathDirectory, "*" ).Count() );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
				Directory.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestGetDirectoriesWithSearchWithNoResults() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = longPathDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Directory.GetDirectories( longPathDirectory, "gibberish" ).ToArray();
				Assert.AreEqual( 0, dirs.Length );
				Assert.IsFalse( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetDirectoriesWithSubsetSearch() {
			var tempLongPathFilename = longPathDirectory.Combine( "TestGetDirectoriesWithSubsetSearch" );
			tempLongPathFilename.CreateDirectory();
			var tempLongPathFilename2 = longPathDirectory.Combine( "ATestGetDirectoriesWithSubsetSearch" );
			tempLongPathFilename2.CreateDirectory();

			try {
				Assert.AreEqual( 1, Directory.GetDirectories( longPathDirectory, "A*" ).Count() );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
				Directory.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestGetDirectoryRoot() => Assert.AreEqual( longPathDirectory.Substring( 0, 3 ), Directory.GetDirectoryRoot( longPathDirectory ) );

		[Test]
		public void TestGetFiles() {
			Assert.AreNotEqual( 0, longPathDirectory.GetFiles().Count() );
			Assert.AreEqual( 1, longPathDirectory.GetFiles().Count() );
			Assert.IsTrue( longPathDirectory.GetFiles().Contains( longPathFilename ) );
		}

		[Test]
		public void TestGetFilesRecursiveWithSearchWithNoResults() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Directory.GetFiles( longPathDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, files.Length );
				Assert.IsFalse( files.Contains( longPathFilename ) );
				Assert.IsFalse( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestGetFilesWithSearch() {
			var files = Directory.GetFiles( longPathDirectory, "*" ).ToArray();
			Assert.AreEqual( 1, files.Length );
			Assert.IsTrue( files.Contains( longPathFilename ) );
		}

		[Test]
		public void TestGetFilesWithSearchWithNoResults() {
			var files = Directory.GetFiles( longPathDirectory, "giberish" ).ToArray();
			Assert.AreEqual( 0, files.Length );
			Assert.IsFalse( files.Contains( longPathFilename ) );
		}

		[Test]
		public void TestGetFileSystemEntries() {
			var entries = Directory.GetFileSystemEntries( longPathDirectory ).ToArray();
			Assert.AreEqual( 1, entries.Length );
			Assert.IsTrue( entries.Contains( longPathFilename ) );
		}

		[Test]
		public void TestGetFileSystemEntriesRecursiveWithSearchWithNoResults() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Directory.GetFileSystemEntries( longPathDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, files.Length );
				Assert.IsFalse( files.Contains( longPathFilename ) );
				Assert.IsFalse( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestGetFileSystemEntriesWithSearch() {
			var entries = Directory.GetFileSystemEntries( longPathDirectory, "*" ).ToArray();
			Assert.AreEqual( 1, entries.Length );
			Assert.IsTrue( entries.Contains( longPathFilename ) );
		}

		[Test]
		public void TestGetFileSystemEntriesWithSearchWithNoResults() {
			var entries = Directory.GetFileSystemEntries( longPathDirectory, "giberish" ).ToArray();
			Assert.AreEqual( 0, entries.Length );
			Assert.IsFalse( entries.Contains( longPathFilename ) );
		}

		[Test]
		public void TestGetLastAccessTime() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Directory.GetLastAccessTime( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastAccessTime, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetLastAccessTimeUtc() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Directory.GetLastAccessTimeUtc( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastAccessTimeUtc, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetLastWriteTime() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Directory.GetLastWriteTime( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastWriteTime, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetLastWriteTimeUtc() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Directory.GetLastWriteTimeUtc( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastWriteTimeUtc, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetLogicalDrives() {
			var directoryGetLogicalDrives = Directory.GetLogicalDrives();
			Assert.IsNotNull( directoryGetLogicalDrives );
			Assert.IsTrue( directoryGetLogicalDrives.Any() );
		}

		/// <remarks>
		///     Tests Directory.GetParent,
		///     depends on Directory.Combine, DirectoryInfo.FullName
		/// </remarks>
		[Test]
		public void TestGetParent() {
			var actual = Directory.GetParent( longPathDirectory.Combine( "system32" ) );
			Assert.AreEqual( longPathDirectory, actual.FullName );
		}

		[Test]
		public void TestGetRecursiveDirectoriesWithSearch() => Assert.AreEqual( 0, longPathDirectory.GetDirectories( "*", SearchOption.AllDirectories ).Count() );

		[Test]
		public void TestGetRecursiveDirectoriesWithSubsetSearch() {
			var tempLongPathFilename = longPathDirectory.Combine( "TestGetRecursiveDirectoriesWithSubsetSearch" );
			tempLongPathFilename.CreateDirectory();
			var tempLongPathFilename2 = tempLongPathFilename.Combine( "ATestGetRecursiveDirectoriesWithSubsetSearch" );
			tempLongPathFilename2.CreateDirectory();

			try {
				Assert.AreEqual( 1, longPathDirectory.GetDirectories( "A*", SearchOption.AllDirectories ).Count() );
			}
			finally {
				Directory.Delete( tempLongPathFilename2 );
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetRecursiveFilesWithAnySearch() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Directory.GetFiles( longPathDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 2, files.Length );
				Assert.IsTrue( files.Contains( longPathFilename ) );
				Assert.IsTrue( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestGetRecursiveFilesWithSubsetSearch() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var searchPattern = randomFileName.GetFileName().Substring( 0, 3 ) + "*" + randomFileName.GetExtension();

				var files = Directory.GetFiles( longPathDirectory, searchPattern, SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, files.Length );
				Assert.IsFalse( files.Contains( longPathFilename ) );
				Assert.IsTrue( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestGetRecursiveFileSystemEntriesWithSearch() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var entries = Directory.GetFileSystemEntries( longPathDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 3, entries.Length );
				Assert.IsTrue( entries.Contains( longPathFilename ) );
				Assert.IsTrue( entries.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestInUseMove() {
			const Boolean recursive = true;

#if SHORT_SOURCE
			var tempPathFilename1 = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), System.IO.Path.GetRandomFileName());
			System.IO.Directory.CreateDirectory(tempPathFilename1);
			Assert.IsTrue(System.IO.Directory.Exists(Path.GetFullPath(tempPathFilename1)));
			var tempPathFilename2 = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), System.IO.Path.GetRandomFileName());
			System.IO.Directory.CreateDirectory(tempPathFilename2);
			Assert.IsTrue(System.IO.Directory.Exists(System.IO.Path.GetFullPath(tempPathFilename2)));
			try
			{
				using (
					var writer = System.IO.File.CreateText(System.IO.Path.Combine(tempPathFilename2, "TestInUseMove")))
				{
					string destinationPath =
						System.IO.Path.GetFullPath(System.IO.Path.Combine(tempPathFilename1, System.IO.Path.GetFileName(tempPathFilename2)));
					System.IO.Directory.Move(tempPathFilename2, destinationPath);
					Assert.IsTrue(System.IO.Directory.Exists(System.IO.Path.GetFullPath(tempPathFilename1)));
					Assert.IsFalse(System.IO.Directory.Exists(System.IO.Path.GetFullPath(tempPathFilename2)));
					Assert.IsTrue(System.IO.Directory.Exists(destinationPath));
				}
			}
			catch (Exception e)
			{
				throw;
			}
			finally
			{
				Directory.Delete(tempPathFilename1, recursive);
				Directory.Delete(tempPathFilename2, recursive);
			}
#endif
			var tempLongPathFilename1 = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename1.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename1.GetFullPath().Exists() );
			var tempLongPathFilename2 = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename2.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename2.GetFullPath().Exists() );

			try {
				using ( var writer = File.CreateText( tempLongPathFilename2.Combine( "TestInUseMove" ) ) ) {
					var destinationPath = tempLongPathFilename1.Combine( tempLongPathFilename2.GetFileName() ).GetFullPath();
					Assert.Throws<IOException>( () => Directory.Move( tempLongPathFilename2, destinationPath ) );
				}
			}
			finally {
				Directory.Delete( tempLongPathFilename1, recursive );
				Directory.Delete( tempLongPathFilename2, recursive );
			}
		}

		[Test]
		public void TestMove() {
			var tempLongPathFilename1 = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename1.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename1.GetFullPath().Exists() );
			var tempLongPathFilename2 = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename2.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename2.GetFullPath().Exists() );

			var destinationPath = tempLongPathFilename1.Combine( tempLongPathFilename2.GetFileName() ).GetFullPath();
			Directory.Move( tempLongPathFilename2, destinationPath );
			Assert.IsTrue( tempLongPathFilename1.GetFullPath().Exists() );
			Assert.IsFalse( tempLongPathFilename2.GetFullPath().Exists() );
			Assert.IsTrue( destinationPath.Exists() );

			const Boolean recursive = true;
			Directory.Delete( tempLongPathFilename1, recursive );
			Directory.Delete( tempLongPathFilename2, recursive );
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithAllSearch() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = longPathDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Directory.EnumerateDirectories( longPathDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch() {
			var randomFileName = "TestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch";
			var tempLongPathFilename = longPathDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();
			randomFileName = "ATestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch";
			var tempLongPathFilename2 = longPathDirectory.Combine( randomFileName );
			tempLongPathFilename2.CreateDirectory();

			try {
				var dirs = Directory.EnumerateDirectories( longPathDirectory, "T*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempLongPathFilename ) );
				Assert.IsFalse( dirs.Contains( tempLongPathFilename2 ) );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
				Directory.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithSearchNoResults() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = longPathDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Directory.EnumerateDirectories( longPathDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, dirs.Length );
				Assert.IsFalse( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithSingleSubsetSearch() {
			const String randomFileName = "TestRecursiveEnumerateDirectoriesWithSubsetSearch";
			var tempLongPathFilename = longPathDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Directory.EnumerateDirectories( longPathDirectory, "T*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetCreationTime() {
			var tempLongPathFilename = longPathRoot.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename.Exists() );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );
				Directory.SetCreationTime( tempLongPathFilename, dateTime );
				var di = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( di.CreationTime, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		/// <remarks>
		///     TODO: test some error conditions.
		/// </remarks>
		[Test]
		public void TestSetCreationTimeUtc() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );
				Directory.SetCreationTimeUtc( tempLongPathFilename, dateTime );
				var di = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( di.CreationTimeUtc, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetCreationTimeUtcNonExistentDir() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			Assert.Throws<FileNotFoundException>( () => Directory.SetCreationTimeUtc( tempLongPathFilename, dateTime ) );
		}

		[Test]
		public void TestSetCurrentDirectory() {
			var originalDir = Directory.GetCurrentDirectory();

			try {
				Assert.Throws<NotSupportedException>( () => Directory.SetCurrentDirectory( longPathDirectory ) );
			}
			finally {
				Assert.Throws<NotSupportedException>( () => Directory.SetCurrentDirectory( originalDir ) );
			}
		}

		[Test]
		public void TestSetLastAccessTime() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.Now.AddDays( 1 );
				Directory.SetLastAccessTime( tempLongPathFilename, dateTime );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastAccessTime, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetLastAccessTimeUtc() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );
				Directory.SetLastAccessTimeUtc( tempLongPathFilename, dateTime );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastAccessTimeUtc, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetLastAccessTimeUtcNonExistentDir() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			Assert.Throws<FileNotFoundException>( () => Directory.SetLastAccessTimeUtc( tempLongPathFilename, dateTime ) );
		}

		[Test]
		public void TestSetLastWriteTime() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.Now.AddDays( 1 );
				Directory.SetLastWriteTime( tempLongPathFilename, dateTime );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastWriteTime, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		/// <remarks>
		///     TODO: test some error conditions.
		/// </remarks>
		[Test]
		public void TestSetLastWriteTimeUtc() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );
				Directory.SetLastWriteTimeUtc( tempLongPathFilename, dateTime );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastWriteTimeUtc, dateTime );
			}
			finally {
				Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetLastWriteTimeUtcNonExistentDir() {
			var tempLongPathFilename = longPathDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			Assert.Throws<FileNotFoundException>( () => Directory.SetLastWriteTimeUtc( tempLongPathFilename, dateTime ) );
		}
	}
}