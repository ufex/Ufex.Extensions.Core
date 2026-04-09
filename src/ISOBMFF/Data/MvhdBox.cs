using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// mvhd — Movie Header Box. Contains overall movie-level metadata.
/// Both QTFF and ISOBMFF use the same field layout.
/// Version 0 uses 32-bit timestamps; version 1 uses 64-bit timestamps.
/// </summary>
internal class MvhdBox : Box
{
	/// <summary>
	/// Creation time in seconds since 1904-01-01 UTC.
	/// </summary>
	public UInt64 CreationTime { get; init; }

	/// <summary>
	/// Modification time in seconds since 1904-01-01 UTC.
	/// </summary>
	public UInt64 ModificationTime { get; init; }

	/// <summary>
	/// Movie-level timescale (ticks per second).
	/// </summary>
	public UInt32 Timescale { get; init; }

	/// <summary>
	/// Duration of the longest track, in movie timescale units.
	/// </summary>
	public UInt64 Duration { get; init; }

	/// <summary>
	/// Preferred playback rate as 16.16 fixed-point. 0x00010000 = normal speed.
	/// </summary>
	public UInt32 Rate { get; init; }

	/// <summary>
	/// Preferred volume as 8.8 fixed-point. 0x0100 = full volume.
	/// </summary>
	public UInt16 Volume { get; init; }

	/// <summary>
	/// Reserved 10 bytes.
	/// </summary>
	public Byte[] Reserved { get; init; }   // 10 bytes

	/// <summary>
	/// 3x3 transformation matrix stored as 9 x Int32.
	/// </summary>
	public Int32[] Matrix { get; init; }   // 9 x 4 bytes = 36 bytes

	/// <summary>
	/// Pre-defined / reserved region (6 x UInt32).
	/// In QTFF this may store preview/poster time; in ISOBMFF these are zero.
	/// </summary>
	public UInt32[] PreDefined { get; init; }   // 6 x 4 bytes = 24 bytes

	/// <summary>
	/// One greater than the largest existing track ID.
	/// </summary>
	public UInt32 NextTrackID { get; init; }

	public MvhdBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		if (Version == 1)
		{
			CreationTime = fr.ReadUInt64();
			ModificationTime = fr.ReadUInt64();
			Timescale = fr.ReadUInt32();
			Duration = fr.ReadUInt64();
		}
		else
		{
			CreationTime = fr.ReadUInt32();
			ModificationTime = fr.ReadUInt32();
			Timescale = fr.ReadUInt32();
			Duration = fr.ReadUInt32();
		}

		Rate = fr.ReadUInt32();
		Volume = fr.ReadUInt16();
		Reserved = fr.ReadBytes(10);

		Matrix = new Int32[9];
		for (int i = 0; i < 9; i++)
			Matrix[i] = fr.ReadInt32();

		PreDefined = new UInt32[6];
		for (int i = 0; i < 6; i++)
			PreDefined[i] = fr.ReadUInt32();

		NextTrackID = fr.ReadUInt32();
	}
}
