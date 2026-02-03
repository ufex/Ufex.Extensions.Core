using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// tEXt - Textual data
/// </summary>
internal class TextChunk : Chunk
{
	public string Keyword { get; init; }
	public string TextString { get; init; }

	public TextChunk(FileReader fr) : base(fr)
	{
		byte[] chunkData = fr.ReadBytes((int)Length);

		// Find the null separator
		int nullPos = Array.IndexOf(chunkData, (byte)0);
		if (nullPos < 0)
		{
			nullPos = chunkData.Length;
		}

		// Keyword is everything before the null
		Keyword = System.Text.Encoding.Latin1.GetString(chunkData, 0, nullPos);

		// Text string is everything after the null
		if (nullPos + 1 < chunkData.Length)
		{
			TextString = System.Text.Encoding.Latin1.GetString(chunkData, nullPos + 1, chunkData.Length - nullPos - 1);
		}
		else
		{
			TextString = string.Empty;
		}
	}
}
