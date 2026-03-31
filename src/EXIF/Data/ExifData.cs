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
