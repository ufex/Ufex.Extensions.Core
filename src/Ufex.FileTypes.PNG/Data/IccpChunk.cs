using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// iCCP - Embedded ICC profile
/// </summary>
internal class IccpChunk : Chunk
{
	public string ProfileName { get; init; } = string.Empty;
	public byte CompressionMethod { get; init; }
	public byte[] CompressedProfile { get; init; } = Array.Empty<byte>();
	public byte[]? Profile { get; init; }

	public IccpChunk(FileReader fr) : base(fr)
	{
		if (Length == 0)
		{
			return;
		}

		// iCCP data format:
		// ProfileName (Latin-1, 1-79 bytes) + NUL (1) + CompressionMethod (1) + CompressedProfile (remaining)
		byte[] data = fr.ReadBytes((int)Length);
		int nullIndex = Array.IndexOf(data, (byte)0);
		if (nullIndex < 0)
		{
			// Invalid chunk; leave properties at defaults.
			return;
		}

		ProfileName = Encoding.Latin1.GetString(data, 0, nullIndex);
		if (nullIndex + 1 >= data.Length)
		{
			return;
		}

		CompressionMethod = data[nullIndex + 1];
		int compressedOffset = nullIndex + 2;
		if (compressedOffset > data.Length)
		{
			return;
		}

		int compressedLength = data.Length - compressedOffset;
		if (compressedLength > 0)
		{
			CompressedProfile = new byte[compressedLength];
			Buffer.BlockCopy(data, compressedOffset, CompressedProfile, 0, compressedLength);
		}

		// Compression method 0 = deflate (zlib stream)
		if (CompressionMethod == 0 && CompressedProfile.Length > 0)
		{
			try
			{
				using var input = new MemoryStream(CompressedProfile, writable: false);
				using var zlib = new ZLibStream(input, CompressionMode.Decompress);
				using var output = new MemoryStream();
				zlib.CopyTo(output);
				Profile = output.ToArray();
			}
			catch
			{
				Profile = null;
			}
		}
	}
}
