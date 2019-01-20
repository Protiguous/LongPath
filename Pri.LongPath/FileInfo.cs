// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "FileInfo.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.LongPath", "FileInfo.cs" was last formatted by Protiguous on 2019/01/12 at 8:26 PM.

namespace Pri.LongPath {

    using System;
    using System.IO;
    using System.Security.AccessControl;
    using System.Text;
    using JetBrains.Annotations;

    public class FileInfo : FileSystemInfo {

        [NotNull]
        public String DirectoryName => this.FullPath.GetDirectoryName();

        public override Boolean Exists {
            get {

                if ( this.state == State.Uninitialized ) {
                    this.Refresh();
                }

                return this.state == State.Initialized && ( this.data.fileAttributes & FileAttributes.Directory ) != FileAttributes.Directory;
            }
        }

        public Boolean IsReadOnly {
            get => ( this.Attributes & FileAttributes.ReadOnly ) != 0;

            set {

                if ( value ) {
                    this.Attributes |= FileAttributes.ReadOnly;

                    return;
                }

                this.Attributes &= ~FileAttributes.ReadOnly;
            }
        }

        public Int64 Length => this.GetFileLength();

        public override String Name { get; }

        [NotNull]
        public System.IO.FileInfo SysFileInfo => new System.IO.FileInfo( this.FullPath );

        [NotNull]
        public override System.IO.FileSystemInfo SystemInfo => this.SysFileInfo;

        [CanBeNull]
        public DirectoryInfo Directory => new DirectoryInfo( this.DirectoryName );

        public FileInfo( [NotNull] String fileName ) {
            this.OriginalPath = fileName;
            this.FullPath = fileName.GetFullPath();
            this.Name = fileName.GetFileName();
            this.DisplayPath = GetDisplayPath( fileName );
        }

        private static String GetDisplayPath( String originalPath ) => originalPath;

        private Int64 GetFileLength() {
            if ( this.state == State.Uninitialized ) {
                this.Refresh();
            }

            if ( this.state == State.Error ) {
                Common.ThrowIOError( this.errorCode, this.FullPath );
            }

            return ( ( Int64 ) this.data.fileSizeHigh << 32 ) | ( this.data.fileSizeLow & 0xFFFFFFFFL );
        }

        [NotNull]
        public StreamWriter AppendText() => File.CreateStreamWriter( this.FullPath, true );

        [NotNull]
        public FileInfo CopyTo( [NotNull] String destFileName ) => this.CopyTo( destFileName, false );

        [NotNull]
        public FileInfo CopyTo( [NotNull] String destFileName, Boolean overwrite ) {
            File.Copy( this.FullPath, destFileName, overwrite );

            return new FileInfo( destFileName );
        }

        [NotNull]
        public FileStream Create() => File.Create( this.FullPath );

        [NotNull]
        public StreamWriter CreateText() => File.CreateStreamWriter( this.FullPath, false );

        public void Decrypt() => File.Decrypt( this.FullPath );

        public override void Delete() => File.Delete( this.FullPath );

        public void Encrypt() => File.Encrypt( this.FullPath );

        [NotNull]
        public FileSecurity GetAccessControl() => File.GetAccessControl( this.FullPath );

        [NotNull]
        public FileSecurity GetAccessControl( AccessControlSections includeSections ) => File.GetAccessControl( this.FullPath, includeSections );

        public void MoveTo( String destFileName ) => File.Move( this.FullPath, destFileName );

        [NotNull]
        public FileStream Open( FileMode mode ) => this.Open( mode, FileAccess.ReadWrite, FileShare.None );

        [NotNull]
        public FileStream Open( FileMode mode, FileAccess access ) => this.Open( mode, access, FileShare.None );

        [NotNull]
        public FileStream Open( FileMode mode, FileAccess access, FileShare share ) => File.Open( this.FullPath, mode, access, share, 4096, FileOptions.SequentialScan );

        [NotNull]
        public FileStream OpenRead() => File.Open( this.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.None );

        [NotNull]
        public StreamReader OpenText() => File.CreateStreamReader( this.FullPath, Encoding.UTF8, true, 1024 );

        [NotNull]
        public FileStream OpenWrite() => File.Open( this.FullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None );

        [NotNull]
        public FileInfo Replace( [NotNull] String destinationFilename, String backupFilename ) => this.Replace( destinationFilename, backupFilename, false );

        [NotNull]
        public FileInfo Replace( [NotNull] String destinationFilename, String backupFilename, Boolean ignoreMetadataErrors ) {
            File.Replace( this.FullPath, destinationFilename, backupFilename, ignoreMetadataErrors );

            return new FileInfo( destinationFilename );
        }

        public void SetAccessControl( [NotNull] FileSecurity security ) => File.SetAccessControl( this.FullPath, security );

        public override String ToString() => this.DisplayPath;
    }
}