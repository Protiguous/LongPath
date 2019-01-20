// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "FileInfoTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "FileInfoTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:13 PM.

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
	using File = Pri.LongPath.File;
	using FileInfo = Pri.LongPath.FileInfo;
	using Path = Pri.LongPath.Path;

	[TestFixture]
	public class FileInfoTests {

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
		public void CanCreateFileInfoWithLongPathFile() {
			String tempLongPathFilename;

			do {
				tempLongPathFilename = longPathDirectory.Combine( Path.GetRandomFileName() );
			} while ( File.Exists( tempLongPathFilename ) );

			Assert.IsFalse( File.Exists( tempLongPathFilename ) );

			using ( var writer = File.CreateText( tempLongPathFilename ) ) {
				writer.WriteLine( "test" );
			}

			try {
				Assert.IsTrue( File.Exists( tempLongPathFilename ) );
				var fileInfo = new FileInfo( tempLongPathFilename );
				Assert.IsNotNull( fileInfo ); // just to use fileInfo variable
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void FileInfoReturnsCorrectDirectoryNameForLongPathFile() {
			Assert.IsTrue( longPathDirectory.Exists() );
			String tempLongPathFilename;

			do {
				tempLongPathFilename = longPathDirectory.Combine( Path.GetRandomFileName() );
			} while ( File.Exists( tempLongPathFilename ) );

			Assert.IsFalse( File.Exists( tempLongPathFilename ) );

			using ( var writer = File.CreateText( tempLongPathFilename ) ) {
				writer.WriteLine( "test" );
			}

			try {
				Assert.IsTrue( File.Exists( tempLongPathFilename ) );
				var fileInfo = new FileInfo( tempLongPathFilename );
				Assert.AreEqual( longPathDirectory, fileInfo.DirectoryName );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestAppendText() {
			var filename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file16.ext" ).ToString();

			using ( var writer = File.CreateText( filename ) ) {
				writer.Write( "start" );
			}

			Assert.IsTrue( File.Exists( filename ) );

			try {
				using ( var writer = new FileInfo( filename ).AppendText() ) {
					writer.WriteLine( "end" );
				}

				using ( var reader = File.OpenText( filename ) ) {
					var text = reader.ReadLine();
					Assert.AreEqual( "startend", text );
				}
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestConstructorWithNullPath() => Assert.Throws<ArgumentNullException>( () => new FileInfo( null ) );

		[Test]
		public void TestCopyToWithoutOverwrite() {
			var fi = new FileInfo( longPathFilename );
			var destLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename (Copy).ext" ).ToString();

			fi.CopyTo( destLongPathFilename );

			try {
				Assert.IsTrue( File.Exists( destLongPathFilename ) );

				Assert.AreEqual( File.ReadAllText( longPathFilename ), File.ReadAllText( destLongPathFilename ) );
			}
			finally {
				File.Delete( destLongPathFilename );
			}
		}

		[Test]
		public void TestCopyToWithoutOverwriteAndExistingFile() {
			var fi = new FileInfo( longPathFilename );
			var destLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename (Copy).ext" ).ToString();

			fi.CopyTo( destLongPathFilename );

			try {
				Assert.IsTrue( File.Exists( destLongPathFilename ) );
				Assert.Throws<IOException>( () => fi.CopyTo( destLongPathFilename ) );
			}
			finally {
				File.Delete( destLongPathFilename );
			}
		}

		[Test]
		public void TestCopyToWithOverwrite() {
			var fi = new FileInfo( longPathFilename );
			var destLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename (Copy).ext" ).ToString();

			fi.CopyTo( destLongPathFilename );

			try {
				Assert.IsTrue( File.Exists( destLongPathFilename ) );
				fi.CopyTo( destLongPathFilename, true );
				Assert.AreEqual( File.ReadAllText( longPathFilename ), File.ReadAllText( destLongPathFilename ) );
			}
			finally {
				File.Delete( destLongPathFilename );
			}
		}

		[Test]
		public void TestCreate() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file19.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );
			Assert.IsFalse( fi.Exists );

			using ( fi.Create() ) { }

			try {
				Assert.IsTrue( File.Exists( fi.FullName ) ); // don't use FileInfo.Exists, it caches existance
			}
			finally {
				fi.Delete();
			}
		}

		[Test]
		public void TestCreateText() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file20.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );
			Assert.IsFalse( fi.Exists );

			using ( fi.CreateText() ) { }

			try {
				Assert.IsTrue( File.Exists( fi.FullName ) ); // don't use FileInfo.Exists, it caches existance
			}
			finally {
				fi.Delete();
			}
		}

		[Test]
		public void TestCreateTextAndWrite() {
			Assert.IsTrue( longPathDirectory.Exists() );
			String tempLongPathFilename;

			do {
				tempLongPathFilename = longPathDirectory.Combine( Path.GetRandomFileName() );
			} while ( File.Exists( tempLongPathFilename ) );

			Assert.IsFalse( File.Exists( tempLongPathFilename ) );

			const String fileText = "test";

			using ( var writer = File.CreateText( tempLongPathFilename ) ) {
				writer.WriteLine( fileText );
			}

			try {
				Assert.IsTrue( File.Exists( tempLongPathFilename ) );
				var fileInfo = new FileInfo( tempLongPathFilename );
				Assert.AreEqual( fileText.Length + Environment.NewLine.Length, fileInfo.Length );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestDecrypt() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			try {
				using ( var s = File.Create( tempLongPathFilename, 200 ) ) { }

				var preAttrib = File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( ( FileAttributes ) 0, preAttrib & FileAttributes.Encrypted );

				var fi = new FileInfo( tempLongPathFilename );
				fi.Encrypt();

				var postAttrib = File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( FileAttributes.Encrypted, postAttrib & FileAttributes.Encrypted );

				fi.Decrypt();

				postAttrib = File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( ( FileAttributes ) 0, postAttrib & FileAttributes.Encrypted );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestDisplayPath() {
			var sfi = new System.IO.FileInfo( @"c:\Windows\notepad.exe" );
			var fi = new FileInfo( @"c:\Windows\notepad.exe" );

			Assert.AreEqual( sfi.ToString(), fi.DisplayPath );
		}

		[Test]
		public void TestExists() => Assert.IsTrue( new FileInfo( longPathFilename ).Exists );

		[Test]
		public void TestExistsNonExistent() => Assert.IsFalse( new FileInfo( "giberish" ).Exists );

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControl() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var fi = new FileInfo( filename );
				var security = fi.GetAccessControl();
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
				File.Delete( filename );
			}
		}

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControlSections() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var fi = new FileInfo( filename );
				var security = fi.GetAccessControl( AccessControlSections.Access );
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
				File.Delete( filename );
			}
		}

		[Test]
		public void TestGetIsReadOnly() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var fi = new FileInfo( filename );
				Assert.IsTrue( fi.Exists );
				Assert.IsFalse( fi.IsReadOnly );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestLastWriteTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				{
					var fiTemp = new FileInfo( filename ) {
						LastWriteTime = dateTime
					};
				}

				var fi = new FileInfo( filename );
				Assert.AreEqual( dateTime, fi.LastWriteTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestLengthWithBadPath() {
			var filename = Util.CreateNewFile( longPathDirectory );
			FileInfo fi = null;

			try {
				Assert.Throws<FileNotFoundException>( () => fi = new FileInfo( filename ) );
			}
			catch {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestMoveTo() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file21.ext" ).ToString();
			var tempDestLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file21-1.ext" ).ToString();
			Assert.IsFalse( File.Exists( tempLongPathFilename ) );
			File.Copy( longPathFilename, tempLongPathFilename );

			try {
				Assert.IsTrue( File.Exists( tempLongPathFilename ) );

				var fi = new FileInfo( tempLongPathFilename );
				fi.MoveTo( tempDestLongPathFilename );

				try {
					Assert.IsFalse( File.Exists( tempLongPathFilename ) );
					Assert.IsTrue( File.Exists( tempDestLongPathFilename ) );
				}
				finally {
					File.Delete( tempDestLongPathFilename );
				}
			}
			finally {
				if ( File.Exists( tempLongPathFilename ) ) {
					File.Delete( tempLongPathFilename );
				}
			}
		}

		[Test]
		public void TestOpenCreateNew() {
			var fi = new FileInfo( longPathFilename );

			Assert.Throws<IOException>( () => {
				using ( var fileStream = fi.Open( FileMode.CreateNew ) ) {
					Assert.IsNotNull( fileStream );
				}
			} );
		}

		[Test]
		public void TestOpenCreatesEmpty() {
			var tempLongPathFilename = longPathDirectory.Combine( Path.GetRandomFileName() );

			try {
				using ( var writer = File.CreateText( tempLongPathFilename ) ) {
					writer.WriteLine( "test" );
				}

				var fi = new FileInfo( tempLongPathFilename );

				using ( var fileStream = fi.Open( FileMode.Append, FileAccess.Read, FileShare.Read ) ) {
					Assert.AreEqual( -1, fileStream.ReadByte() );
				}
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestOpenHidden() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file25.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );

			using ( fi.Create() ) { }

			try {
				Assert.Throws<UnauthorizedAccessException>( () => {
					File.SetAttributes( fi.FullName, File.GetAttributes( fi.FullName ) | FileAttributes.Hidden );

					using ( var fileStream = fi.Open( FileMode.Create ) ) {
						Assert.IsNotNull( fileStream );
					}
				} );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestOpenOpen() {
			var fi = new FileInfo( longPathFilename );

			using ( var fileStream = fi.Open( FileMode.Open ) ) {
				Assert.IsNotNull( fileStream );
			}
		}

		[Test]
		public void TestOpenReadReadsExistingData() {
			var fi = new FileInfo( longPathFilename );

			using ( var fileStream = fi.OpenRead() ) {
				Assert.AreEqual( 't', fileStream.ReadByte() );
			}
		}

		[Test]
		public void TestOpenReadWithWrite() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file31.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );

			try {
				Assert.Throws<NotSupportedException>( () => {
					using ( var fileStream = fi.Open( FileMode.Append, FileAccess.Read ) ) {
						fileStream.WriteByte( 43 );
					}
				} );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestOpenTextReadsExistingData() {
			var fi = new FileInfo( longPathFilename );

			using ( var streamReader = fi.OpenText() ) {
				Assert.AreEqual( "test", streamReader.ReadLine() );
			}
		}

		[Test]
		public void TestOpenWriteWritesCorrectly() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file31a.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );

			try {
				using ( var fileStream = fi.OpenWrite() ) {
					fileStream.WriteByte( 42 );
				}

				using ( var fileStream = fi.OpenRead() ) {
					Assert.AreEqual( 42, fileStream.ReadByte() );
				}
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestReplace() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename ) ) {
				try {
					fileStream.WriteByte( 42 );
				}
				catch ( Exception ) {
					File.Delete( tempLongPathFilename );

					throw;
				}
			}

			var tempLongPathFilename2 = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename2.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename2 ) ) {
				try {
					fileStream.WriteByte( 52 );
				}
				catch ( Exception ) {
					File.Delete( tempLongPathFilename2 );

					throw;
				}
			}

			var fi = new FileInfo( tempLongPathFilename );

			try {
				var fi2 = fi.Replace( tempLongPathFilename2, null );
				Assert.IsNotNull( fi2 );
				Assert.AreEqual( tempLongPathFilename2, fi2.FullName );

				using ( var fileStream = File.OpenRead( tempLongPathFilename2 ) ) {
					Assert.AreEqual( 42, fileStream.ReadByte() );
				}

				Assert.IsFalse( File.Exists( tempLongPathFilename ) );
			}
			finally {
				if ( File.Exists( tempLongPathFilename ) ) {
					File.Delete( tempLongPathFilename );
				}

				File.Delete( tempLongPathFilename2 );
			}
		}

		/// <remarks>
		///     TODO: create a scenario where ignoreMetadataErrors actually makes a difference
		/// </remarks>
		[Test]
		public void TestReplaceIgnoreMerge() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename ) ) {
				try {
					fileStream.WriteByte( 42 );
				}
				catch ( Exception ) {
					File.Delete( tempLongPathFilename );

					throw;
				}
			}

			var tempLongPathFilename2 = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename2.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename2 ) ) {
				try {
					fileStream.WriteByte( 52 );
				}
				catch ( Exception ) {
					File.Delete( tempLongPathFilename2 );

					throw;
				}
			}

			var fi = new FileInfo( tempLongPathFilename );

			try {
				const Boolean ignoreMetadataErrors = true;
				var fi2 = fi.Replace( tempLongPathFilename2, null, ignoreMetadataErrors );
				Assert.IsNotNull( fi2 );
				Assert.AreEqual( tempLongPathFilename2, fi2.FullName );

				using ( var fileStream = File.OpenRead( tempLongPathFilename2 ) ) {
					Assert.AreEqual( 42, fileStream.ReadByte() );
				}

				Assert.IsFalse( File.Exists( tempLongPathFilename ) );
			}
			finally {
				if ( File.Exists( tempLongPathFilename ) ) {
					File.Delete( tempLongPathFilename );
				}

				File.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestSetAccessControl() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var fi = new FileInfo( filename );
				var security = new FileSecurity();
				fi.SetAccessControl( security );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetCreationTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				var fi = new FileInfo( filename ) {
					CreationTime = dateTime
				};

				Assert.AreEqual( dateTime, filename.GetCreationTime() );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetCreationTimeMissingFile() {
			var filename = longPathDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.Now.AddDays( 1 );
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.CreationTime = dateTime );
		}

		[Test]
		public void TestSetCreationTimeUtc() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );

				var fi = new FileInfo( filename ) {
					CreationTimeUtc = dateTime
				};

				Assert.AreEqual( dateTime, filename.GetCreationTimeUtc() );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetCreationTimeUtcMissingFile() {
			var filename = longPathDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.CreationTimeUtc = dateTime );
		}

		[Test]
		public void TestSetIsReadOnly() {
			var filename = Util.CreateNewFile( longPathDirectory );
			var fi = new FileInfo( filename );

			try {
				fi.IsReadOnly = true;
				Assert.IsTrue( fi.IsReadOnly );
			}
			finally {
				fi.IsReadOnly = false;
				fi.Delete();
			}
		}

		[Test]
		public void TestSetLastAccessTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				var fi = new FileInfo( filename ) {
					LastAccessTime = dateTime
				};

				Assert.AreEqual( dateTime, File.GetLastAccessTime( filename ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastAccessTimeMissingFile() {
			var filename = longPathDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.Now.AddDays( 1 );
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.LastAccessTime = dateTime );
		}

		[Test]
		public void TestSetLastAccessTimeUtc() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );

				var fi = new FileInfo( filename ) {
					LastAccessTimeUtc = dateTime
				};

				Assert.AreEqual( dateTime, File.GetLastAccessTimeUtc( filename ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastAccessTimeUtcMissingFile() {
			var filename = longPathDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.LastAccessTimeUtc = dateTime );
		}

		[Test]
		public void TestSetLastWriteTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				var fi = new FileInfo( filename ) {
					LastWriteTime = dateTime
				};

				Assert.AreEqual( dateTime, File.GetLastWriteTime( filename ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastWriteTimeMissingFile() {
			var filename = longPathDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.Now.AddDays( 1 );
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.LastWriteTime = dateTime );
		}

		[Test]
		public void TestSetLastWriteTimeUtc() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );

				var fi = new FileInfo( filename ) {
					LastWriteTimeUtc = dateTime
				};

				Assert.AreEqual( dateTime, File.GetLastWriteTimeUtc( filename ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastWriteTimeUtcMissingFile() {
			var filename = longPathDirectory.Combine( "gibberish.ext" );
			var dateTime = DateTime.UtcNow.AddDays( 1 );
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.LastWriteTimeUtc = dateTime );
		}

		[Test]
		public void TestToString() {
			var fi = new FileInfo( longPathFilename );

			Assert.AreEqual( fi.DisplayPath, fi.ToString() );
		}
	}
}