using System.Text;
using Ufex.API;

namespace Ufex.FileTypes.GIF.Data;

/// <summary>
/// Comment Extension (GIF89a only)
/// Contains textual comments about the image
/// </summary>
internal class CommentExtension : GifBlock
{
	/// <summary>
	/// Extension Introducer (always 0x21)
	/// </summary>
	public byte ExtensionIntroducer { get; init; }

	/// <summary>
	/// Comment Label (always 0xFE)
	/// </summary>
	public byte CommentLabel { get; init; }

	/// <summary>
	/// Comment data sub-blocks
	/// </summary>
	public List<byte[]> DataBlocks { get; init; }

	/// <summary>
	/// Get comment text
	/// </summary>
	public string CommentText
	{
		get
		{
			var sb = new StringBuilder();
			foreach (var block in DataBlocks)
			{
				sb.Append(Encoding.ASCII.GetString(block));
			}
			return sb.ToString();
		}
	}

	public CommentExtension(FileReader fr) : base(fr)
	{
		ExtensionIntroducer = fr.ReadByte();
		CommentLabel = fr.ReadByte();

		// Read data sub-blocks
		DataBlocks = new List<byte[]>();
		while (true)
		{
			byte blockSize = fr.ReadByte();
			if (blockSize == 0)
				break;
			DataBlocks.Add(fr.ReadBytes(blockSize));
		}
	}
}
