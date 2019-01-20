// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "FileTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "FileTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:14 PM.

namespace Tests {

	using System;
	using System.Collections.Generic;
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
	using FileInfo = Pri.LongPath.FileInfo;
	using Path = Pri.LongPath.Path;

	[TestFixture]
	public class FileTests {

		private static String rootTestDir;

		private static String longPathDirectory;

		private static String longPathFilename;

		private const String Filename = "filename.ext";

#if DEBUG1
		private static void references()
		{
			System.IO.File.Exists(".");
			System.IO.FileSystemInfo fs;
		}
#endif

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

		[Test]
		public void TestExists() => Assert.IsTrue( File.Exists( longPathFilename ) );

		[Test]
		public void TestCreateText() {
			var filename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file3.ext" ).ToString();
			const String fileText = "test";

			using ( var writer = File.CreateText( filename ) ) {
				writer.WriteLine( fileText );
			}

			try {
				Assert.IsTrue( File.Exists( filename ) );

				using ( var reader = File.OpenText( filename ) ) {
					var text = reader.ReadLine();
					Assert.AreEqual( fileText, text );
				}
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestWriteAllText() {
			var filename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file3.ext" ).ToString();
			const String fileText = "test";

			using ( File.CreateText( filename ) ) { }

			try {
				File.WriteAllText( filename, fileText );
				Assert.AreEqual( fileText, File.ReadAllText( filename ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestReadAllTextNewFile() {
			var filename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file3.ext" ).ToString();
			const String fileText = "test";

			using ( File.CreateText( filename ) ) { }

			try {
				File.WriteAllText( filename, fileText );
				Assert.AreEqual( fileText, File.ReadAllText( filename ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestReadAllTextNullPath() => Assert.Throws<ArgumentNullException>( () => File.ReadAllText( null ) );

		[Test]
		public void TestWriteAllTextNullPath() => Assert.Throws<ArgumentNullException>( () => File.WriteAllText( null, "test" ) );

		[Test]
		public void TestWriteAllTextEncoding() {
			var filename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file3.ext" ).ToString();
			const String fileText = "test";

			using ( File.CreateText( filename ) ) { }

			try {
				File.WriteAllText( filename, fileText, Encoding.Unicode );
				Assert.AreEqual( fileText, File.ReadAllText( filename, Encoding.Unicode ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestReadAllTextEncoding() {
			var filename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file3.ext" ).ToString();
			const String fileText = "test";

			using ( File.CreateText( filename ) ) { }

			try {
				File.WriteAllText( filename, fileText, Encoding.Unicode );
				Assert.AreEqual( fileText, File.ReadAllText( filename, Encoding.Unicode ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestDirectoryWithRoot() {
			var fi = new FileInfo( @"C:\" );
			Assert.IsNull( fi.Directory );
		}

		[Test]
		public void FileInfoReturnsCorrectDirectoryForLongPathFile() {
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
				Assert.AreEqual( longPathDirectory, fileInfo.Directory.FullName );
				Assert.AreEqual( longPathDirectory.GetFileName(), fileInfo.Directory.Name );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestReadAllText() => Assert.AreEqual( "test" + Environment.NewLine, File.ReadAllText( longPathFilename ) );

		[Test]
		public void TestCopy() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file22.ext" ).ToString();
			var tempDestLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file22-1.ext" ).ToString();
			Assert.IsFalse( File.Exists( tempLongPathFilename ) );
			File.Copy( longPathFilename, tempLongPathFilename );

			try {
				Assert.IsTrue( File.Exists( tempLongPathFilename ) );

				File.Move( tempLongPathFilename, tempDestLongPathFilename );

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
		public void TestCopyWithoutOverwrite() {
			var destLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename (Copy).ext" ).ToString();

			File.Copy( longPathFilename, destLongPathFilename );

			try {
				Assert.IsTrue( File.Exists( destLongPathFilename ) );

				Assert.AreEqual( File.ReadAllText( longPathFilename ), File.ReadAllText( destLongPathFilename ) );
			}
			finally {
				File.Delete( destLongPathFilename );
			}
		}

		[Test]
		public void TestCopyWithoutOverwriteAndExistingFile() {
			var destLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename (Copy).ext" ).ToString();

			File.Copy( longPathFilename, destLongPathFilename );

			try {
				Assert.IsTrue( File.Exists( destLongPathFilename ) );
				Assert.Throws<IOException>( () => File.Copy( longPathFilename, destLongPathFilename ) );
			}
			finally {
				File.Delete( destLongPathFilename );
			}
		}

		[Test]
		public void TestCopyWithOverwrite() {
			var destLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename (Copy).ext" ).ToString();

			File.Copy( longPathFilename, destLongPathFilename );

			try {
				Assert.IsTrue( File.Exists( destLongPathFilename ) );
				File.Copy( longPathFilename, destLongPathFilename, true );
				Assert.AreEqual( File.ReadAllText( longPathFilename ), File.ReadAllText( destLongPathFilename ) );
			}
			finally {
				File.Delete( destLongPathFilename );
			}
		}

		[Test]
		public void TestMove() {
			var sourceFilename = Util.CreateNewFile( longPathDirectory );
			var destFilename = longPathDirectory.Combine( Path.GetRandomFileName() );
			File.Move( sourceFilename, destFilename );

			try {
				Assert.IsFalse( File.Exists( sourceFilename ) );
				Assert.IsTrue( File.Exists( destFilename ) );
				Assert.IsTrue( Util.VerifyContentsOfNewFile( destFilename ) );
			}
			finally {
				if ( File.Exists( destFilename ) ) {
					File.Delete( destFilename );
				}
			}
		}

		[Test]
		public void TestReplace() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename ) ) {
				fileStream.WriteByte( 42 );
			}

			var tempLongPathFilename2 = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename2.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename2 ) ) {
				fileStream.WriteByte( 52 );
			}

			try {
				File.Replace( tempLongPathFilename, tempLongPathFilename2, null );

				using ( var fileStream = File.OpenRead( tempLongPathFilename2 ) ) {
					Assert.AreEqual( 42, fileStream.ReadByte() );
				}

				Assert.IsFalse( File.Exists( tempLongPathFilename ) );
			}
			finally {
				File.Delete( tempLongPathFilename2 );
			}
		}

		[Test]
		public void TestReplaceWithNulls() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				Assert.Throws<ArgumentNullException>( () => File.Replace( null, null, null ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestReplaceWithNullDestination() => Assert.Throws<ArgumentNullException>( () => File.Replace( longPathFilename, null, null ) );

        /// <remarks>
        ///     TODO: create a scenario where ignoreMetadataErrors actually makes a difference
        /// </remarks>
        [Test]
		public void TestReplaceIgnoreMerge() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename ) ) {
				fileStream.WriteByte( 42 );
			}

			var tempLongPathFilename2 = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename2.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename2 ) ) {
				fileStream.WriteByte( 52 );
			}

			try {
				const Boolean ignoreMetadataErrors = true;
				File.Replace( tempLongPathFilename, tempLongPathFilename2, null, ignoreMetadataErrors );

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
		public void TestReplaceIgnoreMergeWithBackup() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();
			var tempBackupLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "backup" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename ) ) {
				fileStream.WriteByte( 42 );
			}

			var tempLongPathFilename2 = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename2.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename2 ) ) {
				fileStream.WriteByte( 52 );
			}

			try {
				const Boolean ignoreMetadataErrors = true;
				File.Replace( tempLongPathFilename, tempLongPathFilename2, tempBackupLongPathFilename, ignoreMetadataErrors );

				using ( var fileStream = File.OpenRead( tempLongPathFilename2 ) ) {
					Assert.AreEqual( 42, fileStream.ReadByte() );
				}

				Assert.IsFalse( File.Exists( tempLongPathFilename ) );
				Assert.IsTrue( File.Exists( tempBackupLongPathFilename ) );
			}
			finally {
				if ( File.Exists( tempLongPathFilename ) ) {
					File.Delete( tempLongPathFilename );
				}

				File.Delete( tempLongPathFilename2 );
				File.Delete( tempBackupLongPathFilename );
			}
		}

		[Test]
		public void TestReplaceIgnoreMergeWithInvalidBackupPath() =>
			Assert.Throws<DirectoryNotFoundException>( () => {
				var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();
				var tempBackupLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\gibberish\" ).Append( "backup" ).ToString();

				using ( var fileStream = File.Create( tempLongPathFilename ) ) {
					fileStream.WriteByte( 42 );
				}

				var tempLongPathFilename2 = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename2.ext" ).ToString();

				using ( var fileStream = File.Create( tempLongPathFilename2 ) ) {
					fileStream.WriteByte( 52 );
				}

				try {
					const Boolean ignoreMetadataErrors = true;
					File.Replace( tempLongPathFilename, tempLongPathFilename2, tempBackupLongPathFilename, ignoreMetadataErrors );

					using ( var fileStream = File.OpenRead( tempLongPathFilename2 ) ) {
						Assert.AreEqual( 42, fileStream.ReadByte() );
					}

					Assert.IsFalse( File.Exists( tempLongPathFilename ) );
					Assert.IsTrue( File.Exists( tempBackupLongPathFilename ) );
				}
				finally {
					if ( File.Exists( tempLongPathFilename ) ) {
						File.Delete( tempLongPathFilename );
					}

					File.Delete( tempLongPathFilename2 );
					File.Delete( tempBackupLongPathFilename );
				}
			} );

		[Test]
		public void TestReplaceIgnoreMergeWithReadonlyBackupPath() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();
			var tempBackupPathName = new StringBuilder( longPathDirectory ).Append( @"\readonly" ).ToString();
			var di = new DirectoryInfo( tempBackupPathName );
			di.Create();

			var attr = di.Attributes;
			di.Attributes = attr | FileAttributes.ReadOnly;
			var tempBackupLongPathFilename = new StringBuilder( tempBackupPathName ).Append( @"\" ).Append( "backup" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename ) ) {
				fileStream.WriteByte( 42 );
			}

			var tempLongPathFilename2 = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename2.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename2 ) ) {
				fileStream.WriteByte( 52 );
			}

			try {
				const Boolean ignoreMetadataErrors = true;
				File.Replace( tempLongPathFilename, tempLongPathFilename2, tempBackupLongPathFilename, ignoreMetadataErrors );

				using ( var fileStream = File.OpenRead( tempLongPathFilename2 ) ) {
					Assert.AreEqual( 42, fileStream.ReadByte() );
				}

				Assert.IsFalse( File.Exists( tempLongPathFilename ) );
				Assert.IsTrue( File.Exists( tempBackupLongPathFilename ) );
			}
			finally {
				di.Attributes = attr;

				if ( File.Exists( tempLongPathFilename ) ) {
					File.Delete( tempLongPathFilename );
				}

				File.Delete( tempLongPathFilename2 );
				File.Delete( tempBackupLongPathFilename );

				if ( tempBackupPathName.Exists() ) {
					Directory.Delete( tempBackupPathName );
				}
			}
		}

		[Test]
		public void TestReplaceIgnoreMergeNulls() {
			const Boolean ignoreMetadataErrors = true;
			Assert.Throws<ArgumentNullException>( () => File.Replace( null, null, null, ignoreMetadataErrors ) );
		}

		[Test]
		public void TestReplaceIgnoreMergeNullDestination() {
			const Boolean ignoreMetadataErrors = true;
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				Assert.Throws<ArgumentNullException>( () => File.Replace( longPathFilename, null, null, ignoreMetadataErrors ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestAppendText() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				using ( var sw = File.AppendText( filename ) ) {
					sw.WriteLine( "end of file" );
				}

				var lines = File.ReadLines( filename );

				Assert.IsTrue( new[] {
					"beginning of file", "end of file"
				}.SequenceEqual( lines ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestCreateWithBuffersize() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			try {
				using ( var s = File.Create( tempLongPathFilename, 200 ) ) {
					s.WriteByte( 42 );
					s.Seek( 0, SeekOrigin.Begin );
					Assert.AreEqual( 42, s.ReadByte() );
				}
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEncrypt() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			try {
				using ( var s = File.Create( tempLongPathFilename, 200 ) ) { }

				var preAttrib = File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( ( FileAttributes ) 0, preAttrib & FileAttributes.Encrypted );
				File.Encrypt( tempLongPathFilename );
				var postAttrib = File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( FileAttributes.Encrypted, postAttrib & FileAttributes.Encrypted );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestEncryptNonExistentFile() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( Path.GetRandomFileName() ).ToString();
			Assert.Throws<FileNotFoundException>( () => File.Encrypt( tempLongPathFilename ) );
		}

		[Test]
		public void TestDecrypt() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			try {
				using ( var s = File.Create( tempLongPathFilename, 200 ) ) { }

				var preAttrib = File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( ( FileAttributes ) 0, preAttrib & FileAttributes.Encrypted );
				File.Encrypt( tempLongPathFilename );
				var postAttrib = File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( FileAttributes.Encrypted, postAttrib & FileAttributes.Encrypted );
				File.Decrypt( tempLongPathFilename );
				postAttrib = File.GetAttributes( tempLongPathFilename );
				Assert.AreEqual( ( FileAttributes ) 0, postAttrib & FileAttributes.Encrypted );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestDecryptNonExistentFile() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( Path.GetRandomFileName() ).ToString();
			Assert.Throws<FileNotFoundException>( () => File.Decrypt( tempLongPathFilename ) );
		}

		[Test]
		public void TestCreate() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			try {
				using ( var s = File.Create( tempLongPathFilename ) ) {
					s.WriteByte( 42 );
					s.Seek( 0, SeekOrigin.Begin );
					Assert.AreEqual( 42, s.ReadByte() );
				}
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestCreateWithBuffersizeFileOptions() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var s = File.Create( tempLongPathFilename, 200, FileOptions.DeleteOnClose ) ) {
				s.WriteByte( 42 );
				s.Seek( 0, SeekOrigin.Begin );
				Assert.AreEqual( 42, s.ReadByte() );
			}

			Assert.IsFalse( File.Exists( tempLongPathFilename ) );
		}

		[Test]
		public void TestOpenExisting() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			try {
				using ( var s = File.Create( tempLongPathFilename ) ) {
					s.WriteByte( 42 );
				}

				using ( var stream = File.Open( tempLongPathFilename, FileMode.Open ) ) {
					Assert.IsNotNull( stream );
				}
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestOpenNonExistent() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( Path.GetRandomFileName() ).ToString();

			Assert.Throws<FileNotFoundException>( () => {
				using ( File.Open( tempLongPathFilename, FileMode.Open ) ) { }
			} );
		}

		[Test]
		public void TestOpenWithAccessNonExistent() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( Path.GetRandomFileName() ).ToString();

			Assert.Throws<FileNotFoundException>( () => {
				using ( File.Open( tempLongPathFilename, FileMode.Open, FileAccess.Read ) ) { }
			} );
		}

		[Test]
		public void TestOpenWithAccess() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( Path.GetRandomFileName() ).ToString();

			try {
				using ( var s = File.Create( tempLongPathFilename ) ) {
					s.WriteByte( 42 );
				}

				using ( File.Open( tempLongPathFilename, FileMode.Open, FileAccess.Read ) ) { }
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestOpenRead() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			try {
				using ( var s = File.Create( tempLongPathFilename ) ) {
					s.WriteByte( 42 );
				}

				using ( var stream = File.OpenRead( tempLongPathFilename ) ) {
					Assert.AreEqual( 42, stream.ReadByte() );
				}
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestOpenWrite() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			try {
				using ( File.Create( tempLongPathFilename ) ) { }

				using ( var stream = File.OpenWrite( tempLongPathFilename ) ) {
					stream.WriteByte( 42 );
				}

				using ( var stream = File.OpenRead( tempLongPathFilename ) ) {
					Assert.AreEqual( 42, stream.ReadByte() );
				}
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestSetCreationTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );
				filename.SetCreationTime( dateTime );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.CreationTime, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetCreationTimeUtc() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );
				File.SetCreationTimeUtc( filename, dateTime );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.CreationTimeUtc, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestGetCreationTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = filename.GetCreationTime();
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.CreationTime, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestGetCreationTimeUTc() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = filename.GetCreationTimeUtc();
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.CreationTimeUtc, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastWriteTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );
				filename.SetLastWriteTime( dateTime );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.LastWriteTime, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastWriteTimeUtc() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );
				File.SetLastWriteTimeUtc( filename, dateTime );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.LastWriteTimeUtc, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestGetLastWriteTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = File.GetLastWriteTime( filename );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.LastWriteTime, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestGetLastWriteTimeUtc() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = File.GetLastWriteTimeUtc( filename );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.LastWriteTimeUtc, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastAccessTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.Now.AddDays( 1 );
				File.SetLastAccessTime( filename, dateTime );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.LastAccessTime, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestSetLastAccessTimeUtc() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = DateTime.UtcNow.AddDays( 1 );
				File.SetLastAccessTimeUtc( filename, dateTime );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.LastAccessTimeUtc, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestGetLastAccessTime() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = File.GetLastAccessTime( filename );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.LastAccessTime, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestGetLastAccessTimeUtc() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var dateTime = File.GetLastAccessTimeUtc( filename );
				var fi = new FileInfo( filename );
				Assert.AreEqual( fi.LastAccessTimeUtc, dateTime );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestOpenAppend() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file26.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );

			using ( var streamWriter = fi.CreateText() ) {
				streamWriter.WriteLine( "file26" );
			}

			try {
				using ( var fileStream = fi.Open( FileMode.Append ) ) {
					Assert.IsNotNull( fileStream );

					using ( var streamWriter = new StreamWriter( fileStream ) ) {
						streamWriter.WriteLine( "eof" );
					}
				}

				Assert.AreEqual( "file26" + Environment.NewLine + "eof" + Environment.NewLine, File.ReadAllText( fi.FullName ) );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestReadAllTextWithEncoding() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file26.ext" ).ToString();
			var fi = new FileInfo( tempLongPathFilename );

			try {
				using ( var streamWriter = File.CreateText( tempLongPathFilename, Encoding.Unicode ) ) {
					streamWriter.WriteLine( "file26" );
				}

				Assert.AreEqual( "file26" + Environment.NewLine, File.ReadAllText( fi.FullName, Encoding.Unicode ) );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestReadAllBytes() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename ) ) {
				fileStream.WriteByte( 42 );
			}

			try {
				Assert.IsTrue( new Byte[] {
					42
				}.SequenceEqual( File.ReadAllBytes( tempLongPathFilename ) ) );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestReadAllBytesOnHugeFile() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var fileStream = File.Create( tempLongPathFilename ) ) {
				fileStream.Seek( ( Int64 ) Int32.MaxValue + 1, SeekOrigin.Begin );
				fileStream.WriteByte( 42 );
			}

			Assert.Throws<IOException>( () => {
				try {
					Assert.IsTrue( new Byte[] {
						42
					}.SequenceEqual( File.ReadAllBytes( tempLongPathFilename ) ) );
				}
				finally {
					File.Delete( tempLongPathFilename );
				}
			} );
		}

		[Test]
		public void TestWriteAllBytes() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			var expected = new Byte[] {
				3, 4, 1, 5, 9, 2, 6, 5
			};

			File.WriteAllBytes( tempLongPathFilename, expected );

			try {
				Assert.IsTrue( expected.SequenceEqual( File.ReadAllBytes( tempLongPathFilename ) ) );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestReadAllLines() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				using ( var sw = File.AppendText( filename ) ) {
					sw.WriteLine( "end of file" );
				}

				var lines = File.ReadAllLines( filename );

				Assert.IsTrue( new[] {
					"beginning of file", "end of file"
				}.SequenceEqual( lines ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestWriteAllLines() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file26.ext" ).ToString();

			File.WriteAllLines( tempLongPathFilename, new[] {
				"file26"
			} );

			try {
				Assert.AreEqual( "file26" + Environment.NewLine, File.ReadAllText( tempLongPathFilename ) );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestWriteAllLinesWithEncoding() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file26.ext" ).ToString();

			File.WriteAllLines( tempLongPathFilename, new[] {
				"file26"
			}, Encoding.Unicode );

			try {

				Assert.AreEqual( "file26" + Environment.NewLine, File.ReadAllText( tempLongPathFilename, Encoding.Unicode ) );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestWriteAllLinesEnumerable() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file26.ext" ).ToString();

			File.WriteAllLines( tempLongPathFilename, new List<String> {
				"file26"
			} );

			try {
				Assert.AreEqual( "file26" + Environment.NewLine, File.ReadAllText( tempLongPathFilename ) );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestWriteAllLinesWithEncodingEnumerable() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file26.ext" ).ToString();

			File.WriteAllLines( tempLongPathFilename, new List<String> {
				"file26"
			}, Encoding.Unicode );

			try {

				Assert.AreEqual( "file26" + Environment.NewLine, File.ReadAllText( tempLongPathFilename, Encoding.Unicode ) );
			}
			finally {
				File.Delete( tempLongPathFilename );
			}
		}

		[Test]
		public void TestAppendAllText() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				File.AppendAllText( filename, "test" );
				Assert.AreEqual( "beginning of file" + Environment.NewLine + "test", File.ReadAllText( filename ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestAppendAllTextEncoding() {
			var filename = Util.CreateNewFileUnicode( longPathDirectory );

			try {
				File.AppendAllText( filename, "test", Encoding.Unicode );
				Assert.AreEqual( "beginning of file" + Environment.NewLine + "test", File.ReadAllText( filename, Encoding.Unicode ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestAppendAllLines() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				File.AppendAllLines( filename, new[] {
					"test1", "test2"
				} );

				Assert.AreEqual( "beginning of file" + Environment.NewLine + "test1" + Environment.NewLine + "test2" + Environment.NewLine, File.ReadAllText( filename ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		public void TestAppendAllLinesEncoding() {
			var filename = Util.CreateNewFileUnicode( longPathDirectory );

			try {
				File.AppendAllLines( filename, new[] {
					"test1", "test2"
				}, Encoding.Unicode );

				Assert.AreEqual( "beginning of file" + Environment.NewLine + "test1" + Environment.NewLine + "test2" + Environment.NewLine,
					File.ReadAllText( filename, Encoding.Unicode ) );
			}
			finally {
				File.Delete( filename );
			}
		}

		[Test]
		[Ignore( "does not work on some server/domain systems." )]
		public void TestGetAccessControl() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var security = File.GetAccessControl( filename );
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
				var security = File.GetAccessControl( filename, AccessControlSections.Access );
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
		public void TestSetAccessControl() {
			var filename = Util.CreateNewFile( longPathDirectory );

			try {
				var security = new FileSecurity();
				File.SetAccessControl( filename, security );
			}
			finally {
				File.Delete( filename );
			}
		}

		private static String longPathRoot;

        /// <remarks>
        ///     TODO: more realistic FileSecurity scenarios
        /// </remarks>
        [Test]
		public void TestCreateWithFileSecurity() {
			var tempLongPathFilename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "filename.ext" ).ToString();

			using ( var s = File.Create( tempLongPathFilename, 200, FileOptions.DeleteOnClose, new FileSecurity() ) ) {
				s.WriteByte( 42 );
				s.Seek( 0, SeekOrigin.Begin );
				Assert.AreEqual( 42, s.ReadByte() );
			}

			Assert.IsFalse( File.Exists( tempLongPathFilename ) );
		}

		[Test]
		public void TestGetLastWriteTimeOnMissingFileHasNoException() {
			var dt = File.GetLastWriteTime( "gibberish" );
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
	}
}