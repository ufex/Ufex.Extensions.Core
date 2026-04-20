using Ufex.API;

namespace Ufex.Extensions.Core.EXIF.Data;

public class ExifData
{
	public required TiffHeader TiffHeader { get; init; }
	public required Ifd Ifd0 { get; init; }
	public Ifd? ExifIfd { get; init; }
	public Ifd? GpsIfd { get; init; }
	public Ifd? Ifd1 { get; init; }
	public Endian ByteOrder => TiffHeader.ByteOrder;

	/// <summary>
	/// Absolute file offset of the embedded JPEG thumbnail (from IFD1 tag 0x0201), or null if not present.
	/// </summary>
	public long? ThumbnailOffset { get; set; }

	/// <summary>
	/// Length of the embedded JPEG thumbnail data (from IFD1 tag 0x0202), or null if not present.
	/// </summary>
	public long? ThumbnailLength { get; set; }

	public Ifd? GetIfd(IfdType ifdType)
	{
		return ifdType switch
		{
			IfdType.IFD0 => Ifd0,
			IfdType.ExifIFD => ExifIfd,
			IfdType.GPSIFD => GpsIfd,
			IfdType.IFD1 => Ifd1,
			_ => null,
		};
	}

	public IfdEntry? GetEntry(IfdType ifdType, UInt16 tag)
	{
		Ifd? ifd = GetIfd(ifdType);
		return ifd?.FindEntry(tag);
	}
}
