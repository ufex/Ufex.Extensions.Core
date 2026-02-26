using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// Base class for all JPEG marker segments.
/// A JPEG marker consists of 0xFF followed by a marker type byte.
/// Most markers also have a 2-byte length field and data payload.
/// Standalone markers (SOI, EOI, RSTn) have no length or data.
/// </summary>
internal class Segment
{
	/// <summary>
	/// File offset where this segment starts
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// Marker prefix byte (always 0xFF)
	/// </summary>
	public byte Marker { get; init; }

	/// <summary>
	/// Marker type byte (e.g. 0xD8 for SOI, 0xE0 for APP0)
	/// </summary>
	public byte MarkerType { get; init; }

	/// <summary>
	/// Length of the segment data including the 2-byte length field itself,
	/// but excluding the 2-byte marker. Zero for standalone markers.
	/// </summary>
	public ushort Length { get; init; }

	/// <summary>
	/// Human-readable marker name (e.g. "SOI", "APP0", "SOF0")
	/// </summary>
	public string MarkerName => Constants.GetMarkerName(MarkerType);

	/// <summary>
	/// Total size of this segment in the file (marker bytes + length field + data)
	/// </summary>
	public long TotalSize => Length > 0 ? 2 + Length : 2;

	/// <summary>
	/// Constructor for markers with a length field (most markers)
	/// </summary>
	public Segment(FileReader fr)
	{
		Offset = fr.BaseStream.Position;
		Marker = fr.ReadByte();
		MarkerType = fr.ReadByte();
		Length = fr.ReadUInt16();
	}

	/// <summary>
	/// Constructor for standalone markers (SOI, EOI, RSTn) which have no length field
	/// </summary>
	protected Segment(FileReader fr, bool standalone)
	{
		Offset = fr.BaseStream.Position;
		Marker = fr.ReadByte();
		MarkerType = fr.ReadByte();
		Length = 0;
	}

	/// <summary>
	/// Factory method to create the appropriate Segment subclass based on marker type
	/// </summary>
	public static Segment? CreateSegment(byte markerType, FileReader fr)
	{
		return markerType switch
		{
			Signatures.SOI => new SoiSegment(fr),
			Signatures.EOI => new EoiSegment(fr),
			Signatures.APP0 => CreateApp0Segment(fr),
			>= Signatures.APP1 and <= Signatures.APP15 => new AppNSegment(fr),
			Signatures.SOF0 or Signatures.SOF1 or Signatures.SOF2 or Signatures.SOF3 => new SofSegment(fr),
			Signatures.DHT => new DhtSegment(fr),
			Signatures.DQT => new DqtSegment(fr),
			Signatures.DRI => new DriSegment(fr),
			Signatures.SOS => new SosSegment(fr),
			Signatures.COM => new ComSegment(fr),
			_ => null,
		};
	}

	/// <summary>
	/// Creates the appropriate APP0 subclass by peeking at the identifier string
	/// </summary>
	private static Segment CreateApp0Segment(FileReader fr)
	{
		long startPos = fr.BaseStream.Position;

		// Read marker and length to peek at identifier
		fr.ReadByte(); // 0xFF
		fr.ReadByte(); // 0xE0
		fr.ReadUInt16(); // length
		byte[] identifier = fr.ReadBytes(5);

		// Rewind to start
		fr.BaseStream.Seek(startPos, SeekOrigin.Begin);

		if (identifier.SequenceEqual(Signatures.JfifIdentifier))
			return new App0JfifSegment(fr);

		if (identifier.SequenceEqual(Signatures.JfxxIdentifier))
			return new App0JfxxSegment(fr);

		// Unknown APP0 - treat as generic APPn
		return new AppNSegment(fr);
	}
}
