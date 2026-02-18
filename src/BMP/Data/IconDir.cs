using Ufex.API;

namespace Ufex.Extensions.Core.BMP.Data;

/// <summary>
/// ICONDIR - Icon directory header
/// </summary>
internal class IconDir
{
	/// <summary>
	/// File offset where this header starts
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// Reserved, must be zero
	/// </summary>
	public ushort Reserved { get; init; }

	/// <summary>
	/// Resource type (1 = icon, 2 = cursor)
	/// </summary>
	public ushort Type { get; init; }

	/// <summary>
	/// Number of images in the file
	/// </summary>
	public ushort Count { get; init; }

	public IconDir(FileReader fr)
	{
		Offset = fr.BaseStream.Position;
		Reserved = fr.ReadUInt16();
		Type = fr.ReadUInt16();
		Count = fr.ReadUInt16();
	}
}
