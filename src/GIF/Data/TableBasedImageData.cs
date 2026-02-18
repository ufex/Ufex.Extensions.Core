using Ufex.API;

namespace Ufex.Extensions.Core.GIF.Data;

/// <summary>
/// Table Based Image Data
/// Contains LZW compressed image data
/// </summary>
internal class TableBasedImageData : GifBlock
{
	/// <summary>
	/// LZW Minimum Code Size
	/// </summary>
	public byte LzwMinimumCodeSize { get; init; }

	/// <summary>
	/// Image data sub-blocks
	/// </summary>
	public List<byte[]> DataBlocks { get; init; }

	/// <summary>
	/// Total size of compressed data
	/// </summary>
	public int TotalDataSize { get; init; }

	/// <summary>
	/// Number of data sub-blocks
	/// </summary>
	public int BlockCount => DataBlocks.Count;

	public TableBasedImageData(FileReader fr) : base(fr)
	{
		LzwMinimumCodeSize = fr.ReadByte();

		// Read data sub-blocks
		DataBlocks = new List<byte[]>();
		int totalSize = 0;
		while (true)
		{
			byte blockSize = fr.ReadByte();
			if (blockSize == 0)
				break;
			var block = fr.ReadBytes(blockSize);
			DataBlocks.Add(block);
			totalSize += blockSize;
		}
		TotalDataSize = totalSize;
	}
}
