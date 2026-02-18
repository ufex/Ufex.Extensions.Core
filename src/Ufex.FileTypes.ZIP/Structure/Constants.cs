using System;
using System.Collections.Generic;
using Ufex.API;

namespace Ufex.FileTypes.ZIP.Structure;

internal static class Constants
{
	public static readonly Dictionary<int, string> COMPRESSION_METHODS = new Dictionary<int, string>
	{
		{ 0, "Stored (no compression)" },
		{ 1, "Shrunk" },
		{ 2, "Reduced with compression factor 1" },
		{ 3, "Reduced with compression factor 2" },
		{ 4, "Reduced with compression factor 3" },
		{ 5, "Reduced with compression factor 4" },
		{ 6, "Imploded" },
		{ 7, "Reserved for Tokenizing compression algorithm" },
		{ 8, "Deflated" },
		{ 9, "Deflate64" },
		{ 10, "PKWARE Data Compression Library Imploding" },
		{ 11, "Reserved by PKWARE" },
		{ 12, "BZIP2" },
		{ 13, "Reserved by PKWARE" },
		{ 14, "LZMA" },
		{ 15, "Reserved by PKWARE" },
		{ 16, "Reserved by PKWARE" },
		{ 17, "Reserved by PKWARE" },
		{ 18, "IBM TERSE" },
		{ 19, "IBM LZ77 z Architecture (PFS)" },
		{ 97, "WavPack" },
		{ 98, "PPMd version I, Rev 1" }
	};

	public static string CompressionMethodDescription(int method)
	{
		if (COMPRESSION_METHODS.ContainsKey(method))
			return COMPRESSION_METHODS[method];
		else
			return "Unknown";
	}

	public static string GeneralPurposeBitFlagDescription(UInt16 flag, UInt16 compressionMethod)
	{
		List<string> descriptions = new List<string>();
		if (ByteUtil.GetBit(flag, 0)) descriptions.Add("Encrypted");
		if (compressionMethod == 6) {
			descriptions.Add(ByteUtil.GetBit(flag, 1) ? "8K sliding dictionary" : "4K sliding dictionary");
			descriptions.Add(ByteUtil.GetBit(flag, 2) ? "3 Shannon-Fano trees" : "2 Shannon-Fano trees");
		}
		else if (compressionMethod == 8 || compressionMethod == 9) {
			var bit1 = ByteUtil.GetBit(flag, 1);
			var bit2 = ByteUtil.GetBit(flag, 2);
			if(bit1 && bit2)
				descriptions.Add("Super Fast compression");
			else if(!bit1 && bit2)
				descriptions.Add("Fast compression");
			else if(bit1 && !bit2)
				descriptions.Add("Maximum compression");
			else
				descriptions.Add("Normal compression");
		}
		else if(compressionMethod == 14) {
			descriptions.Add(ByteUtil.GetBit(flag, 1) ? "end-of-stream marker" : "no end-of-stream marker");
		}
		if (ByteUtil.GetBit(flag, 3)) descriptions.Add("Data descriptor present");
		if (ByteUtil.GetBit(flag, 4) && compressionMethod == 8) descriptions.Add("Enhanced deflation");
		if (ByteUtil.GetBit(flag, 5)) descriptions.Add("Compressed patched data");
		if (ByteUtil.GetBit(flag, 6)) descriptions.Add("Strong encryption");
		if (ByteUtil.GetBit(flag, 11)) descriptions.Add("Language encoding flag (UTF-8 filenames and comments)");
		if (ByteUtil.GetBit(flag, 12)) descriptions.Add("Reserved by PKWARE for enhanced compression");
		return string.Join(", ", descriptions);
	}
}
