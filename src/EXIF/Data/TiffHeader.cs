using Ufex.API;

namespace Ufex.Extensions.Core.EXIF.Data;

public class TiffHeader
{
	public long Offset { get; init; }
	public Endian ByteOrder { get; init; }
	public UInt16 Magic { get; init; }
	public UInt32 Ifd0Offset { get; init; }

	public bool IsValid => Magic == 42;
}
