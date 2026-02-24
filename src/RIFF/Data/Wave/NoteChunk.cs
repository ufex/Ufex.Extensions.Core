using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// note - Note chunk. Associates a comment/note with a cue point.
/// Found inside a LIST "adtl" chunk.
/// </summary>
internal class NoteChunk : Chunk
{
	/// <summary>
	/// The cue point ID that this note refers to.
	/// </summary>
	public UInt32 CuePointID { get; init; }

	/// <summary>
	/// The note text (null-terminated ASCII string).
	/// </summary>
	public byte[] Text { get; init; }

	public string TextString => System.Text.Encoding.ASCII.GetString(Text);

	public NoteChunk(FileReader fr) : base(fr)
	{
		CuePointID = fr.ReadUInt32();
		Text = fr.ReadBytesUntil(0x00);
	}
}
