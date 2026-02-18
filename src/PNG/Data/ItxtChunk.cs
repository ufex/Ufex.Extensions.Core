using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// iTXt - International textual data
/// </summary>
internal class ItxtChunk : Chunk
{
	public string Keyword { get; init; } = string.Empty;
	public byte CompressionFlag { get; init; }
	public byte CompressionMethod { get; init; }
	public string LanguageTag { get; init; } = string.Empty;
	public string TranslatedKeyword { get; init; } = string.Empty;
	public byte[] TextBytes { get; init; } = Array.Empty<byte>();
	public byte[]? CompressedTextBytes { get; init; }
	public string Text { get; init; } = string.Empty;

	public bool IsCompressed => CompressionFlag != 0;

	public ItxtChunk(FileReader fr) : base(fr)
	{
		if (Length == 0)
		{
			return;
		}

		// iTXt data format:
		// Keyword (Latin-1) + NUL + CompressionFlag (1) + CompressionMethod (1)
		// + LanguageTag (ASCII) + NUL + TranslatedKeyword (UTF-8) + NUL
		// + Text (UTF-8, optionally zlib-compressed depending on CompressionFlag)
		byte[] data = fr.ReadBytes((int)Length);
		int index = 0;

		static bool TryReadNullTerminatedString(byte[] buffer, ref int offset, Encoding encoding, out string value)
		{
			value = string.Empty;
			if (offset < 0 || offset > buffer.Length)
			{
				return false;
			}

			int nullIndex = Array.IndexOf(buffer, (byte)0, offset);
			if (nullIndex < 0)
			{
				return false;
			}

			int length = nullIndex - offset;
			value = length > 0 ? encoding.GetString(buffer, offset, length) : string.Empty;
			offset = nullIndex + 1;
			return true;
		}

		if (!TryReadNullTerminatedString(data, ref index, Encoding.Latin1, out string keyword))
		{
			return;
		}
		Keyword = keyword;

		if (index + 2 > data.Length)
		{
			return;
		}

		CompressionFlag = data[index++];
		CompressionMethod = data[index++];

		if (!TryReadNullTerminatedString(data, ref index, Encoding.ASCII, out string languageTag))
		{
			return;
		}
		LanguageTag = languageTag;

		if (!TryReadNullTerminatedString(data, ref index, Encoding.UTF8, out string translatedKeyword))
		{
			return;
		}
		TranslatedKeyword = translatedKeyword;

		if (index > data.Length)
		{
			return;
		}

		int remaining = data.Length - index;
		if (remaining <= 0)
		{
			TextBytes = Array.Empty<byte>();
			Text = string.Empty;
			return;
		}

		var payload = new byte[remaining];
		Buffer.BlockCopy(data, index, payload, 0, remaining);

		if (IsCompressed)
		{
			CompressedTextBytes = payload;
			if (CompressionMethod == 0)
			{
				try
				{
					using var input = new MemoryStream(payload, writable: false);
					using var zlib = new ZLibStream(input, CompressionMode.Decompress);
					using var output = new MemoryStream();
					zlib.CopyTo(output);
					TextBytes = output.ToArray();
					Text = Encoding.UTF8.GetString(TextBytes);
				}
				catch
				{
					TextBytes = Array.Empty<byte>();
					Text = string.Empty;
				}
			}
			else
			{
				// Unknown compression method
				TextBytes = Array.Empty<byte>();
				Text = string.Empty;
			}
		}
		else
		{
			TextBytes = payload;
			Text = Encoding.UTF8.GetString(payload);
		}
	}
}
