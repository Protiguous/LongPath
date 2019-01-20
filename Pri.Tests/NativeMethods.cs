// Copyright © Rick@AIBrain.org and Protiguous. All Rights Reserved.
//
// This entire copyright notice and license must be retained and must be kept visible
// in any binaries, libraries, repositories, and source code (directly or derived) from
// our binaries, libraries, projects, or solutions.
//
// This source code contained in "NativeMethods.cs" belongs to Protiguous@Protiguous.com and
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
// Project: "Pri.Tests", "NativeMethods.cs" was last formatted by Protiguous on 2019/01/12 at 8:14 PM.

namespace Tests {

	using System;
	using System.Runtime.InteropServices;
	using System.Text;

	internal static class NativeMethods {

		[Flags]
		public enum FileSystemFeature : UInt32 {

			/// <summary>
			///     The file system supports case-sensitive file names.
			/// </summary>
			CaseSensitiveSearch = 1,

			/// <summary>
			///     The file system preserves the case of file names when it places a name on disk.
			/// </summary>
			CasePreservedNames = 2,

			/// <summary>
			///     The file system supports Unicode in file names as they appear on disk.
			/// </summary>
			UnicodeOnDisk = 4,

			/// <summary>
			///     The file system preserves and enforces access control lists (ACL).
			/// </summary>
			PersistentACLS = 8,

			/// <summary>
			///     The file system supports file-based compression.
			/// </summary>
			FileCompression = 0x10,

			/// <summary>
			///     The file system supports disk quotas.
			/// </summary>
			VolumeQuotas = 0x20,

			/// <summary>
			///     The file system supports sparse files.
			/// </summary>
			SupportsSparseFiles = 0x40,

			/// <summary>
			///     The file system supports re-parse points.
			/// </summary>
			SupportsReparsePoints = 0x80,

			/// <summary>
			///     The specified volume is a compressed volume, for example, a DoubleSpace volume.
			/// </summary>
			VolumeIsCompressed = 0x8000,

			/// <summary>
			///     The file system supports object identifiers.
			/// </summary>
			SupportsObjectIDs = 0x10000,

			/// <summary>
			///     The file system supports the Encrypted File System (EFS).
			/// </summary>
			SupportsEncryption = 0x20000,

			/// <summary>
			///     The file system supports named streams.
			/// </summary>
			NamedStreams = 0x40000,

			/// <summary>
			///     The specified volume is read-only.
			/// </summary>
			ReadOnlyVolume = 0x80000,

			/// <summary>
			///     The volume supports a single sequential write.
			/// </summary>
			SequentialWriteOnce = 0x100000,

			/// <summary>
			///     The volume supports transactions.
			/// </summary>
			SupportsTransactions = 0x200000
		}

		[DllImport( "Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		public static extern Boolean GetVolumeInformation( String RootPathName, StringBuilder VolumeNameBuffer, Int32 VolumeNameSize, out UInt32 VolumeSerialNumber,
			out UInt32 MaximumComponentLength, out FileSystemFeature FileSystemFlags, StringBuilder FileSystemNameBuffer, Int32 nFileSystemNameSize );
	}
}