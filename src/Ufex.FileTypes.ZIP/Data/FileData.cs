
using System;
using System.IO;
using Ufex.API;
using Ufex.API.Validation;

namespace Ufex.FileTypes.ZIP.Data;

internal class FileData : Section
{
	private const int ChunkSize = 65536; // 64KB chunks
	public UInt32 Crc32 { get; init; }

	public List<DeflateStreamReader.Block>? Blocks { get; init; }

	public ulong CompressedSize { get; init; }

	public FileData(BinaryReader br, ulong size, UInt16 compressionMethod, ValidationReport validationReport)
	{
		CompressedSize = size;
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
		
		if(compressionMethod == (ushort)CompressionMethod.Deflate && size > 0) // Deflate
		{
			// Reset stream position to the start of file data and read the blocks from the deflate stream
			br.BaseStream.Position = StartPosition;
			try 
			{
				Blocks = DeflateStreamReader.ReadBlocks(br.BaseStream);
			}
			catch(Exception ex)
			{
				validationReport.Error("Error reading Deflate blocks: " + ex.Message);
				Blocks = new List<DeflateStreamReader.Block>();
			}
			br.BaseStream.Position = EndPosition;
		}
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
