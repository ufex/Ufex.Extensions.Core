using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// plst - Playlist chunk. Specifies a play order for a series of cue points.
/// </summary>
internal class PlstChunk : Chunk
{
	/// <summary>
	/// Number of segments in the playlist.
	/// </summary>
	public UInt32 SegmentCount { get; init; }

	/// <summary>
	/// Array of playlist segments.
	/// </summary>
	public PlaylistSegment[] Segments { get; init; }

	public PlstChunk(FileReader fr) : base(fr)
	{
		SegmentCount = fr.ReadUInt32();
		Segments = new PlaylistSegment[SegmentCount];
		for (int i = 0; i < SegmentCount; i++)
		{
			Segments[i] = new PlaylistSegment(fr);
		}
	}
}

/// <summary>
/// A single segment entry within a playlist chunk.
/// </summary>
internal class PlaylistSegment
{
	/// <summary>
	/// Cue point ID that this segment refers to.
	/// </summary>
	public UInt32 CuePointID { get; init; }

	/// <summary>
	/// Length of the section in samples.
	/// </summary>
	public UInt32 Length { get; init; }

	/// <summary>
	/// Number of times to play the section.
	/// </summary>
	public UInt32 Repeats { get; init; }

	public PlaylistSegment(FileReader fr)
	{
		CuePointID = fr.ReadUInt32();
		Length = fr.ReadUInt32();
		Repeats = fr.ReadUInt32();
	}
}
