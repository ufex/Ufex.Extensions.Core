using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// pixi — Pixel Information property. Declares the number of channels and bits
/// per channel for each image component.
/// </summary>
internal class PixiBox : Box
{
	public Byte NumChannels { get; init; }
	public Byte[] BitsPerChannel { get; init; }

	public PixiBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		NumChannels = fr.ReadByte();
		BitsPerChannel = fr.ReadBytes(NumChannels);
	}
}
