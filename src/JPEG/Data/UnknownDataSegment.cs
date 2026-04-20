namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// Represents a region of unknown or unrecognized data in the JPEG file.
/// This is used when data is found after an EOI marker or when a segment
/// cannot be parsed as a valid JPEG marker.
/// </summary>
internal class UnknownDataSegment
{
	public long Offset { get; init; }
	public long DataLength { get; init; }

	public long TotalSize => DataLength;

	public UnknownDataSegment(long offset, long dataLength)
	{
		Offset = offset;
		DataLength = dataLength;
	}
}
