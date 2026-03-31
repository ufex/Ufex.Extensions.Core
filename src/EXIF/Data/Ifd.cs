namespace Ufex.Extensions.Core.EXIF.Data;

public enum IfdType
{
	IFD0,
	ExifIFD,
	GPSIFD,
	IFD1,
}

public class Ifd
{
	public IfdType IfdType { get; init; }
	public long Offset { get; init; }
	public List<IfdEntry> Entries { get; } = [];
	public UInt32 NextIfdOffset { get; set; }

	public IfdEntry? FindEntry(UInt16 tag)
	{
		foreach (IfdEntry entry in Entries)
		{
			if (entry.Tag == tag)
				return entry;
		}
		return null;
	}
}
