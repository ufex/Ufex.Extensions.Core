using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// Base class for ISOBMFF/QTFF boxes (atoms). A box consists of a 4-byte size field,
/// a 4-byte type (FourCC), and the box payload. When size == 1, an 8-byte extended
/// size follows the type field. When size == 0, the box extends to EOF.
/// Both QTFF atoms and ISOBMFF boxes share this identical header layout.
/// </summary>
internal class Box
{
	/// <summary>
	/// Container boxes that hold only child boxes (no payload of their own).
	/// Both QTFF and ISOBMFF share these container types.
	/// </summary>
	private static readonly HashSet<string> CONTAINER_TYPES = new()
	{
		"moov", "trak", "mdia", "minf", "stbl", "dinf", "edts", "udta",
		"meta", "mvex", "moof", "traf", "mfra", "clip", "matt", "iprp",
		"sinf", "schi", "rinf", "ilst", "ipco", "grpl"
	};

	/// <summary>
	/// FullBox types that have a version (1 byte) and flags (3 bytes) prefix
	/// before their payload. QTFF uses the same binary layout informally;
	/// ISOBMFF formalises it as FullBox.
	/// </summary>
	private static readonly HashSet<string> FULLBOX_TYPES = new()
	{
		"mvhd", "tkhd", "mdhd", "hdlr", "vmhd", "smhd", "hmhd", "nmhd",
		"sthd", "dref", "stsd", "stts", "ctts", "cslg", "stss", "stsc",
		"stsz", "stz2", "stco", "co64", "sgpd", "sbgp", "sdtp", "subs",
		"mehd", "trex", "mfhd", "tfhd", "tfdt", "trun", "tfra", "mfro",
		"elst", "pdin", "iinf", "infe", "iloc", "iref",
		"pitm", "url ", "urn ", "meta",
		// 3GPP metadata boxes
		"titl", "dscp", "cprt", "perf", "auth", "gnre",
		"rtng", "clsf", "kywd", "loci", "albm", "yrrc",
		// HEIF boxes
		"ispe", "pixi", "auxC", "ipma"
	};

	/// <summary>
	/// Specialized box types mapped to their concrete classes.
	/// </summary>
	private static readonly Dictionary<string, Type> BOX_TYPES = new()
	{
		{ "ftyp", typeof(FtypBox) },
		{ "mvhd", typeof(MvhdBox) },
		{ "tkhd", typeof(TkhdBox) },
		{ "mdhd", typeof(MdhdBox) },
		{ "hdlr", typeof(HdlrBox) },
		{ "vmhd", typeof(VmhdBox) },
		{ "smhd", typeof(SmhdBox) },
		{ "elst", typeof(ElstBox) },
		{ "stsd", typeof(StsdBox) },
		{ "stts", typeof(SttsBox) },
		{ "stsc", typeof(StscBox) },
		{ "stsz", typeof(StszBox) },
		{ "stco", typeof(StcoBox) },
		{ "co64", typeof(Co64Box) },
		{ "stss", typeof(StssBox) },
		{ "ctts", typeof(CttsBox) },
		// 3GPP metadata boxes
		{ "titl", typeof(ThreeGpp.TextMetadataBox) },
		{ "dscp", typeof(ThreeGpp.TextMetadataBox) },
		{ "cprt", typeof(ThreeGpp.TextMetadataBox) },
		{ "perf", typeof(ThreeGpp.TextMetadataBox) },
		{ "auth", typeof(ThreeGpp.TextMetadataBox) },
		{ "gnre", typeof(ThreeGpp.TextMetadataBox) },
		{ "rtng", typeof(ThreeGpp.RtngBox) },
		{ "clsf", typeof(ThreeGpp.ClsfBox) },
		{ "kywd", typeof(ThreeGpp.KywdBox) },
		{ "loci", typeof(ThreeGpp.LociBox) },
		{ "albm", typeof(ThreeGpp.AlbmBox) },
		{ "yrrc", typeof(ThreeGpp.YrrcBox) },
		// HEIF boxes
		{ "pitm", typeof(Heif.PitmBox) },
		{ "iloc", typeof(Heif.IlocBox) },
		{ "iinf", typeof(Heif.IinfBox) },
		{ "infe", typeof(Heif.InfeBox) },
		{ "ispe", typeof(Heif.IspeBox) },
		{ "pixi", typeof(Heif.PixiBox) },
		{ "colr", typeof(Heif.ColrBox) },
		{ "irot", typeof(Heif.IrotBox) },
		{ "imir", typeof(Heif.ImirBox) },
		{ "clap", typeof(Heif.ClapBox) },
		{ "auxC", typeof(Heif.AuxcBox) },
		{ "ipma", typeof(Heif.IpmaBox) },
		{ "pasp", typeof(Heif.PaspBox) },
		{ "clli", typeof(Heif.ClliBox) },
		{ "mdcv", typeof(Heif.MdcvBox) },
	};

	/// <summary>
	/// 4-byte size field. If 1, the actual size is in ExtendedSize.
	/// If 0, the box extends to the end of the file.
	/// </summary>
	public UInt32 Size { get; init; }

	/// <summary>
	/// 4-character code (FourCC) that identifies the box type.
	/// </summary>
	public Byte[] Type { get; init; }   // 4 bytes

	/// <summary>
	/// 64-bit extended size. Present only when Size == 1.
	/// </summary>
	public UInt64 ExtendedSize { get; init; }

	/// <summary>
	/// The offset in the file where this box starts (beginning of size field).
	/// </summary>
	public Int64 Offset { get; init; }

	/// <summary>
	/// Child boxes, if this is a container box.
	/// </summary>
	public List<Box> Children { get; init; } = [];

	/// <summary>
	/// Version field for FullBox types (1 byte). Null for non-FullBox types.
	/// </summary>
	public Byte? Version { get; init; }

	/// <summary>
	/// Flags field for FullBox types (3 bytes stored as UInt32). Null for non-FullBox types.
	/// </summary>
	public UInt32? Flags { get; init; }

	/// <summary>
	/// The type as a readable ASCII string.
	/// </summary>
	public string TypeString => Encoding.ASCII.GetString(Type);

	/// <summary>
	/// The actual total size of the box in bytes, accounting for extended size and size == 0.
	/// </summary>
	public UInt64 ActualSize { get; init; }

	/// <summary>
	/// The size of the box header in bytes (8, 16, or variable depending on extended size).
	/// </summary>
	public Int32 HeaderSize { get; init; }

	public Box(FileReader fr, Int64 boxEndBoundary)
	{
		Offset = fr.BaseStream.Position;
		Size = fr.ReadUInt32();
		Type = fr.ReadBytes(4);
		HeaderSize = 8;

		if (Size == 1)
		{
			ExtendedSize = fr.ReadUInt64();
			ActualSize = ExtendedSize;
			HeaderSize = 16;
		}
		else if (Size == 0)
		{
			// Box extends to end of boundary (typically EOF)
			ActualSize = (UInt64)(boxEndBoundary - Offset);
		}
		else
		{
			ActualSize = Size;
		}

		string typeStr = TypeString;

		// Read FullBox version/flags if applicable
		if (FULLBOX_TYPES.Contains(typeStr))
		{
			Version = fr.ReadByte();
			Byte[] flagBytes = fr.ReadBytes(3);
			Flags = (UInt32)((flagBytes[0] << 16) | (flagBytes[1] << 8) | flagBytes[2]);
			HeaderSize += 4;
		}

		// Read children if this is a container box
		if (CONTAINER_TYPES.Contains(typeStr))
		{
			Int64 payloadEnd = Offset + (Int64)ActualSize;
			// meta is both FullBox and container in ISOBMFF
			Children = ReadSubBoxes(fr, payloadEnd);
		}
	}

	/// <summary>
	/// Protected constructor for specialized box subclasses.
	/// Reads the common header fields (size, type, extended size, FullBox version/flags).
	/// Subclasses should read their specific payload after calling this.
	/// </summary>
	protected Box(FileReader fr, Int64 boxEndBoundary, bool readFullBoxHeader)
	{
		Offset = fr.BaseStream.Position;
		Size = fr.ReadUInt32();
		Type = fr.ReadBytes(4);
		HeaderSize = 8;

		if (Size == 1)
		{
			ExtendedSize = fr.ReadUInt64();
			ActualSize = ExtendedSize;
			HeaderSize = 16;
		}
		else if (Size == 0)
		{
			ActualSize = (UInt64)(boxEndBoundary - Offset);
		}
		else
		{
			ActualSize = Size;
		}

		if (readFullBoxHeader)
		{
			Version = fr.ReadByte();
			Byte[] flagBytes = fr.ReadBytes(3);
			Flags = (UInt32)((flagBytes[0] << 16) | (flagBytes[1] << 8) | flagBytes[2]);
			HeaderSize += 4;
		}
	}

	/// <summary>
	/// Read child boxes from the current stream position up to the given end boundary.
	/// </summary>
	public static List<Box> ReadSubBoxes(FileReader fr, Int64 endBoundary)
	{
		var children = new List<Box>();

		while (fr.BaseStream.Position + 8 <= endBoundary)
		{
			Int64 boxStart = fr.BaseStream.Position;

			// Peek at size and type to determine which class to instantiate
			UInt32 peekSize = fr.ReadUInt32();
			Byte[] peekType = fr.ReadBytes(4);
			string typeStr = Encoding.ASCII.GetString(peekType);

			// Rewind to let the constructor read from the start
			fr.BaseStream.Seek(boxStart, SeekOrigin.Begin);

			// Determine actual box size for boundary enforcement
			UInt64 actualSize;
			if (peekSize == 1)
			{
				// Need to peek at extended size
				fr.BaseStream.Seek(boxStart + 8, SeekOrigin.Begin);
				actualSize = fr.ReadUInt64();
				fr.BaseStream.Seek(boxStart, SeekOrigin.Begin);
			}
			else if (peekSize == 0)
			{
				actualSize = (UInt64)(endBoundary - boxStart);
			}
			else
			{
				actualSize = peekSize;
			}

			Int64 boxEnd = boxStart + (Int64)actualSize;
			boxEnd = Math.Min(boxEnd, endBoundary);

			Box box = CreateBox(typeStr, fr, endBoundary);
			children.Add(box);

			// Ensure we're positioned at the end of this box
			if (fr.BaseStream.Position != boxEnd)
			{
				fr.BaseStream.Seek(boxEnd, SeekOrigin.Begin);
			}
		}

		return children;
	}

	/// <summary>
	/// Factory method to create the appropriate Box subclass based on type.
	/// </summary>
	public static Box CreateBox(string typeStr, FileReader fr, Int64 boxEndBoundary)
	{
		if (BOX_TYPES.TryGetValue(typeStr, out var boxClass))
		{
			return (Box)(Activator.CreateInstance(boxClass, fr, boxEndBoundary)
				?? throw new InvalidOperationException($"Failed to create box of type {typeStr}"));
		}

		return new Box(fr, boxEndBoundary);
	}
}
