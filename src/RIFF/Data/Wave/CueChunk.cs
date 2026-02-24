using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// cue - Cue Points chunk. Identifies a series of positions in the waveform data.
/// </summary>
internal class CueChunk : Chunk
{
	/// <summary>
	/// Number of cue points in the list.
	/// </summary>
	public UInt32 CuePointCount { get; init; }

	/// <summary>
	/// Array of cue points.
	/// </summary>
	public CuePoint[] CuePoints { get; init; }

	public CueChunk(FileReader fr) : base(fr)
	{
		CuePointCount = fr.ReadUInt32();
		CuePoints = new CuePoint[CuePointCount];
		for (int i = 0; i < CuePointCount; i++)
		{
			CuePoints[i] = new CuePoint(fr);
		}
	}
}

/// <summary>
/// A single cue point entry within a cue chunk.
/// </summary>
internal class CuePoint
{
	/// <summary>
	/// Unique identifier for the cue point.
	/// </summary>
	public UInt32 ID { get; init; }

	/// <summary>
	/// The play order position. Specifies the sample position for the cue point
	/// when used in a playlist.
	/// </summary>
	public UInt32 Position { get; init; }

	/// <summary>
	/// The FOURCC chunk ID of the chunk containing the cue point (e.g., "data").
	/// </summary>
	public Byte[] DataChunkID { get; init; }

	/// <summary>
	/// The byte offset of the start of the block containing the position.
	/// </summary>
	public UInt32 ChunkStart { get; init; }

	/// <summary>
	/// The byte offset of the start of the block relative to the start of the data section.
	/// </summary>
	public UInt32 BlockStart { get; init; }

	/// <summary>
	/// The sample offset of the cue point relative to the start of the block.
	/// </summary>
	public UInt32 SampleOffset { get; init; }

	public string DataChunkIDString => System.Text.Encoding.ASCII.GetString(DataChunkID);

	public CuePoint(FileReader fr)
	{
		ID = fr.ReadUInt32();
		Position = fr.ReadUInt32();
		DataChunkID = fr.ReadBytes(4);
		ChunkStart = fr.ReadUInt32();
		BlockStart = fr.ReadUInt32();
		SampleOffset = fr.ReadUInt32();
	}
}
