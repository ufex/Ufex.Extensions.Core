using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// hdlr — Handler Reference Box. Identifies the media handler type.
/// In QTFF, hdlr can also appear inside minf as a data handler.
/// In ISOBMFF, hdlr only appears inside mdia and pre_defined is always zero.
/// </summary>
internal class HdlrBox : Box
{
	/// <summary>
	/// ISOBMFF: always zero. QTFF: component type ('mhlr' or 'dhlr').
	/// </summary>
	public UInt32 PreDefined { get; init; }

	/// <summary>
	/// Media handler type FourCC (e.g. "vide", "soun", "subt").
	/// </summary>
	public Byte[] HandlerType { get; init; }   // 4 bytes

	/// <summary>
	/// Reserved (3 x UInt32), should be zero.
	/// </summary>
	public UInt32[] Reserved { get; init; }   // 3 x 4 bytes

	/// <summary>
	/// Human-readable name of the handler, null-terminated UTF-8.
	/// </summary>
	public Byte[] Name { get; init; }

	public string HandlerTypeString => System.Text.Encoding.ASCII.GetString(HandlerType);

	public string PreDefinedString
	{
		get
		{
			if (PreDefined == 0) return "";
			Byte[] bytes = BitConverter.GetBytes(PreDefined);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);
			return System.Text.Encoding.ASCII.GetString(bytes);
		}
	}

	public string NameString => System.Text.Encoding.UTF8.GetString(Name).TrimEnd('\0');

	public HdlrBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		PreDefined = fr.ReadUInt32();
		HandlerType = fr.ReadBytes(4);
		Reserved = [ fr.ReadUInt32(), fr.ReadUInt32(), fr.ReadUInt32() ];

		// Read remaining bytes as the name string
		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Int64 remaining = payloadEnd - fr.BaseStream.Position;
		if (remaining > 0)
			Name = fr.ReadBytes((int)remaining);
		else
			Name = [];
	}
}
