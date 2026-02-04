using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ufex.API.Visual;

/// <summary>
/// Reads DEFLATE compressed streams and extracts block information.
/// </summary>
public class DeflateStreamReader
{
	private static readonly Lazy<(Dictionary<int, (int code, int len)> litMap, Dictionary<int, (int code, int len)> distMap)> FixedHuffmanMaps =
		new Lazy<(Dictionary<int, (int code, int len)> litMap, Dictionary<int, (int code, int len)> distMap)>(BuildFixedHuffmanMaps);

	public class Block
	{
		public BlockType Type { get; set; }
		public Dictionary<int, (int code, int len)>? LiteralCodeMap { get; set; }
		public Dictionary<int, (int code, int len)>? DistanceCodeMap { get; set; }
	}

	public enum BlockType
	{
		Stored = 0,
		StaticHuffman = 1,
		DynamicHuffman = 2,
		Reserved = 3
	}

	/// <summary>
	/// Reads the stream and returns a list of blocks found in the stream.
	/// (one for every block found in the stream).
	/// </summary>
	public static List<Block> ReadBlocks(Stream stream)
	{
		var blocks = new List<Block>();

		using (var reader = new BitReader(stream, leaveOpen: true))
		{
			bool bfinal = false;
			while (!bfinal)
			{
				// --- 1. Read Block Header ---
				bfinal = reader.ReadBits(1) == 1;
				int btype = reader.ReadBits(2);

				if (btype == (int)BlockType.Stored)
				{
					// STORED Block (Uncompressed)
					SkipStoredBlock(reader);
					blocks.Add(new Block { Type = BlockType.Stored });
				}
				else if (btype == (int)BlockType.StaticHuffman)
				{
					// STATIC Huffman (Fixed trees)
					// Trees are defined by RFC 1951 and are not stored in the file.
					// We must decode enough to reach the End-Of-Block symbol (256) to find the next block.
					var (litMap, distMap) = FixedHuffmanMaps.Value;
					SkipHuffmanBlockData(reader, litMap, distMap);
					blocks.Add(new Block
					{
						Type = BlockType.StaticHuffman,
						LiteralCodeMap = litMap,
						DistanceCodeMap = distMap
					});
				}
				else if (btype == (int)BlockType.DynamicHuffman)
				{
					// DYNAMIC Huffman (The trees are inside the file)
					// 1. Read the header and build the tree nodes
					var (literalCodeMap, distCodeMap) = ReadDynamicHeader(reader);
					
					// 2. We MUST decode the compressed data to find the End-Of-Block symbol (256)
					//  so we can be in the correct position for the next block.
					SkipHuffmanBlockData(reader, literalCodeMap, distCodeMap);
					blocks.Add(new Block { 
						Type = BlockType.DynamicHuffman, 
						LiteralCodeMap = literalCodeMap, 
						DistanceCodeMap = distCodeMap 
					});
				}
				else if (btype == (int)BlockType.Reserved)
				{
					// RFC 1951: BTYPE=3 is reserved/invalid
					throw new InvalidDataException("Invalid DEFLATE block type (BTYPE=3 reserved) encountered.");
				}
				else
				{
					throw new InvalidDataException($"Invalid DEFLATE block type encountered: {btype}.");
				}
			}
		}
		return blocks;
	}

	/// <summary>
	/// Skips over a Stored block (block type = 0).
	/// </summary>
	/// <param name="reader"></param>
	/// <exception cref="Exception"></exception>
	private static void SkipStoredBlock(BitReader reader)
	{
		reader.AlignToByteBoundary();
		int len = reader.ReadUInt16();
		int nlen = reader.ReadUInt16(); // One's complement of len

		if ((ushort)len != (ushort)~nlen)
			throw new InvalidDataException("Invalid DEFLATE stored block length (LEN/NLEN mismatch).");

		// Skip the actual data bytes
		for (int i = 0; i < len; i++) reader.ReadByte();
	}

	/// <summary>
	/// Builds the fixed Huffman code maps for literal/length and distance codes.
	/// </summary>
	private static (Dictionary<int, (int code, int len)> litMap, Dictionary<int, (int code, int len)> distMap) BuildFixedHuffmanMaps()
	{
		// RFC1951: Fixed Huffman codes
		// Literal/Length alphabet: 288 symbols
		// 0-143: 8 bits
		// 144-255: 9 bits
		// 256-279: 7 bits
		// 280-287: 8 bits
		int[] litLengths = new int[288];
		for (int i = 0; i <= 143; i++) litLengths[i] = 8;
		for (int i = 144; i <= 255; i++) litLengths[i] = 9;
		for (int i = 256; i <= 279; i++) litLengths[i] = 7;
		for (int i = 280; i <= 287; i++) litLengths[i] = 8;

		// Distance alphabet: 32 symbols, all 5 bits
		int[] distLengths = new int[32];
		for (int i = 0; i < distLengths.Length; i++) distLengths[i] = 5;

		return (GenerateCanonicalCodes(litLengths), GenerateCanonicalCodes(distLengths));
	}

	/// <summary>
	/// Reads the dynamic header and builds the literal/length and distance code maps.
	/// </summary>
	/// <param name="reader"></param>
	/// <returns></returns>
	private static (Dictionary<int, (int, int)> litMap, Dictionary<int, (int, int)> distMap) ReadDynamicHeader(BitReader reader)
	{
		int hlit = reader.ReadBits(5) + 257;
		int hdist = reader.ReadBits(5) + 1;
		int hclen = reader.ReadBits(4) + 4;

		int[] codeLengthOrder = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1 };
		int[] codeLenCodeLengths = new int[19];

		for (int i = 0; i < hclen; i++)
			codeLenCodeLengths[codeLengthOrder[i]] = reader.ReadBits(3);

		var metaCodeMap = GenerateCanonicalCodes(codeLenCodeLengths);
		if (metaCodeMap.Count == 0)
			throw new InvalidDataException("Invalid DEFLATE dynamic header: code-length Huffman tree has no symbols.");

		// Read Combined Literal/Length + Distance lengths
		int[] allLengths = new int[hlit + hdist];
		int count = 0;
		
		while (count < hlit + hdist)
		{
			int symbol = DecodeSymbol(reader, metaCodeMap);
			if (symbol <= 15)
			{
				allLengths[count++] = symbol;
			}
			else if (symbol == 16)
			{
				if (count == 0)
					throw new InvalidDataException("Invalid DEFLATE dynamic header: repeat-previous (code 16) encountered with no previous length.");

				int copy = allLengths[count - 1];
				int repeat = reader.ReadBits(2) + 3;
				if (count + repeat > allLengths.Length)
					throw new InvalidDataException($"Invalid DEFLATE dynamic header: repeat-previous (code 16) overflows code-length array (index {count}, repeat {repeat}, size {allLengths.Length}).");
				for (int i = 0; i < repeat; i++) allLengths[count++] = copy;
			}
			else if (symbol == 17)
			{
				int repeat = reader.ReadBits(3) + 3;
				if (count + repeat > allLengths.Length)
					throw new InvalidDataException($"Invalid DEFLATE dynamic header: repeat-zero (code 17) overflows code-length array (index {count}, repeat {repeat}, size {allLengths.Length}).");
				for (int i = 0; i < repeat; i++) allLengths[count++] = 0;
			}
			else if (symbol == 18)
			{
				int repeat = reader.ReadBits(7) + 11;
				if (count + repeat > allLengths.Length)
					throw new InvalidDataException($"Invalid DEFLATE dynamic header: repeat-zero (code 18) overflows code-length array (index {count}, repeat {repeat}, size {allLengths.Length}).");
				for (int i = 0; i < repeat; i++) allLengths[count++] = 0;
			}
			else
			{
				throw new InvalidDataException($"Invalid DEFLATE dynamic header: unexpected code-length symbol {symbol}.");
			}
		}

		// Split into Literal and Distance lengths
		int[] litLengths = allLengths.Take(hlit).ToArray();
		int[] distLengths = allLengths.Skip(hlit).Take(hdist).ToArray();

		// Build maps for decoding the data
		var litMap = GenerateCanonicalCodes(litLengths);
		var distMap = GenerateCanonicalCodes(distLengths);

		return (litMap, distMap);
	}

	/// <summary>
	/// Skips over a Huffman-coded block (static or dynamic) by decoding until End-Of-Block (256).
	/// </summary>
	private static void SkipHuffmanBlockData(BitReader reader, Dictionary<int, (int, int)> litMap, Dictionary<int, (int, int)> distMap)
	{
		while (true)
		{
			int symbol = DecodeSymbol(reader, litMap);

			if (symbol < 256) { /* Literal byte - ignore */ }
			else if (symbol == 256) { return; /* END OF BLOCK */ }
			else
			{
				// Length/Distance pair
				// We must consume the correct extra bits so the stream stays aligned.
				int lengthExtraBits = GetLengthExtraBitCount(symbol);
				if (lengthExtraBits > 0)
					reader.ReadBits(lengthExtraBits);

				int distSymbol = DecodeSymbol(reader, distMap);
				int distExtraBits = GetDistanceExtraBitCount(distSymbol);
				if (distExtraBits > 0)
					reader.ReadBits(distExtraBits);
			}
		}
	}

	private static int GetLengthExtraBitCount(int symbol)
	{
		// RFC1951: length codes are 257..285
		// Extra bit counts for 257..285
		// 257-264: 0
		// 265-268: 1
		// 269-272: 2
		// 273-276: 3
		// 277-280: 4
		// 281-284: 5
		// 285: 0
		if (symbol >= 257 && symbol <= 264) return 0;
		if (symbol >= 265 && symbol <= 268) return 1;
		if (symbol >= 269 && symbol <= 272) return 2;
		if (symbol >= 273 && symbol <= 276) return 3;
		if (symbol >= 277 && symbol <= 280) return 4;
		if (symbol >= 281 && symbol <= 284) return 5;
		if (symbol == 285) return 0;
		throw new InvalidDataException($"Invalid DEFLATE length symbol: {symbol}.");
	}

	private static int GetDistanceExtraBitCount(int distSymbol)
	{
		// RFC1951: distance codes are 0..29 (30-31 invalid)
		// Extra bit counts for 0..29
		// 0-3: 0
		// 4-5: 1
		// 6-7: 2
		// ...
		// 28-29: 13
		if (distSymbol < 0 || distSymbol > 29)
			throw new InvalidDataException($"Invalid DEFLATE distance symbol: {distSymbol}.");
		if (distSymbol <= 3) return 0;
		return (distSymbol / 2) - 1;
	}

	// --- Shared Helper Methods ---
	private static Dictionary<int, (int code, int len)> GenerateCanonicalCodes(int[] lengths)
	{
		var result = new Dictionary<int, (int, int)>();
		int maxLen = lengths.Length == 0 ? 0 : lengths.Max();
		if (maxLen == 0)
			return result;
		int[] bl_count = new int[maxLen + 1];
		foreach (var l in lengths) if (l > 0) bl_count[l]++;
		int[] next_code = new int[maxLen + 1];
		int code = 0;
		bl_count[0] = 0;
		for (int bits = 1; bits <= maxLen; bits++)
		{
			code = (code + bl_count[bits - 1]) << 1;
			next_code[bits] = code;
		}
		for (int n = 0; n < lengths.Length; n++)
		{
			if (lengths[n] != 0)
			{
				result[n] = (next_code[lengths[n]], lengths[n]);
				next_code[lengths[n]]++;
			}
		}
		return result;
	}

	/// <summary>
	/// Decodes a symbol from the bit stream using the provided Huffman code map.
	/// </summary>
	private static int DecodeSymbol(BitReader reader, Dictionary<int, (int code, int len)> codes)
	{
		// For performance in a real app, use a lookup array. For this demo, this is fine.
		var lookup = codes.ToDictionary(x => (x.Value.len, x.Value.code), x => x.Key);
		int code = 0, len = 0;
		while (len < 15)
		{
			code = (code << 1) | reader.ReadBits(1);
			len++;
			if (lookup.TryGetValue((len, code), out int symbol)) return symbol;
		}
		throw new InvalidDataException("Invalid Huffman code or corrupted DEFLATE stream.");
	}
}