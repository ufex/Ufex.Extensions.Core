using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// tkhd — Track Header Box. Contains per-track metadata.
/// Both QTFF and ISOBMFF use the same field layout, though flag bit 0x0008
/// has different semantics (TrackInPoster vs track_size_is_aspect_ratio).
/// </summary>
internal class TkhdBox : Box
{
	public UInt64 CreationTime { get; init; }
	public UInt64 ModificationTime { get; init; }
	public UInt32 TrackID { get; init; }
	public UInt32 Reserved1 { get; init; }
	public UInt64 Duration { get; init; }
	public UInt32[] Reserved2 { get; init; }   // 2 x 4 bytes
	public Int16 Layer { get; init; }
	public Int16 AlternateGroup { get; init; }
	public UInt16 Volume { get; init; }
	public UInt16 Reserved3 { get; init; }
	public Int32[] Matrix { get; init; }   // 9 x 4 bytes
	public UInt32 Width { get; init; }     // 16.16 fixed-point
	public UInt32 Height { get; init; }    // 16.16 fixed-point

	public TkhdBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		if (Version == 1)
		{
			CreationTime = fr.ReadUInt64();
			ModificationTime = fr.ReadUInt64();
			TrackID = fr.ReadUInt32();
			Reserved1 = fr.ReadUInt32();
			Duration = fr.ReadUInt64();
		}
		else
		{
			CreationTime = fr.ReadUInt32();
			ModificationTime = fr.ReadUInt32();
			TrackID = fr.ReadUInt32();
			Reserved1 = fr.ReadUInt32();
			Duration = fr.ReadUInt32();
		}

		Reserved2 = [ fr.ReadUInt32(), fr.ReadUInt32() ];
		Layer = fr.ReadInt16();
		AlternateGroup = fr.ReadInt16();
		Volume = fr.ReadUInt16();
		Reserved3 = fr.ReadUInt16();

		Matrix = new Int32[9];
		for (int i = 0; i < 9; i++)
			Matrix[i] = fr.ReadInt32();

		Width = fr.ReadUInt32();
		Height = fr.ReadUInt32();
	}
}
