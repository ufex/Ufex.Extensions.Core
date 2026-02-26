using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// APP0 JFIF marker segment (0xFFE0 with "JFIF\0" identifier)
/// Mandatory in JFIF files, must appear immediately after SOI.
/// Contains version, density, and optional thumbnail information.
/// </summary>
internal class App0JfifSegment : Segment
{
	/// <summary>
	/// Identifier string bytes: "JFIF\0" (5 bytes)
	/// </summary>
	public byte[] Identifier { get; init; }

	/// <summary>
	/// Major version number (typically 1)
	/// </summary>
	public byte VersionMajor { get; init; }

	/// <summary>
	/// Minor version number (typically 0, 1, or 2)
	/// </summary>
	public byte VersionMinor { get; init; }

	/// <summary>
	/// Density units: 0 = no units (aspect ratio), 1 = dots/inch, 2 = dots/cm
	/// </summary>
	public byte Units { get; init; }

	/// <summary>
	/// Horizontal pixel density
	/// </summary>
	public ushort Xdensity { get; init; }

	/// <summary>
	/// Vertical pixel density
	/// </summary>
	public ushort Ydensity { get; init; }

	/// <summary>
	/// Thumbnail horizontal pixel count
	/// </summary>
	public byte Xthumbnail { get; init; }

	/// <summary>
	/// Thumbnail vertical pixel count
	/// </summary>
	public byte Ythumbnail { get; init; }

	/// <summary>
	/// Packed 24-bit RGB thumbnail data (3 * Xthumbnail * Ythumbnail bytes)
	/// </summary>
	public byte[] ThumbnailData { get; init; }

	/// <summary>
	/// Version string (e.g. "1.02")
	/// </summary>
	public string VersionString => $"{VersionMajor}.{VersionMinor:D2}";

	public App0JfifSegment(FileReader fr) : base(fr)
	{
		Identifier = fr.ReadBytes(5);
		VersionMajor = fr.ReadByte();
		VersionMinor = fr.ReadByte();
		Units = fr.ReadByte();
		Xdensity = fr.ReadUInt16();
		Ydensity = fr.ReadUInt16();
		Xthumbnail = fr.ReadByte();
		Ythumbnail = fr.ReadByte();

		int thumbnailSize = 3 * Xthumbnail * Ythumbnail;
		ThumbnailData = thumbnailSize > 0 ? fr.ReadBytes(thumbnailSize) : [];
	}
}
