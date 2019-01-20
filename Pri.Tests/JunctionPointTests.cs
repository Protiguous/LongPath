// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "JunctionPointTests.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "JunctionPointTests.cs" was last formatted by Protiguous on 2019/01/12 at 8:14 PM.

namespace Tests {

	using System;
	using System.IO;
	using NUnit.Framework;
	using Pri.LongPath;
	using Directory = Pri.LongPath.Directory;
	using DirectoryInfo = Pri.LongPath.DirectoryInfo;
	using File = Pri.LongPath.File;
	using Path = Pri.LongPath.Path;

	[TestFixture]
	public class JunctionPointTest {

		[SetUp]
		public void CreateTempFolder() {
			this.tempFolder = Path.GetTempFileName();
			File.Delete( this.tempFolder );
			this.tempFolder.CreateDirectory();
		}

		[TearDown]
		public void DeleteTempFolder() {
			if ( this.tempFolder != null ) {
				foreach ( var file in new DirectoryInfo( this.tempFolder ).GetFileSystemInfos() ) {
					file.Delete();
				}

				Directory.Delete( this.tempFolder );
				this.tempFolder = null;
			}
		}

		private String tempFolder;

		[Test]
		public void Create_OverwritesIfSpecifiedAndDirectoryExists() {
			var targetFolder = this.tempFolder.Combine( "ADirectory" );
			var junctionPoint = this.tempFolder.Combine( "SymLink" );

			junctionPoint.CreateDirectory();
			targetFolder.CreateDirectory();

			JunctionPoint.Create( junctionPoint, targetFolder, true );

			Assert.AreEqual( targetFolder, JunctionPoint.GetTarget( junctionPoint ) );
		}

		[Test]
		public void Create_ThrowsIfOverwriteNotSpecifiedAndDirectoryExists() {
			var targetFolder = this.tempFolder.Combine( "ADirectory" );
			var junctionPoint = this.tempFolder.Combine( "SymLink" );

			junctionPoint.CreateDirectory();

			Assert.Throws<IOException>( () => JunctionPoint.Create( junctionPoint, targetFolder, false ), "Directory already exists and overwrite parameter is false." );
		}

		[Test]
		public void Create_ThrowsIfTargetDirectoryDoesNotExist() {
			var targetFolder = this.tempFolder.Combine( "ADirectory" );
			var junctionPoint = this.tempFolder.Combine( "SymLink" );

			Assert.Throws<IOException>( () => JunctionPoint.Create( junctionPoint, targetFolder, false ), "Target path does not exist or is not a directory." );
		}

		[Test]
		public void Create_VerifyExists_GetTarget_Delete() {
			var targetFolder = this.tempFolder.Combine( "ADirectory" );
			var junctionPoint = this.tempFolder.Combine( "SymLink" );

			targetFolder.CreateDirectory();

			try {
				File.Create( targetFolder.Combine( "AFile" ) ).Close();

				try {

					// Verify behavior before junction point created.
					Assert.IsFalse( File.Exists( junctionPoint.Combine( "AFile" ) ), "File should not be located until junction point created." );

					Assert.IsFalse( JunctionPoint.Exists( junctionPoint ), "Junction point not created yet." );

					// Create junction point and confirm its properties.
					JunctionPoint.Create( junctionPoint, targetFolder, overwrite: false );

					Assert.IsTrue( JunctionPoint.Exists( junctionPoint ), "Junction point exists now." );

					Assert.AreEqual( targetFolder, JunctionPoint.GetTarget( junctionPoint ) );

					Assert.IsTrue( File.Exists( junctionPoint.Combine( "AFile" ) ), "File should be accessible via the junction point." );

					// Delete junction point.
					JunctionPoint.Delete( junctionPoint );

					Assert.IsFalse( JunctionPoint.Exists( junctionPoint ), "Junction point should not exist now." );

					Assert.IsFalse( File.Exists( junctionPoint.Combine( "AFile" ) ), "File should not be located after junction point deleted." );

					Assert.IsFalse( junctionPoint.Exists(), "Ensure directory was deleted too." );
				}
				finally {
					File.Delete( targetFolder.Combine( "AFile" ) );
				}
			}
			finally {
				Directory.Delete( targetFolder );
			}
		}

		[Test]
		public void Delete_CalledOnADirectoryThatIsNotAJunctionPoint() =>
			Assert.Throws<IOException>( () => JunctionPoint.Delete( this.tempFolder ), "Unable to delete junction point." );

		[Test]
		public void Delete_CalledOnAFile() {
			File.Create( this.tempFolder.Combine( "AFile" ) ).Close();

			Assert.Throws<IOException>( () => JunctionPoint.Delete( this.tempFolder.Combine( "AFile" ) ), "Path is not a junction point." );
		}

		[Test]
		public void Delete_NonExistentJunctionPoint() =>

			// Should do nothing.
			JunctionPoint.Delete( this.tempFolder.Combine( "SymLink" ) );

		[Test]
		public void Exists_IsADirectory() {
			File.Create( this.tempFolder.Combine( "AFile" ) ).Close();

			Assert.IsFalse( JunctionPoint.Exists( this.tempFolder.Combine( "AFile" ) ) );
		}

		[Test]
		public void Exists_NoSuchFile() => Assert.IsFalse( JunctionPoint.Exists( this.tempFolder.Combine( "$$$NoSuchFolder$$$" ) ) );

		[Test]
		public void GetTarget_CalledOnADirectoryThatIsNotAJunctionPoint() =>
			Assert.Throws<IOException>( () => JunctionPoint.GetTarget( this.tempFolder ), "Path is not a junction point." );

		[Test]
		public void GetTarget_CalledOnAFile() {
			File.Create( this.tempFolder.Combine( "AFile" ) ).Close();

			Assert.Throws<IOException>( () => JunctionPoint.GetTarget( this.tempFolder.Combine( "AFile" ) ), "Path is not a junction point." );
		}

		[Test]
		public void GetTarget_NonExistentJunctionPoint() =>
			Assert.Throws<IOException>( () => JunctionPoint.GetTarget( this.tempFolder.Combine( "SymLink" ) ), "Unable to open reparse point." );
	}
}