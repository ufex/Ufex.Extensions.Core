using System;
using System.IO;
using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// hIST - Image histogram
/// </summary>
internal class HistChunk : Chunk
{
	public UInt16[] Frequencies { get; init; } = Array.Empty<UInt16>();

	public HistChunk(FileReader fr) : base(fr)
	{
		// hIST data is a series of UInt16 frequencies (one per palette entry).
		if ((Length % 2) != 0)
		{
			// Invalid length; skip the bytes to keep the reader aligned.
			fr.BaseStream.Seek(Length, SeekOrigin.Current);
			return;
		}

		int entryCount = checked((int)(Length / 2));
		var frequencies = new UInt16[entryCount];
		for (int i = 0; i < entryCount; i++)
		{
			frequencies[i] = fr.ReadUInt16();
		}

		Frequencies = frequencies;
	}
}
