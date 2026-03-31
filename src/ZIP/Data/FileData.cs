
using System;
using System.IO;
using Ufex.API;

namespace Ufex.Extensions.Core.ZIP.Data;

public class FileData : Section
{
	private const int ChunkSize = 65536; // 64KB chunks
	public UInt32 Crc32 { get; init; }

	public UInt16 CompressionMethod { get; init; }

	public List<DeflateStreamReader.Block>? Blocks { get; private set; }

	public ulong CompressedSize { get; init; }

	public FileData(BinaryReader br, ulong size, UInt16 compressionMethod)
	{
		CompressedSize = size;
		CompressionMethod = compressionMethod;
		StartRead(br);
		
		uint crc = 0xFFFFFFFF;
		ulong remaining = size;
		byte[] buffer = new byte[ChunkSize];
		
		while(remaining > 0)
		{
			int bytesToRead = (int)Math.Min(remaining, ChunkSize);
			int bytesRead = br.Read(buffer, 0, bytesToRead);
			
			if (bytesRead == 0)
				break; // End of stream reached
			
			// Update CRC32 using Span for better performance
			Span<byte> chunk = buffer.AsSpan(0, bytesRead);
			crc = UpdateCrc32(crc, chunk);
			
			remaining -= (ulong)bytesRead;
		}
		
		Crc32 = crc ^ 0xFFFFFFFF;
		EndRead(br);
	}

	/// <summary>
	/// Reads the deflate block structure from the compressed data.
	/// Call this on demand rather than during initial parse.
	/// </summary>
	public void ReadBlocks(Stream stream)
	{
		if (CompressionMethod != 8 || CompressedSize == 0) // 8 = Deflate
			return;

		stream.Position = StartPosition;
		Blocks = DeflateStreamReader.ReadBlocks(stream);
	}

	// CRC32 lookup table (polynomial 0xEDB88320)
	private static readonly uint[] Crc32Table = GenerateCrc32Table();

	private static uint[] GenerateCrc32Table()
	{
		uint[] table = new uint[256];
		for(uint i = 0; i < 256; i++)
		{
			uint crc = i;
			for(int j = 0; j < 8; j++)
			{
				crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
			}
			table[i] = crc;
		}
		return table;
	}

	private static uint UpdateCrc32(uint crc, ReadOnlySpan<byte> data)
	{
		foreach (byte b in data)
		{
			crc = Crc32Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
		}
		return crc;
	}
}
