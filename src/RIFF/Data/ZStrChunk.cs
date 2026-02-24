using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

/// <summary>
/// Generic Chunk Type for null-terminated strings. 
/// The chunk data is read as a byte array until a null terminator (0x00) is encountered, 
/// and can be accessed as a string via the TextString property.
/// </summary>
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