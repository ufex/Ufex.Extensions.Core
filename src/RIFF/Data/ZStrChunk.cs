using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

internal class ZStrChunk : Chunk
{
	public byte[] Text { get; init; }

	// TODO: We should probably be detecting the encoding based on the CSET chunk if it exists, but for now we'll just assume UTF-8
	public string TextString => System.Text.Encoding.UTF8.GetString(Text);

	public ZStrChunk(FileReader fr) : base(fr)
	{
		Text = fr.ReadBytesUntil(0x00);
	}
}