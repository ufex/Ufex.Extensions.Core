using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// ltxt - Labeled Text chunk. Associates text with a range of samples 
/// anchored to a cue point. Found inside a LIST "adtl" chunk.
/// </summary>
internal class LtxtChunk : Chunk
{
	/// <summary>
	/// The cue point ID that this labeled text refers to.
	/// </summary>
	public UInt32 CuePointID { get; init; }

	/// <summary>
	/// The number of samples from the cue point to the end of the text region.
	/// </summary>
	public UInt32 SampleLength { get; init; }

	/// <summary>
	/// The FOURCC purpose of the text (e.g., "rgn " for region, "scrp" for script).
	/// </summary>
	public Byte[] PurposeID { get; init; }

	/// <summary>
	/// The country code for the text.
	/// </summary>
	public UInt16 Country { get; init; }

	/// <summary>
	/// The language for the text.
	/// </summary>
	public UInt16 Language { get; init; }

	/// <summary>
	/// The dialect for the text.
	/// </summary>
	public UInt16 Dialect { get; init; }

	/// <summary>
	/// The code page for the text.
	/// </summary>
	public UInt16 CodePage { get; init; }

	/// <summary>
	/// The labeled text data (remainder of chunk after the fixed fields).
	/// </summary>
	public byte[] Text { get; init; }

	public string PurposeIDString => System.Text.Encoding.ASCII.GetString(PurposeID);
	public string TextString => System.Text.Encoding.ASCII.GetString(Text);

	public LtxtChunk(FileReader fr) : base(fr)
	{
		CuePointID = fr.ReadUInt32();
		SampleLength = fr.ReadUInt32();
		PurposeID = fr.ReadBytes(4);
		Country = fr.ReadUInt16();
		Language = fr.ReadUInt16();
		Dialect = fr.ReadUInt16();
		CodePage = fr.ReadUInt16();

		// Remaining bytes are the text. Fixed fields = 20 bytes.
		int textLength = (int)Size - 20;
		if (textLength > 0)
		{
			Text = fr.ReadBytes(textLength);
		}
		else
		{
			Text = [];
		}
	}
}
