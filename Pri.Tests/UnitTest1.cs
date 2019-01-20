// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "UnitTest1.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "UnitTest1.cs" was last formatted by Protiguous on 2019/01/12 at 8:17 PM.

namespace Tests {

	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using NUnit.Framework;
	using Pri.LongPath;
	using Directory = Pri.LongPath.Directory;
	using DirectoryInfo = System.IO.DirectoryInfo;
	using File = System.IO.File;
	using FileInfo = System.IO.FileInfo;
	using FileSystemInfo = System.IO.FileSystemInfo;
	using Path = System.IO.Path;

	[TestFixture]
	public class UnitTest1 {

		[SetUp]
		public void SetUp() {
			longPathDirectory = Util.MakeLongPath( TestContext.CurrentContext.TestDirectory );

			longPathRoot = longPathDirectory.Substring( 0,
				TestContext.CurrentContext.TestDirectory.Length + 1 + longPathDirectory.Substring( TestContext.CurrentContext.TestDirectory.Length + 1 ).IndexOf( '\\' ) );

			longPathDirectory.CreateDirectory();
			Debug.Assert( longPathDirectory.Exists() );
		}

		[TearDown]
		public void TearDown() {
			Directory.Delete( longPathRoot, true );
			Debug.Assert( !longPathDirectory.Exists() );
		}

		private static String longPathDirectory;

		private static String longPathRoot;

		private static String MemberToMethodString( MemberInfo member ) {
			var method = member as MethodInfo;

			if ( method == null ) {
				return member.Name;
			}

			var parameters = method.GetParameters();

			return
				$"{method.ReturnType.Name} {method.Name}({( !parameters.Any() ? "" : parameters.Select( e => e.ParameterType.Name ).Aggregate( ( c, n ) => c + ", " + n ) )})";
		}

		[Test]
		public void DirectoryClassIsComplete() {
			var systemIoDirectoryMembers =
				typeof( System.IO.Directory ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );

			var directoryMembers = typeof( Directory ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
			var missing = "";

			if ( systemIoDirectoryMembers.Length != directoryMembers.Length ) {
				var systemIoDirectoryMembersOrdered = systemIoDirectoryMembers.OrderBy( e => e.Name );
				var systemIoDirectoryMemberNames = systemIoDirectoryMembersOrdered.Select( MemberToMethodString );
				var directoryMembersOrdered = directoryMembers.OrderBy( e => e.Name );
				var directoryMemberNames = directoryMembersOrdered.Select( MemberToMethodString );
				var missingCollection = directoryMemberNames.Except( systemIoDirectoryMemberNames );
				var missingCollection2 = systemIoDirectoryMemberNames.Except( directoryMemberNames );

				missing = ( !missingCollection2.Any() ? "" : "missing: " + missingCollection2.Aggregate( ( c, n ) => c + ", " + n ) + Environment.NewLine ) +
				          ( !missingCollection.Any() ? "" : "extra: " + missingCollection.Aggregate( ( c, n ) => c + ", " + n ) );
			}

			Assert.AreEqual( systemIoDirectoryMembers.Length, directoryMembers.Length, missing );
		}

		[Test]
		public void DirectoryInfoClassIsComplete() {
			var systemIoDirectoryInfoMembers =
				typeof( DirectoryInfo ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );

			var DirectoryInfoMembers =
				typeof( Pri.LongPath.DirectoryInfo ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );

			var missing = "";

			if ( systemIoDirectoryInfoMembers.Length != DirectoryInfoMembers.Length ) {
				var systemIoDirectoryInfoMembersOrdered = systemIoDirectoryInfoMembers.OrderBy( e => e.Name );
				var systemIoDirectoryInfoMemberNames = systemIoDirectoryInfoMembersOrdered.Select( MemberToMethodString );
				var DirectoryInfoMembersOrdered = DirectoryInfoMembers.OrderBy( e => e.Name );
				var DirectoryInfoMemberNames = DirectoryInfoMembersOrdered.Select( MemberToMethodString );
				var missingCollection = DirectoryInfoMemberNames.Except( systemIoDirectoryInfoMemberNames );
				var missingCollection2 = systemIoDirectoryInfoMemberNames.Except( DirectoryInfoMemberNames );

				missing = ( !missingCollection2.Any() ? "" : "missing: " + missingCollection2.Aggregate( ( c, n ) => c + ", " + n ) + Environment.NewLine ) +
				          ( !missingCollection.Any() ? "" : "extra: " + missingCollection.Aggregate( ( c, n ) => c + ", " + n ) );
			}

			Assert.LessOrEqual( systemIoDirectoryInfoMembers.Length, DirectoryInfoMembers.Length, missing );
		}

		[Test]
		public void FileClassIsComplete() {
			var systemIoFileMembers = typeof( File ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
			var fileMembers = typeof( Pri.LongPath.File ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
			var missing = "";

			if ( systemIoFileMembers.Length != fileMembers.Length ) {
				var systemIoFileMemberNames = systemIoFileMembers.OrderBy( e => e.Name ).Select( MemberToMethodString );
				missing = systemIoFileMemberNames.Aggregate( ( c, n ) => c + ", " + n );
				var fileMemberNames = fileMembers.OrderBy( e => e.Name ).Select( MemberToMethodString );
				missing = fileMemberNames.Aggregate( ( c, n ) => c + ", " + n );
				var missingCollection = fileMemberNames.Except( systemIoFileMemberNames );
				var missingCollection2 = systemIoFileMemberNames.Except( fileMemberNames );

				missing = ( !missingCollection2.Any() ? "" : "missing: " + missingCollection2.Aggregate( ( c, n ) => c + ", " + n ) + Environment.NewLine ) +
				          ( !missingCollection.Any() ? "" : "extra: " + missingCollection.Aggregate( ( c, n ) => c + ", " + n ) );
			}

			Assert.AreEqual( systemIoFileMembers.Length, fileMembers.Length, missing );
		}

		[Test]
		public void FileInfoClassIsComplete() {
			var systemIoFileInfoMembers = typeof( FileInfo ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
			var FileInfoMembers = typeof( Pri.LongPath.FileInfo ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
			var missing = "";

			if ( systemIoFileInfoMembers.Length != FileInfoMembers.Length ) {
				var systemIoFileInfoMembersOrdered = systemIoFileInfoMembers.OrderBy( e => e.Name );
				var systemIoFileInfoMemberNames = systemIoFileInfoMembersOrdered.Select( MemberToMethodString );
				var FileInfoMembersOrdered = FileInfoMembers.OrderBy( e => e.Name );
				var FileInfoMemberNames = FileInfoMembersOrdered.Select( MemberToMethodString );
				var missingCollection = FileInfoMemberNames.Except( systemIoFileInfoMemberNames );
				var missingCollection2 = systemIoFileInfoMemberNames.Except( FileInfoMemberNames );

				missing = ( !missingCollection2.Any() ? "" : "missing: " + missingCollection2.Aggregate( ( c, n ) => c + ", " + n ) + Environment.NewLine ) +
				          ( !missingCollection.Any() ? "" : "extra: " + missingCollection.Aggregate( ( c, n ) => c + ", " + n ) );
			}

			Assert.LessOrEqual( systemIoFileInfoMembers.Length, FileInfoMembers.Length, missing );
		}

		[Test]
		public void FileSystemInfoClassIsComplete() {
			var systemIoFileSystemInfoMembers =
				typeof( FileSystemInfo ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );

			var FileSystemInfoMembers =
				typeof( Pri.LongPath.FileSystemInfo ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );

			var missing = "";

			if ( systemIoFileSystemInfoMembers.Length != FileSystemInfoMembers.Length ) {
				var systemIoFileSystemInfoMembersOrdered = systemIoFileSystemInfoMembers.OrderBy( e => e.Name );
				var systemIoFileSystemInfoMemberNames = systemIoFileSystemInfoMembersOrdered.Select( MemberToMethodString );
				var FileSystemInfoMembersOrdered = FileSystemInfoMembers.OrderBy( e => e.Name );
				var FileSystemInfoMemberNames = FileSystemInfoMembersOrdered.Select( MemberToMethodString );
				var missingCollection = FileSystemInfoMemberNames.Except( systemIoFileSystemInfoMemberNames );
				var missingCollection2 = systemIoFileSystemInfoMemberNames.Except( FileSystemInfoMemberNames );

				missing = ( !missingCollection2.Any() ? "" : "missing: " + missingCollection2.Aggregate( ( c, n ) => c + ", " + n ) + Environment.NewLine ) +
				          ( !missingCollection.Any() ? "" : "extra: " + missingCollection.Aggregate( ( c, n ) => c + ", " + n ) );
			}

			Assert.LessOrEqual( systemIoFileSystemInfoMembers.Length, FileSystemInfoMembers.Length, missing );
		}

		[Test]
		public void PathClassIsComplete() {
			var systemIoPathMembers = typeof( Path ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
			var PathMembers = typeof( Pri.LongPath.Path ).GetMembers( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
			var missing = "";

			if ( systemIoPathMembers.Length != PathMembers.Length ) {
				var systemIoPathMembersOrdered = systemIoPathMembers.OrderBy( e => e.Name );
				var systemIoPathMemberNames = systemIoPathMembersOrdered.Select( MemberToMethodString );
				var PathMembersOrdered = PathMembers.OrderBy( e => e.Name );
				var PathMemberNames = PathMembersOrdered.Select( MemberToMethodString );
				var missingCollection = PathMemberNames.Except( systemIoPathMemberNames );
				var missingCollection2 = systemIoPathMemberNames.Except( PathMemberNames );

				missing = ( !missingCollection2.Any() ? "" : "missing: " + missingCollection2.Aggregate( ( c, n ) => c + ", " + n ) + Environment.NewLine ) +
				          ( !missingCollection.Any() ? "" : "extra: " + missingCollection.Aggregate( ( c, n ) => c + ", " + n ) );
			}

			Assert.AreEqual( systemIoPathMembers.Length, PathMembers.Length, missing );
		}

		[Test]
		public void TestProblemWithSystemIoExists() =>
			Assert.Throws<PathTooLongException>( () => {
				var filename = new StringBuilder( longPathDirectory ).Append( @"\" ).Append( "file4.ext" ).ToString();

				using ( var writer = Pri.LongPath.File.CreateText( filename ) ) {
					writer.WriteLine( "test" );
				}

				Assert.IsTrue( Pri.LongPath.File.Exists( filename ) );

				try {
					using ( var fileStream = new FileStream( filename, FileMode.Append, FileAccess.Write, FileShare.None ) ) {
						using ( var bw = new BinaryWriter( fileStream ) ) {
							bw.Write( 10u );
						}
					}
				}
				finally {
					Pri.LongPath.File.Delete( filename );
				}
			} );

		[Test]
		public void WhatHappensWithBclPathGetDiretoryNameAndRelatiePath() {
			var text = Path.GetDirectoryName( @"foo\bar\baz" );
			Assert.AreEqual( @"foo\bar", text );
		}
	}
}