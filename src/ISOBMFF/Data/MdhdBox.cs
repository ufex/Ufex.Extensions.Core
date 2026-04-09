using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// mdhd — Media Header Box. Contains per-track media timing metadata.
/// Both QTFF and ISOBMFF use the same field layout.
/// </summary>
internal class MdhdBox : Box
{
	public UInt64 CreationTime { get; init; }
	public UInt64 ModificationTime { get; init; }
	public UInt32 Timescale { get; init; }
	public UInt64 Duration { get; init; }

	/// <summary>
	/// Packed ISO 639-2/T language code (3 x 5-bit characters).
	/// </summary>
	public UInt16 Language { get; init; }

	/// <summary>
	/// Quality indicator in QTFF; zero in ISOBMFF.
	/// </summary>
	public UInt16 PreDefined { get; init; }

	/// <summary>
	/// Decodes the packed 15-bit language code to a 3-character ISO 639-2/T string.
	/// </summary>
	public string LanguageString
	{
		get
		{
			char c1 = (char)(((Language >> 10) & 0x1F) + 0x60);
			char c2 = (char)(((Language >> 5) & 0x1F) + 0x60);
			char c3 = (char)((Language & 0x1F) + 0x60);
			return new string([c1, c2, c3]);
		}
	}

	public MdhdBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		if (Version == 1)
		{
			CreationTime = fr.ReadUInt64();
			ModificationTime = fr.ReadUInt64();
			Timescale = fr.ReadUInt32();
			Duration = fr.ReadUInt64();
		}
		else
		{
			CreationTime = fr.ReadUInt32();
			ModificationTime = fr.ReadUInt32();
			Timescale = fr.ReadUInt32();
			Duration = fr.ReadUInt32();
		}

		Language = fr.ReadUInt16();
		PreDefined = fr.ReadUInt16();
	}
}
