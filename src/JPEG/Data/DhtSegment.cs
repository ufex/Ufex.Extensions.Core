using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// DHT - Define Huffman Table marker segment (0xFFC4)
/// Contains one or more Huffman tables used for entropy coding.
/// Each table has a class (DC/AC), an ID, code counts per bit length,
/// and the actual symbol values.
/// </summary>
internal class DhtSegment : Segment
{
	/// <summary>
	/// Number of Huffman tables in this segment
	/// </summary>
	public int TableCount { get; init; }

	/// <summary>
	/// Table class and ID packed byte for each table:
	/// High nibble = class (0 = DC, 1 = AC)
	/// Low nibble = table ID (0-3)
	/// </summary>
	public byte[] TableInfos { get; init; }

	/// <summary>
	/// Number of codes for each bit length (1-16), one 16-byte array per table
	/// </summary>
	public byte[][] CodeCounts { get; init; }

	/// <summary>
	/// Huffman symbol values for each table
	/// </summary>
	public byte[][] SymbolValues { get; init; }

	public DhtSegment(FileReader fr) : base(fr)
	{
		var tableInfoList = new List<byte>();
		var codeCountsList = new List<byte[]>();
		var symbolValuesList = new List<byte[]>();

		int bytesRemaining = Length - 2; // Subtract length field itself
		while (bytesRemaining > 0)
		{
			byte tableInfo = fr.ReadByte();
			bytesRemaining--;

			tableInfoList.Add(tableInfo);

			// Read 16 bytes: number of codes for each bit length (1-16)
			byte[] counts = fr.ReadBytes(16);
			bytesRemaining -= 16;

			codeCountsList.Add(counts);

			// Total number of symbol values
			int totalSymbols = 0;
			for (int i = 0; i < 16; i++)
				totalSymbols += counts[i];

			byte[] symbols = fr.ReadBytes(totalSymbols);
			bytesRemaining -= totalSymbols;

			symbolValuesList.Add(symbols);
		}

		TableCount = tableInfoList.Count;
		TableInfos = tableInfoList.ToArray();
		CodeCounts = codeCountsList.ToArray();
		SymbolValues = symbolValuesList.ToArray();
	}

	/// <summary>
	/// Gets the table class (0 = DC, 1 = AC) for a table
	/// </summary>
	public int GetTableClass(int tableIndex) => (TableInfos[tableIndex] >> 4) & 0x0F;

	/// <summary>
	/// Gets the table destination ID (0-3) for a table
	/// </summary>
	public int GetTableId(int tableIndex) => TableInfos[tableIndex] & 0x0F;

	/// <summary>
	/// Gets the total number of symbols in a table
	/// </summary>
	public int GetSymbolCount(int tableIndex)
	{
		int count = 0;
		for (int i = 0; i < 16; i++)
			count += CodeCounts[tableIndex][i];
		return count;
	}
}
