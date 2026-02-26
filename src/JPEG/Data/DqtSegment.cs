using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// DQT - Define Quantization Table marker segment (0xFFDB)
/// Contains one or more quantization tables used in the DCT compression process.
/// Each table has a precision (8 or 16 bit) and 64 values.
/// </summary>
internal class DqtSegment : Segment
{
	/// <summary>
	/// Number of quantization tables in this segment
	/// </summary>
	public int TableCount { get; init; }

	/// <summary>
	/// Table precision and ID packed byte for each table:
	/// High nibble = precision (0 = 8-bit, 1 = 16-bit)
	/// Low nibble = table ID (0-3)
	/// </summary>
	public byte[] TableInfos { get; init; }

	/// <summary>
	/// Quantization table values (64 values per table, stored as bytes or ushorts)
	/// </summary>
	public byte[][] TableData { get; init; }

	public DqtSegment(FileReader fr) : base(fr)
	{
		var tableInfoList = new List<byte>();
		var tableDataList = new List<byte[]>();

		int bytesRemaining = Length - 2; // Subtract length field itself
		while (bytesRemaining > 0)
		{
			byte tableInfo = fr.ReadByte();
			bytesRemaining--;

			tableInfoList.Add(tableInfo);

			int precision = (tableInfo >> 4) & 0x0F;
			int tableSize = precision == 0 ? 64 : 128; // 8-bit: 64 bytes, 16-bit: 128 bytes

			byte[] data = fr.ReadBytes(tableSize);
			bytesRemaining -= tableSize;

			tableDataList.Add(data);
		}

		TableCount = tableInfoList.Count;
		TableInfos = tableInfoList.ToArray();
		TableData = tableDataList.ToArray();
	}

	/// <summary>
	/// Gets the precision (0 = 8-bit, 1 = 16-bit) for a table
	/// </summary>
	public int GetPrecision(int tableIndex) => (TableInfos[tableIndex] >> 4) & 0x0F;

	/// <summary>
	/// Gets the table destination ID (0-3) for a table
	/// </summary>
	public int GetTableId(int tableIndex) => TableInfos[tableIndex] & 0x0F;
}
