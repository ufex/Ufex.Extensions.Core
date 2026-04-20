using System;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// colr — Colour Information property. Declares the colour space of an image item,
/// either as an ICC profile or as parametric nclx colour primaries.
/// </summary>
internal class ColrBox : Box
{
	/// <summary>
	/// Colour type FourCC: 'rICC', 'prof' (ICC profile), or 'nclx' (parametric).
	/// </summary>
	public Byte[] ColourType { get; init; }

	// nclx fields (only valid when ColourTypeString is "nclx")
	public UInt16 ColourPrimaries { get; init; }
	public UInt16 TransferCharacteristics { get; init; }
	public UInt16 MatrixCoefficients { get; init; }
	public bool FullRangeFlag { get; init; }

	/// <summary>
	/// Raw ICC profile bytes (only valid when ColourTypeString is "rICC" or "prof").
	/// </summary>
	public Int32 IccProfileLength { get; init; }

	public string ColourTypeString => Encoding.ASCII.GetString(ColourType).TrimEnd('\0');

	public static readonly Dictionary<UInt16, string> ColourPrimariesNames = new()
	{
		{ 1, "BT.709 / sRGB" },
		{ 4, "BT.470M" },
		{ 5, "BT.601 PAL" },
		{ 6, "BT.601 NTSC" },
		{ 8, "Generic Film" },
		{ 9, "BT.2020" },
		{ 10, "SMPTE ST 428 (XYZ)" },
		{ 11, "SMPTE RP 431 (DCI P3)" },
		{ 12, "Display P3" },
		{ 22, "EBU Tech 3213" },
	};

	public static readonly Dictionary<UInt16, string> TransferNames = new()
	{
		{ 1, "BT.709" },
		{ 4, "BT.470M (Gamma 2.2)" },
		{ 5, "BT.470BG (Gamma 2.8)" },
		{ 6, "BT.601" },
		{ 8, "Linear" },
		{ 11, "IEC 61966-2-4 (xvYCC)" },
		{ 13, "sRGB" },
		{ 14, "BT.2020 (10-bit)" },
		{ 15, "BT.2020 (12-bit)" },
		{ 16, "ST.2084 (PQ / HDR10)" },
		{ 17, "ST.428 (Cinema)" },
		{ 18, "HLG (Hybrid Log-Gamma)" },
	};

	public static readonly Dictionary<UInt16, string> MatrixNames = new()
	{
		{ 0, "Identity (RGB/XYZ)" },
		{ 1, "BT.709" },
		{ 5, "BT.601 (PAL)" },
		{ 6, "BT.601 (NTSC)" },
		{ 8, "YCgCo" },
		{ 9, "BT.2020 (non-constant)" },
		{ 10, "BT.2020 (constant)" },
		{ 11, "SMPTE ST 2085 (Y'D'zD'x)" },
		{ 14, "ICTCP" },
	};

	public ColrBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, false)
	{
		Int64 payloadEnd = Offset + (Int64)ActualSize;

		ColourType = fr.ReadBytes(4);
		string colourTypeStr = ColourTypeString;

		if (colourTypeStr == "nclx")
		{
			ColourPrimaries = fr.ReadUInt16();
			TransferCharacteristics = fr.ReadUInt16();
			MatrixCoefficients = fr.ReadUInt16();
			if (fr.BaseStream.Position < payloadEnd)
			{
				Byte flags = fr.ReadByte();
				FullRangeFlag = (flags & 0x80) != 0;
			}
		}
		else if (colourTypeStr == "rICC" || colourTypeStr == "prof")
		{
			Int64 remaining = payloadEnd - fr.BaseStream.Position;
			IccProfileLength = (Int32)Math.Max(0, remaining);
			// Skip the ICC profile data (we record the length but don't load the full profile)
			if (remaining > 0)
				fr.BaseStream.Seek(payloadEnd, System.IO.SeekOrigin.Begin);
		}
	}
}
