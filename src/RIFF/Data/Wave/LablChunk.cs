using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// labl - Label chunk. Associates a text label with a cue point.
/// Found inside a LIST "adtl" chunk.
/// </summary>
internal class LablChunk : Chunk
{
	/// <summary>
	/// The cue point ID that this label refers to.
	/// </summary>
	public UInt32 CuePointID { get; init; }

	/// <summary>
	/// The label text (null-terminated ASCII string).
	/// </summary>
	public byte[] Text { get; init; }

	public string TextString => System.Text.Encoding.ASCII.GetString(Text);

	public LablChunk(FileReader fr) : base(fr)
	{
		CuePointID = fr.ReadUInt32();
		Text = fr.ReadBytesUntil(0x00);
	}
}
