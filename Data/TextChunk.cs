using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// tEXt - Textual data
/// </summary>
internal class TextChunk : Chunk
{
	public byte[] Keyword { get; init; }
	public byte? NullSeparator { get; init; }
	public byte[] TextString { get; init; }

	public string KeywordString => System.Text.Encoding.Latin1.GetString(Keyword);
	public string TextStringString => System.Text.Encoding.Latin1.GetString(TextString);

	public TextChunk(FileReader fr) : base(fr)
	{
		byte[] chunkData = fr.ReadBytes((int)Length);

		// Find the null separator
		int nullPos = Array.IndexOf(chunkData, (byte)0);
		if (nullPos < 0)
		{
			NullSeparator = null;
			nullPos = chunkData.Length;
		}
		else
		{
			NullSeparator = chunkData[nullPos];
		}

		// Keyword is everything before the null
		Keyword = chunkData[0..nullPos];

		// Text string is everything after the null
		if (nullPos + 1 < chunkData.Length)
		{
			TextString = chunkData[(nullPos + 1)..];
		}
		else
		{
			TextString = Array.Empty<byte>();
		}
	}
}
