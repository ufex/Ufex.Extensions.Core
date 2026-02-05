using System.Collections.Frozen;
using Ufex.API;

namespace Ufex.FileTypes.GZIP;

internal class Member
{
	public static FrozenDictionary<byte, string> CompressionMethods { get; } = new Dictionary<byte, string>
	{
		{ 0x00, "reserved" },
		{ 0x01, "reserved" },
		{ 0x02, "reserved" },
		{ 0x03, "reserved" },
		{ 0x04, "reserved" },
		{ 0x05, "reserved" },
		{ 0x06, "reserved" },
		{ 0x07, "reserved" },
		{ 0x08, "deflate" },
	}.ToFrozenDictionary();

	public long Offset { get; init; }

	public byte ID1 { get; init; }
	public byte ID2 { get; init; }
	public byte CompressionMethod { get; init; }
	public byte Flags { get; init; }
	public uint ModTime { get; init; }
	public byte ExtraFlags { get; init; }
	public byte OperatingSystem { get; init; }
	public ushort ExtraLength { get; init; }
	public byte[]? ExtraBytes { get; init; }
	public byte[]? FileName { get; init; }
	public byte[]? FileComment { get; init; }
	public ushort Crc16 { get; init; }

	public string? FileNameString { get { return FileName != null ? System.Text.Encoding.Latin1.GetString(FileName) : null; } }
	public string? FileCommentString { get { return FileComment != null ? System.Text.Encoding.Latin1.GetString(FileComment) : null; } }

	public Member(FileReader fr)
	{
		Offset = fr.BaseStream.Position;
		ExtraBytes = null;
		FileName = null;
		FileComment = null;

		ID1 = fr.ReadByte();
		ID2 = fr.ReadByte();
		CompressionMethod = fr.ReadByte();
		Flags = fr.ReadByte();
		ModTime = fr.ReadUInt32();
		ExtraFlags = fr.ReadByte();
		OperatingSystem = fr.ReadByte();

		// Read the FEXTRA
		if(ByteUtil.GetBit(Flags, 2))
		{
			ExtraLength = fr.ReadUInt16();
			ExtraBytes = fr.ReadBytes(ExtraLength);
		}

		// Read the FNAME
		if(ByteUtil.GetBit(Flags, 3))
			FileName = fr.ReadBytesUntil(0x00, true);

		// Read the FCOMMENT
		if(ByteUtil.GetBit(Flags, 4))
			FileComment = fr.ReadBytesUntil(0x00, true);

		// Read the FHCRC
		if(ByteUtil.GetBit(Flags, 1))
			Crc16 = fr.ReadUInt16();
	}

}
