using System;
using System.Collections.Generic;
using System.IO;
using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

internal class Chunk
{
	private static readonly Dictionary<string, Type> CHUNK_TYPES = new()
	{
		{ "IHDR", typeof(IhdrChunk) },
		{ "IDAT", typeof(IdatChunk) },
		{ "IEND", typeof(IendChunk) },
		{ "PLTE", typeof(PlteChunk) },
		{ "bKGD", typeof(BkgdChunk) },
		{ "gAMA", typeof(GamaChunk) },
		{ "cHRM", typeof(ChrmChunk) },
		{ "iCCP", typeof(IccpChunk) },
		{ "tIME", typeof(TimeChunk) },
		{ "tRNS", typeof(TrnsChunk) },
		{ "hIST", typeof(HistChunk) },
		{ "sPLT", typeof(SpltChunk) },
		{ "sRGB", typeof(SrgbChunk) },
		{ "sBIT", typeof(SbitChunk) },
		{ "pHYs", typeof(PhysChunk) },
		{ "tEXt", typeof(TextChunk) },
		{ "zTXt", typeof(ZtxtChunk) },
		{ "iTXt", typeof(ItxtChunk) },
	};

	public UInt32 Length { get; init; }
	public Byte[] ChunkType { get; init; }   // 4 bytes
	public CRC CRC = new CRC();

	public string ChunkTypeString
	{
		get { return System.Text.Encoding.ASCII.GetString(ChunkType); }
	}

	public long Offset { get; init; }

	public Chunk(FileReader fr)
	{
		Offset = fr.BaseStream.Position;
		Length = fr.ReadUInt32();
		ChunkType = fr.ReadBytes(4);
	}

	public static Chunk? CreateChunk(string chunkType, FileReader fr, int colorType)
	{
		if (chunkType == "tRNS")
			return new TrnsChunk(fr, colorType);
		if (chunkType == "bKGD")
			return new BkgdChunk(fr, colorType);

		if (!CHUNK_TYPES.TryGetValue(chunkType, out var chunkClass))
			return null;

		return (Chunk?)Activator.CreateInstance(chunkClass, fr);
	}
}