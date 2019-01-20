// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "UncDirectoryTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "UncDirectoryTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:15 PM.

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
	using Path = System.IO.Path;

	[TestFixture]
	public class UncDirectoryTests {

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
				if ( File.Exists( filePath ) ) {
					File.Delete( filePath );
				}
			}
			catch ( Exception e ) {
				Trace.WriteLine( "Exception {0} deleting \"filePath\"", e.ToString() );

				throw;
			}
			finally {
				if ( Directory.Exists( directory ) ) {
					Directory.Delete( directory, true );
				}
			}
		}

		private static String uncDirectory;

		private static String uncFilePath;

		private static String directory;

		private static String filePath;

		private const String Filename = "filename.ext";

		[Test]
		public void BaselineDirectoryExists() => Assert.IsTrue( Directory.Exists( UncHelper.GetUncFromPath( "." ) ) );

		[Test]
		public void BaselineGetParent() {
			var actual = Directory.GetParent( Path.Combine( uncDirectory, "system32" ) );
			Assert.AreEqual( uncDirectory, actual.FullName );
		}

		[Test]
		public void BaselineTestCreateDirectory() {
			var tempPath = Path.Combine( uncDirectory, Pri.LongPath.Path.GetRandomFileName() );
			var di = Directory.CreateDirectory( tempPath );

			try {
				Assert.IsNotNull( di );
				Assert.IsTrue( Directory.Exists( tempPath ) );
			}
			finally {
				Directory.Delete( tempPath );
			}
		}

		[Test]
		public void BaselineTestCreateMultipleDirectories() {
			var tempSubDir = Path.Combine( uncDirectory, Pri.LongPath.Path.GetRandomFileName() );
			var tempSubSubDir = Path.Combine( tempSubDir, Pri.LongPath.Path.GetRandomFileName() );
			var di = Directory.CreateDirectory( tempSubSubDir );

			try {
				Assert.IsNotNull( di );
				Assert.IsTrue( Directory.Exists( tempSubSubDir ) );
			}
			finally {
				Directory.Delete( tempSubDir, true );
			}
		}

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
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			var di = tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsNotNull( di );
				Assert.IsTrue( tempLongPathFilename.Exists() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestCreateDirectoryThatAlreadyExists() {
			var di = uncDirectory.CreateDirectory();
			Assert.IsNotNull( di );
			Assert.IsTrue( uncDirectory.Exists() );
		}

		[Test]
		public void TestCreateDirectoryThatEndsWithSlash() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() + @"\" );
			var di = tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsNotNull( di );
				Assert.IsTrue( tempLongPathFilename.Exists() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		/// <remarks>
		///     TODO: more realistic DirectorySecurity scenarios
		/// </remarks>
		[Test]
		public void TestCreateWithFileSecurity() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );

			try {
				Pri.LongPath.Directory.CreateDirectory( tempLongPathFilename, new DirectorySecurity() );
				Assert.IsTrue( tempLongPathFilename.Exists() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestCurrentDirectory() {
			var di = new DirectoryInfo( "." );
			Assert.AreEqual( di.FullName, Pri.LongPath.Directory.GetCurrentDirectory() );
		}

		[Test]
		public void TestDeleteDirectory() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename.GetFullPath().Exists() );
			Pri.LongPath.Directory.Delete( tempLongPathFilename );
			Assert.IsFalse( tempLongPathFilename.GetFullPath().Exists() );
		}

		[Test]
		public void TestDirectoryCreateNearMaxPathLimit() {
			var uncPathNearMaxPathLimit = uncDirectory.Combine( new String( 'x', Pri.LongPath.NativeMethods.MAX_PATH - uncDirectory.Length - 2 ) );
			uncPathNearMaxPathLimit.CreateDirectory();
			Assert.That( uncPathNearMaxPathLimit.Exists() );
			Pri.LongPath.Directory.Delete( uncPathNearMaxPathLimit );
		}

		[Test]
		public void TestDirectoryEnumerateDirectoriesNearMaxPathLimit() {
			var uncPathNearMaxPathLimit = uncDirectory.Combine( new String( 'x', Pri.LongPath.NativeMethods.MAX_PATH - uncDirectory.Length - 2 ) );
			uncPathNearMaxPathLimit.Replace( uncDirectory, directory ).CreateDirectory();

			var uncPathAboveMaxPathLimit = uncPathNearMaxPathLimit.Combine( "wibble" );
			uncPathAboveMaxPathLimit.CreateDirectory();

			Assert.That( uncPathNearMaxPathLimit.Exists() );
			Assert.That( uncPathAboveMaxPathLimit.Exists() );

			// there should be one subdirectory inside almostLongPath
			var subDirs = Pri.LongPath.Directory.EnumerateDirectories( uncPathNearMaxPathLimit ).ToArray();

			Pri.LongPath.Directory.Delete( uncPathAboveMaxPathLimit );
			Pri.LongPath.Directory.Delete( uncPathNearMaxPathLimit );

			Assert.That( subDirs.Length, Is.EqualTo( 1 ) );
		}

		/// <remarks>
		///     Tests <see cref="Pri.LongPath.Directory.EnumerateDirectories(String)" />, depends on
		///     <see cref="Pri.LongPath.Directory.CreateDirectory(string)" />
		/// </remarks>
		[Test]
		public void TestEnumerateDirectories() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempPath = uncDirectory.Combine( randomFileName );
			tempPath.CreateDirectory();

			try {
				var dirs = Pri.LongPath.Directory.EnumerateDirectories( uncDirectory ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempPath ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempPath );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearch() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Pri.LongPath.Directory.EnumerateDirectories( uncDirectory, "*" ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEnumerateDirectoriesWithSearchWithNoResults() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Pri.LongPath.Directory.EnumerateDirectories( uncDirectory, "gibberish" ).ToArray();
				Assert.AreEqual( 0, dirs.Length );
				Assert.IsFalse( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		/// <remarks>
		///     Tests <see cref="Pri.LongPath.Directory.EnumerateDirectories(String)" />, depends on
		///     <see cref="Pri.LongPath.Directory.CreateDirectory(string)" />
		/// </remarks>
		[Test]
		public void TestEnumerateFiles() {
			var files = Pri.LongPath.Directory.EnumerateFiles( uncDirectory ).ToArray();
			Assert.AreEqual( 1, files.Length );
			Assert.IsTrue( files.Contains( uncFilePath ) );
		}

		[Test]
		public void TestEnumerateFilesRecursiveWithSearchWithNoResults() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Pri.LongPath.Directory.EnumerateFiles( uncDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, files.Length );
				Assert.IsFalse( files.Contains( uncFilePath ) );
				Assert.IsFalse( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestEnumerateFilesWithSearch() {
			var files = Pri.LongPath.Directory.EnumerateFiles( uncDirectory, "*" ).ToArray();
			Assert.AreEqual( 1, files.Length );
			Assert.IsTrue( files.Contains( uncFilePath ) );
		}

		[Test]
		public void TestEnumerateFilesWithSearchWithNoResults() {
			var files = Pri.LongPath.Directory.EnumerateFiles( uncDirectory, "giberish" ).ToArray();
			Assert.AreEqual( 0, files.Length );
			Assert.IsFalse( files.Contains( uncFilePath ) );
		}

		[Test]
		public void TestEnumerateFileSystemEntries() {
			var entries = Pri.LongPath.Directory.EnumerateFileSystemEntries( uncDirectory ).ToArray();
			Assert.AreEqual( 1, entries.Length );
			Assert.IsTrue( entries.Contains( uncFilePath ) );
		}

		[Test]
		public void TestEnumerateFileSystemEntriesRecursiveWithSearchWithNoResults() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Pri.LongPath.Directory.EnumerateFileSystemEntries( uncDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, files.Length );
				Assert.IsFalse( files.Contains( uncFilePath ) );
				Assert.IsFalse( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestEnumerateFileSystemEntriesWithSearch() {
			var entries = Pri.LongPath.Directory.EnumerateFileSystemEntries( uncDirectory, "*" ).ToArray();
			Assert.AreEqual( 1, entries.Length );
			Assert.IsTrue( entries.Contains( uncFilePath ) );
		}

		[Test]
		public void TestEnumerateFileSystemEntriesWithSearchWithNoResults() {
			var entries = Pri.LongPath.Directory.EnumerateFileSystemEntries( uncDirectory, "giberish" ).ToArray();
			Assert.AreEqual( 0, entries.Length );
			Assert.IsFalse( entries.Contains( uncFilePath ) );
		}

		[Test]
		public void TestEnumerateRecursiveFilesWithSearch() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Pri.LongPath.Directory.EnumerateFiles( uncDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 2, files.Length );
				Assert.IsTrue( files.Contains( uncFilePath ) );
				Assert.IsTrue( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestEnumerateRecursiveFileSystemEntriesWithSearch() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var entries = Pri.LongPath.Directory.EnumerateFileSystemEntries( uncDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 3, entries.Length );
				Assert.IsTrue( entries.Contains( uncFilePath ) );
				Assert.IsTrue( entries.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestExists() => Assert.IsTrue( uncDirectory.Exists() );

		[Test]
		public void TestExistsOnFile() => Assert.IsFalse( uncFilePath.Exists() );

		[Test]
		public void TestExistsOnNonexistentFile() => Assert.IsFalse( new StringBuilder( uncDirectory ).Append( @"\" ).Append( "does-not-exist" ).ToString().Exists() );

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControl() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var security = Pri.LongPath.Directory.GetAccessControl( tempLongPathFilename );
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
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var security = Pri.LongPath.Directory.GetAccessControl( tempLongPathFilename, AccessControlSections.Access );
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
		public void TestGetCreationTime() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Pri.LongPath.Directory.GetCreationTime( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.CreationTime, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		/// <remarks>
		///     TODO: test some error conditions.
		/// </remarks>
		[Test]
		public void TestGetCreationTimeUTc() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Pri.LongPath.Directory.GetCreationTimeUtc( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.CreationTimeUtc, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetDirectories() => Assert.AreEqual( 0, Pri.LongPath.Directory.GetDirectories( uncDirectory ).Count() );

		[Test]
		public void TestGetDirectoriesWithAnySearch() {
			var tempLongPathFilename = uncDirectory.Combine( "TestGetDirectoriesWithAnySearch" );
			tempLongPathFilename.CreateDirectory();
			var tempLongPathFilename2 = uncDirectory.Combine( "ATestGetDirectoriesWithAnySearch" );
			tempLongPathFilename2.CreateDirectory();

			try {
				Assert.AreEqual( 2, Pri.LongPath.Directory.GetDirectories( uncDirectory, "*" ).Count() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
				Pri.LongPath.Directory.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestGetDirectoriesWithSearchWithNoResults() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Pri.LongPath.Directory.GetDirectories( uncDirectory, "gibberish" ).ToArray();
				Assert.AreEqual( 0, dirs.Length );
				Assert.IsFalse( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetDirectoriesWithSubsetSearch() {
			var tempLongPathFilename = uncDirectory.Combine( "TestGetDirectoriesWithSubsetSearch" );
			tempLongPathFilename.CreateDirectory();
			var tempLongPathFilename2 = uncDirectory.Combine( "ATestGetDirectoriesWithSubsetSearch" );
			tempLongPathFilename2.CreateDirectory();

			try {
				Assert.AreEqual( 1, Pri.LongPath.Directory.GetDirectories( uncDirectory, "A*" ).Count() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
				Pri.LongPath.Directory.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestGetDirectoryRoot() =>
			Assert.IsTrue( @"\\localhost\C$\".Equals( Pri.LongPath.Directory.GetDirectoryRoot( uncDirectory ), StringComparison.InvariantCultureIgnoreCase ) );

		[Test]
		public void TestGetFiles() {
			Assert.AreNotEqual( 0, uncDirectory.GetFiles().Count() );
			Assert.AreEqual( 1, uncDirectory.GetFiles().Count() );
			Assert.IsTrue( uncDirectory.GetFiles().Contains( uncFilePath ) );
		}

		[Test]
		public void TestGetFilesRecursiveWithSearchWithNoResults() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Pri.LongPath.Directory.GetFiles( uncDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, files.Length );
				Assert.IsFalse( files.Contains( uncFilePath ) );
				Assert.IsFalse( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestGetFilesWithSearch() {
			var files = Pri.LongPath.Directory.GetFiles( uncDirectory, "*" ).ToArray();
			Assert.AreEqual( 1, files.Length );
			Assert.IsTrue( files.Contains( uncFilePath ) );
		}

		[Test]
		public void TestGetFilesWithSearchWithNoResults() {
			var files = Pri.LongPath.Directory.GetFiles( uncDirectory, "giberish" ).ToArray();
			Assert.AreEqual( 0, files.Length );
			Assert.IsFalse( files.Contains( uncFilePath ) );
		}

		[Test]
		public void TestGetFileSystemEntries() {
			var entries = Pri.LongPath.Directory.GetFileSystemEntries( uncDirectory ).ToArray();
			Assert.AreEqual( 1, entries.Length );
			Assert.IsTrue( entries.Contains( uncFilePath ) );
		}

		[Test]
		public void TestGetFileSystemEntriesRecursiveWithSearchWithNoResults() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Pri.LongPath.Directory.GetFileSystemEntries( uncDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, files.Length );
				Assert.IsFalse( files.Contains( uncFilePath ) );
				Assert.IsFalse( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestGetFileSystemEntriesWithSearch() {
			var entries = Pri.LongPath.Directory.GetFileSystemEntries( uncDirectory, "*" ).ToArray();
			Assert.AreEqual( 1, entries.Length );
			Assert.IsTrue( entries.Contains( uncFilePath ) );
		}

		[Test]
		public void TestGetFileSystemEntriesWithSearchWithNoResults() {
			var entries = Pri.LongPath.Directory.GetFileSystemEntries( uncDirectory, "giberish" ).ToArray();
			Assert.AreEqual( 0, entries.Length );
			Assert.IsFalse( entries.Contains( uncFilePath ) );
		}

		[Test]
		public void TestGetLastAccessTime() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Pri.LongPath.Directory.GetLastAccessTime( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastAccessTime, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetLastAccessTimeUtc() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Pri.LongPath.Directory.GetLastAccessTimeUtc( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastAccessTimeUtc, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetLastWriteTime() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Pri.LongPath.Directory.GetLastWriteTime( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastWriteTime, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetLastWriteTimeUtc() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = Pri.LongPath.Directory.GetLastWriteTimeUtc( tempLongPathFilename );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastWriteTimeUtc, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetLogicalDrives() {
			var directoryGetLogicalDrives = Pri.LongPath.Directory.GetLogicalDrives();
			Assert.IsNotNull( directoryGetLogicalDrives );
			Assert.IsTrue( directoryGetLogicalDrives.Any() );
		}

		/// <remarks>
		///     Tests Directory.GetParent,
		///     depends on Directory.Combine, DirectoryInfo.FullName
		/// </remarks>
		[Test]
		public void TestGetParent() {
			var actual = Pri.LongPath.Directory.GetParent( uncDirectory.Combine( "system32" ) );
			Assert.AreEqual( uncDirectory, actual.FullName );
		}

		[Test]
		public void TestGetRecursiveDirectoriesWithSearch() => Assert.AreEqual( 0, uncDirectory.GetDirectories( "*", SearchOption.AllDirectories ).Count() );

		[Test]
		public void TestGetRecursiveDirectoriesWithSubsetSearch() {
			var tempLongPathFilename = uncDirectory.Combine( "TestGetRecursiveDirectoriesWithSubsetSearch" );
			tempLongPathFilename.CreateDirectory();
			var tempLongPathFilename2 = tempLongPathFilename.Combine( "ATestGetRecursiveDirectoriesWithSubsetSearch" );
			tempLongPathFilename2.CreateDirectory();

			try {
				Assert.AreEqual( 1, uncDirectory.GetDirectories( "A*", SearchOption.AllDirectories ).Count() );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename2 );
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestGetRecursiveFilesWithAnySearch() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var files = Pri.LongPath.Directory.GetFiles( uncDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 2, files.Length );
				Assert.IsTrue( files.Contains( uncFilePath ) );
				Assert.IsTrue( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestGetRecursiveFilesWithSubsetSearch() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var searchPattern = randomFileName.GetFileName().Substring( 0, 3 ) + "*" + randomFileName.GetExtension();

				var files = Pri.LongPath.Directory.GetFiles( uncDirectory, searchPattern, SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, files.Length );
				Assert.IsFalse( files.Contains( uncFilePath ) );
				Assert.IsTrue( files.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
			}
		}

		[Test]
		public void TestGetRecursiveFileSystemEntriesWithSearch() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				Assert.IsTrue( tempLongPathFilename.Exists() );
				var randomFileName = Util.CreateNewEmptyFile( tempLongPathFilename );

				var entries = Pri.LongPath.Directory.GetFileSystemEntries( uncDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 3, entries.Length );
				Assert.IsTrue( entries.Contains( uncFilePath ) );
				Assert.IsTrue( entries.Contains( randomFileName ) );
			}
			finally {
				const Boolean recursive = true;
				Pri.LongPath.Directory.Delete( tempLongPathFilename, recursive );
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
			Assert.Throws<IOException>( () => {
				var tempLongPathFilename1 = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
				tempLongPathFilename1.CreateDirectory();
				Assert.IsTrue( tempLongPathFilename1.GetFullPath().Exists() );
				var tempLongPathFilename2 = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
				tempLongPathFilename2.CreateDirectory();
				Assert.IsTrue( tempLongPathFilename2.GetFullPath().Exists() );

				try {
					using ( var writer = Pri.LongPath.File.CreateText( tempLongPathFilename2.Combine( "TestInUseMove" ) ) ) {
						var destinationPath = tempLongPathFilename1.Combine( tempLongPathFilename2.GetFileName() ).GetFullPath();
						Pri.LongPath.Directory.Move( tempLongPathFilename2, destinationPath );
						Assert.IsTrue( tempLongPathFilename1.GetFullPath().Exists() );
						Assert.IsFalse( tempLongPathFilename2.GetFullPath().Exists() );
						Assert.IsTrue( destinationPath.Exists() );
					}
				}
				finally {
					Pri.LongPath.Directory.Delete( tempLongPathFilename1, recursive );
					Pri.LongPath.Directory.Delete( tempLongPathFilename2, recursive );
				}
			} );
		}

		[Test]
		public void TestMove() {
			var tempLongPathFilename1 = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename1.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename1.GetFullPath().Exists() );
			var tempLongPathFilename2 = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename2.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename2.GetFullPath().Exists() );

			var destinationPath = tempLongPathFilename1.Combine( tempLongPathFilename2.GetFileName() ).GetFullPath();
			Pri.LongPath.Directory.Move( tempLongPathFilename2, destinationPath );
			Assert.IsTrue( tempLongPathFilename1.GetFullPath().Exists() );
			Assert.IsFalse( tempLongPathFilename2.GetFullPath().Exists() );
			Assert.IsTrue( destinationPath.Exists() );

			const Boolean recursive = true;
			Pri.LongPath.Directory.Delete( tempLongPathFilename1, recursive );
			Pri.LongPath.Directory.Delete( tempLongPathFilename2, recursive );
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithAllSearch() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Pri.LongPath.Directory.EnumerateDirectories( uncDirectory, "*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch() {
			var randomFileName = "TestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch";
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();
			randomFileName = "ATestRecursiveEnumerateDirectoriesWithMultipleSubsetSearch";
			var tempLongPathFilename2 = uncDirectory.Combine( randomFileName );
			tempLongPathFilename2.CreateDirectory();

			try {
				var dirs = Pri.LongPath.Directory.EnumerateDirectories( uncDirectory, "T*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempLongPathFilename ) );
				Assert.IsFalse( dirs.Contains( tempLongPathFilename2 ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
				Pri.LongPath.Directory.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithSearchNoResults() {
			var randomFileName = Pri.LongPath.Path.GetRandomFileName();
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Pri.LongPath.Directory.EnumerateDirectories( uncDirectory, "gibberish", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 0, dirs.Length );
				Assert.IsFalse( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestRecursiveEnumerateDirectoriesWithSingleSubsetSearch() {
			const String randomFileName = "TestRecursiveEnumerateDirectoriesWithSubsetSearch";
			var tempLongPathFilename = uncDirectory.Combine( randomFileName );
			tempLongPathFilename.CreateDirectory();

			try {
				var dirs = Pri.LongPath.Directory.EnumerateDirectories( uncDirectory, "T*", SearchOption.AllDirectories ).ToArray();
				Assert.AreEqual( 1, dirs.Length );
				Assert.IsTrue( dirs.Contains( tempLongPathFilename ) );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetCreationTime() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();
			Assert.IsTrue( tempLongPathFilename.Exists() );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );
				Pri.LongPath.Directory.SetCreationTime( tempLongPathFilename, dateTime );
				var di = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( di.CreationTime, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		/// <remarks>
		///     TODO: test some error conditions.
		/// </remarks>
		[Test]
		public void TestSetCreationTimeUtc() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );
				Directory.SetCreationTimeUtc( tempLongPathFilename, dateTime );
				var di = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( di.CreationTimeUtc, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetCreationTimeUtcNonExistentDir() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			Assert.Throws<FileNotFoundException>( () => Directory.SetCreationTimeUtc( tempLongPathFilename, dateTime ) );
		}

		[Test]
		public void TestSetCurrentDirectory() {
			var originalDir = Pri.LongPath.Directory.GetCurrentDirectory();

			try {
				Assert.Throws<NotSupportedException>( () => Pri.LongPath.Directory.SetCurrentDirectory( uncDirectory ) );
			}
			finally {
				Assert.Throws<NotSupportedException>( () => Pri.LongPath.Directory.SetCurrentDirectory( originalDir ) );
			}
		}

		[Test]
		public void TestSetLastAccessTime() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.Now.AddDays( 1 );
				Pri.LongPath.Directory.SetLastAccessTime( tempLongPathFilename, dateTime );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastAccessTime, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetLastAccessTimeUtc() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );
				Pri.LongPath.Directory.SetLastAccessTimeUtc( tempLongPathFilename, dateTime );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastAccessTimeUtc, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetLastAccessTimeUtcNonExistentDir() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			Assert.Throws<FileNotFoundException>( () => Pri.LongPath.Directory.SetLastAccessTimeUtc( tempLongPathFilename, dateTime ) );
		}

		[Test]
		public void TestSetLastWriteTime() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.Now.AddDays( 1 );
				Pri.LongPath.Directory.SetLastWriteTime( tempLongPathFilename, dateTime );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastWriteTime, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		/// <remarks>
		///     TODO: test some error conditions.
		/// </remarks>
		[Test]
		public void TestSetLastWriteTimeUtc() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			tempLongPathFilename.CreateDirectory();

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );
				Pri.LongPath.Directory.SetLastWriteTimeUtc( tempLongPathFilename, dateTime );
				var fi = new DirectoryInfo( tempLongPathFilename );
				Assert.AreEqual( fi.LastWriteTimeUtc, dateTime );
			}
			finally {
				Pri.LongPath.Directory.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetLastWriteTimeUtcNonExistentDir() {
			var tempLongPathFilename = uncDirectory.Combine( Pri.LongPath.Path.GetRandomFileName() );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			Assert.Throws<FileNotFoundException>( () => Pri.LongPath.Directory.SetLastWriteTimeUtc( tempLongPathFilename, dateTime ) );
		}
	}
}