using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// ftyp — File Type Box. Declares the file's brand compatibility.
/// Present in ISOBMFF files (mandatory) and optionally in newer QTFF .mov files.
/// </summary>
internal class FtypBox : Box
{
	/// <summary>
	/// Primary format brand (e.g. "isom", "mp41", "qt  ").
	/// </summary>
	public Byte[] MajorBrand { get; init; }   // 4 bytes

	/// <summary>
	/// Informational version of the major brand.
	/// </summary>
	public UInt32 MinorVersion { get; init; }

	/// <summary>
	/// Array of compatible brand FourCCs.
	/// </summary>
	public Byte[][] CompatibleBrands { get; init; }

	public string MajorBrandString => System.Text.Encoding.ASCII.GetString(MajorBrand);

	public FtypBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, false)
	{
		MajorBrand = fr.ReadBytes(4);
		MinorVersion = fr.ReadUInt32();

		// Read remaining bytes as compatible brands (4 bytes each)
		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Int64 remaining = payloadEnd - fr.BaseStream.Position;
		int brandCount = (int)(remaining / 4);
		CompatibleBrands = new Byte[brandCount][];
		for (int i = 0; i < brandCount; i++)
		{
			CompatibleBrands[i] = fr.ReadBytes(4);
		}
	}
}
