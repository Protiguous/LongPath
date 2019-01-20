// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "UncFileInfoTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "UncFileInfoTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:16 PM.

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
	using File = System.IO.File;
	using FileInfo = Pri.LongPath.FileInfo;
	using Path = Pri.LongPath.Path;

	[TestFixture]
	public class UncFileInfoTests {

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
		public void CanCreateFileInfoWithLongPathFile() {
			String tempLongPathFilename;

			do {
				tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			} while ( Pri.LongPath.File.Exists( tempLongPathFilename ) );

			Assert.IsFalse( Pri.LongPath.File.Exists( tempLongPathFilename ) );

			using ( var writer = Pri.LongPath.File.CreateText( tempLongPathFilename ) ) {
				writer.WriteLine( "test" );
			}

			try {
				Assert.IsTrue( Pri.LongPath.File.Exists( tempLongPathFilename ) );
				var fileInfo = new FileInfo( tempLongPathFilename );
				Assert.IsNotNull( fileInfo ); // just to use fileInfo variable
			}
			finally {
				Pri.LongPath.File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void FileInfoReturnsCorrectDirectoryNameForLongPathFile() {
			Assert.IsTrue( uncDirectory.Exists() );
			String tempLongPathFilename;

			do {
				tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			} while ( Pri.LongPath.File.Exists( tempLongPathFilename ) );

			Assert.IsFalse( Pri.LongPath.File.Exists( tempLongPathFilename ) );

			using ( var writer = Pri.LongPath.File.CreateText( tempLongPathFilename ) ) {
				writer.WriteLine( "test" );
			}

			try {
				Assert.IsTrue( Pri.LongPath.File.Exists( tempLongPathFilename ) );
				var fileInfo = new FileInfo( tempLongPathFilename );
				Assert.AreEqual( uncDirectory, fileInfo.DirectoryName );
			}
			finally {
				Pri.LongPath.File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestAppendText() {
			var filename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "file16.ext" ).ToString();

			using ( var writer = Pri.LongPath.File.CreateText( filename ) ) {
				writer.Write( "start" );
			}

			Assert.IsTrue( Pri.LongPath.File.Exists( filename ) );

			try {
				using ( var writer = new FileInfo( filename ).AppendText() ) {
					writer.WriteLine( "end" );
				}

				using ( var reader = Pri.LongPath.File.OpenText( filename ) ) {
					var text = reader.ReadLine();
					Assert.AreEqual( "startend", text );
				}
			}
			finally {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestConstructorWithNullPath() => Assert.Throws<ArgumentNullException>( () => new FileInfo( null ) );

		[Test]
		public void TestCopyToWithoutOverwrite() {
			var fi = new FileInfo( filePath );
			var destLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "filename (Copy).ext" ).ToString();

			fi.CopyTo( destLongPathFilename );

			try {
				Assert.IsTrue( Pri.LongPath.File.Exists( destLongPathFilename ) );

				Assert.AreEqual( Pri.LongPath.File.ReadAllText( filePath ), Pri.LongPath.File.ReadAllText( destLongPathFilename ) );
			}
			finally {
				Pri.LongPath.File.Delete( destLongPathFilename );
			}
		}

		[Test]
		public void TestCopyToWithoutOverwriteAndExistingFile() {
			var fi = new FileInfo( filePath );
			var destLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "filename (Copy).ext" ).ToString();

			fi.CopyTo( destLongPathFilename );

			try {
				Assert.IsTrue( Pri.LongPath.File.Exists( destLongPathFilename ) );
				Assert.Throws<IOException>( () => fi.CopyTo( destLongPathFilename ) );
			}
			finally {
				Pri.LongPath.File.Delete( destLongPathFilename );
			}
		}

		[Test]
		public void TestCopyToWithOverwrite() {
			var fi = new FileInfo( filePath );
			var destLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "filename (Copy).ext" ).ToString();

			fi.CopyTo( destLongPathFilename );

			try {
				Assert.IsTrue( Pri.LongPath.File.Exists( destLongPathFilename ) );
				fi.CopyTo( destLongPathFilename, true );
				Assert.AreEqual( Pri.LongPath.File.ReadAllText( filePath ), Pri.LongPath.File.ReadAllText( destLongPathFilename ) );
			}
			finally {
				Pri.LongPath.File.Delete( destLongPathFilename );
			}
		}

		[Test]
		public void TestCreate() {
			var tempLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "file19.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );
			Assert.IsFalse( fi.Exists );

			using ( fi.Create() ) { }

			try {
				Assert.IsTrue( Pri.LongPath.File.Exists( fi.FullName ) ); // don't use FileInfo.Exists, it caches existance
			}
			finally {
				fi.Delete();
			}
		}

		[Test]
		public void TestCreateText() {
			var tempLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "file20.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );
			Assert.IsFalse( fi.Exists );

			using ( fi.CreateText() ) { }

			try {
				Assert.IsTrue( Pri.LongPath.File.Exists( fi.FullName ) ); // don't use FileInfo.Exists, it caches existance
			}
			finally {
				fi.Delete();
			}
		}

		[Test]
		public void TestCreateTextAndWrite() {
			Assert.IsTrue( uncDirectory.Exists() );
			String tempLongPathFilename;

			do {
				tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );
			} while ( Pri.LongPath.File.Exists( tempLongPathFilename ) );

			Assert.IsFalse( Pri.LongPath.File.Exists( tempLongPathFilename ) );

			const String fileText = "test";

			using ( var writer = Pri.LongPath.File.CreateText( tempLongPathFilename ) ) {
				writer.WriteLine( fileText );
			}

			try {
				Assert.IsTrue( Pri.LongPath.File.Exists( tempLongPathFilename ) );
				var fileInfo = new FileInfo( tempLongPathFilename );
				Assert.AreEqual( fileText.Length + Environment.NewLine.Length, fileInfo.Length );
			}
			finally {
				Pri.LongPath.File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestDecrypt() {
			var tempLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			try {
				using ( var s = Pri.LongPath.File.Create( tempLongPathFilename, 200 ) ) { }

				var preAttrib = Pri.LongPath.File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( ( FileAttributes ) 0, preAttrib & FileAttributes.Encrypted );

				var fi = new FileInfo( tempLongPathFilename );
				fi.Encrypt();

				var postAttrib = Pri.LongPath.File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( FileAttributes.Encrypted, postAttrib & FileAttributes.Encrypted );

				fi.Decrypt();

				postAttrib = Pri.LongPath.File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( ( FileAttributes ) 0, postAttrib & FileAttributes.Encrypted );
			}
			finally {
				Pri.LongPath.File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestDisplayPath() {
			var sfi = new System.IO.FileInfo( @"c:\Windows\notepad.exe" );
			var fi = new FileInfo( @"c:\Windows\notepad.exe" );

			Assert.AreEqual( sfi.ToString(), fi.DisplayPath );
		}

		[Test]
		public void TestExists() => Assert.IsTrue( new FileInfo( filePath ).Exists );

		[Test]
		public void TestExistsNonExistent() => Assert.IsFalse( new FileInfo( "giberish" ).Exists );

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControl() {
			var filename = Util.CreateNewFile( uncDirectory );

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
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControlSections() {
			var filename = Util.CreateNewFile( uncDirectory );

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
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestGetIsReadOnly() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var fi = new FileInfo( filename );
				Assert.IsTrue( fi.Exists );
				Assert.IsFalse( fi.IsReadOnly );
			}
			finally {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestLastWriteTime() {
			var filename = Util.CreateNewFile( uncDirectory );

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
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestLengthWithBadPath() {
			var filename = Util.CreateNewFile( uncDirectory );
			FileInfo fi;

			try {
				Assert.Throws<FileNotFoundException>( () => fi = new FileInfo( filename ) );
			}
			catch {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestMoveTo() {
			var tempLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "file21.ext" ).ToString();
			var tempDestLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "file21-1.ext" ).ToString();
			Assert.IsFalse( Pri.LongPath.File.Exists( tempLongPathFilename ) );
			Pri.LongPath.File.Copy( filePath, tempLongPathFilename );

			try {
				Assert.IsTrue( Pri.LongPath.File.Exists( tempLongPathFilename ) );

				var fi = new FileInfo( tempLongPathFilename );
				fi.MoveTo( tempDestLongPathFilename );

				try {
					Assert.IsFalse( Pri.LongPath.File.Exists( tempLongPathFilename ) );
					Assert.IsTrue( Pri.LongPath.File.Exists( tempDestLongPathFilename ) );
				}
				finally {
					Pri.LongPath.File.Delete( tempDestLongPathFilename );
				}
			}
			finally {
				if ( Pri.LongPath.File.Exists( tempLongPathFilename ) ) {
					Pri.LongPath.File.Delete( tempLongPathFilename );
				}
			}
		}

		[Test]
		public void TestOpenCreateNew() {
			var fi = new FileInfo( filePath );

			Assert.Throws<IOException>( () => {
				using ( var fileStream = fi.Open( FileMode.CreateNew ) ) {
					Assert.IsNotNull( fileStream );
				}
			} );
		}

		[Test]
		public void TestOpenCreatesEmpty() {
			var tempLongPathFilename = uncDirectory.Combine( Path.GetRandomFileName() );

			try {
				using ( var writer = Pri.LongPath.File.CreateText( tempLongPathFilename ) ) {
					writer.WriteLine( "test" );
				}

				var fi = new FileInfo( tempLongPathFilename );

				using ( var fileStream = fi.Open( FileMode.Append, FileAccess.Read, FileShare.Read ) ) {
					Assert.AreEqual( -1, fileStream.ReadByte() );
				}
			}
			finally {
				Pri.LongPath.File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestOpenHidden() {
			var tempLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "file25.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );

			using ( fi.Create() ) { }

			try {
				Pri.LongPath.File.SetAttributes( fi.FullName, Pri.LongPath.File.GetAttributes( fi.FullName ) | FileAttributes.Hidden );

				Assert.Throws<UnauthorizedAccessException>( () => {
					using ( var fileStream = fi.Open( FileMode.Create ) ) {
						Assert.IsNotNull( fileStream );
					}
				} );
			}
			finally {
				Pri.LongPath.File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestOpenOpen() {
			var fi = new FileInfo( filePath );

			using ( var fileStream = fi.Open( FileMode.Open ) ) {
				Assert.IsNotNull( fileStream );
			}
		}

		[Test]
		public void TestOpenReadReadsExistingData() {
			var fi = new FileInfo( filePath );

			using ( var fileStream = fi.OpenRead() ) {
				Assert.AreEqual( 't', fileStream.ReadByte() );
			}
		}

		[Test]
		public void TestOpenReadWithWrite() {
			var tempLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "file31.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );

			try {
				Assert.Throws<NotSupportedException>( () => {
					using ( var fileStream = fi.Open( FileMode.Append, FileAccess.Read ) ) {
						fileStream.WriteByte( 43 );
					}
				} );
			}
			finally {
				Pri.LongPath.File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestOpenTextReadsExistingData() {
			var fi = new FileInfo( filePath );

			using ( var streamReader = fi.OpenText() ) {
				Assert.AreEqual( "test", streamReader.ReadLine() );
			}
		}

		[Test]
		public void TestOpenWriteWritesCorrectly() {
			var tempLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "file31a.ext" ).ToString();
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
				Pri.LongPath.File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestReplace() {
			var tempLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var fileStream = Pri.LongPath.File.Create( tempLongPathFilename ) ) {
				try {
					fileStream.WriteByte( 42 );
				}
				catch ( Exception ) {
					Pri.LongPath.File.Delete( tempLongPathFilename );

					throw;
				}
			}

			var tempLongPathFilename2 = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "filename2.ext" ).ToString();

			using ( var fileStream = Pri.LongPath.File.Create( tempLongPathFilename2 ) ) {
				try {
					fileStream.WriteByte( 52 );
				}
				catch ( Exception ) {
					Pri.LongPath.File.Delete( tempLongPathFilename2 );

					throw;
				}
			}

			var fi = new FileInfo( tempLongPathFilename );

			try {
				var fi2 = fi.Replace( tempLongPathFilename2, null );
				Assert.IsNotNull( fi2 );
				Assert.AreEqual( tempLongPathFilename2, fi2.FullName );

				using ( var fileStream = Pri.LongPath.File.OpenRead( tempLongPathFilename2 ) ) {
					Assert.AreEqual( 42, fileStream.ReadByte() );
				}

				Assert.IsFalse( Pri.LongPath.File.Exists( tempLongPathFilename ) );
			}
			finally {
				if ( Pri.LongPath.File.Exists( tempLongPathFilename ) ) {
					Pri.LongPath.File.Delete( tempLongPathFilename );
				}

				Pri.LongPath.File.Delete( tempLongPathFilename2 );
			}
		}

		/// <remarks>
		///     TODO: create a scenario where ignoreMetadataErrors actually makes a difference
		/// </remarks>
		[Test]
		public void TestReplaceIgnoreMerge() {
			var tempLongPathFilename = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var fileStream = Pri.LongPath.File.Create( tempLongPathFilename ) ) {
				try {
					fileStream.WriteByte( 42 );
				}
				catch ( Exception ) {
					Pri.LongPath.File.Delete( tempLongPathFilename );

					throw;
				}
			}

			var tempLongPathFilename2 = new StringBuilder( uncDirectory ).Append( @"\" ).Append( "filename2.ext" ).ToString();

			using ( var fileStream = Pri.LongPath.File.Create( tempLongPathFilename2 ) ) {
				try {
					fileStream.WriteByte( 52 );
				}
				catch ( Exception ) {
					Pri.LongPath.File.Delete( tempLongPathFilename2 );

					throw;
				}
			}

			var fi = new FileInfo( tempLongPathFilename );

			try {
				const Boolean ignoreMetadataErrors = true;
				var fi2 = fi.Replace( tempLongPathFilename2, null, ignoreMetadataErrors );
				Assert.IsNotNull( fi2 );
				Assert.AreEqual( tempLongPathFilename2, fi2.FullName );

				using ( var fileStream = Pri.LongPath.File.OpenRead( tempLongPathFilename2 ) ) {
					Assert.AreEqual( 42, fileStream.ReadByte() );
				}

				Assert.IsFalse( Pri.LongPath.File.Exists( tempLongPathFilename ) );
			}
			finally {
				if ( Pri.LongPath.File.Exists( tempLongPathFilename ) ) {
					Pri.LongPath.File.Delete( tempLongPathFilename );
				}

				Pri.LongPath.File.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestSetAccessControl() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var fi = new FileInfo( filename );
				var security = new FileSecurity();
				fi.SetAccessControl( security );
			}
			finally {
				Pri.LongPath.File.Delete( filename );
			}
		}

		[Test]
		public void TestSetCreationTime() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				var fi = new FileInfo( filename ) {
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
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.CreationTime = dateTime );
		}

		[Test]
		public void TestSetCreationTimeUtc() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );

				var fi = new FileInfo( filename ) {
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
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.CreationTimeUtc = dateTime );
		}

		[Test]
		public void TestSetIsReadOnly() {
			var filename = Util.CreateNewFile( uncDirectory );
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
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				var fi = new FileInfo( filename ) {
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
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.LastAccessTime = dateTime );
		}

		[Test]
		public void TestSetLastAccessTimeUtc() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );

				var fi = new FileInfo( filename ) {
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
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.LastAccessTimeUtc = dateTime );
		}

		[Test]
		public void TestSetLastWriteTime() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );

				var fi = new FileInfo( filename ) {
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
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.LastWriteTime = dateTime );
		}

		[Test]
		public void TestSetLastWriteTimeUtc() {
			var filename = Util.CreateNewFile( uncDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );

				var fi = new FileInfo( filename ) {
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
			var fi = new FileInfo( filename );
			Assert.Throws<FileNotFoundException>( () => fi.LastWriteTimeUtc = dateTime );
		}

		[Test]
		public void TestToString() {
			var fi = new FileInfo( filePath );

			Assert.AreEqual( fi.DisplayPath, fi.ToString() );
		}
	}
}