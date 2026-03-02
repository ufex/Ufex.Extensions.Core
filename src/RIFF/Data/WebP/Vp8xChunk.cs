using Ufex.API;
using Ufex.API.Types;

namespace Ufex.Extensions.Core.RIFF.Data.WebP;

/// <summary>
/// VP8X - Extended file header chunk. Contains feature flags and canvas dimensions.
/// </summary>
internal class Vp8xChunk : Chunk
{
	/// <summary>
	/// Feature flags byte. Individual flags can be accessed via the
	/// Has* properties.
	/// </summary>
	public Byte Flags { get; init; }

	/// <summary>
	/// Reserved bytes (3 bytes, must be 0).
	/// </summary>
	public Byte[] Reserved { get; init; }

	/// <summary>
	/// 1-based canvas width stored as a 24-bit value.
	/// The actual canvas width is CanvasWidthMinusOne + 1.
	/// </summary>
	public UInt24 CanvasWidthMinusOne { get; init; }

	/// <summary>
	/// 1-based canvas height stored as a 24-bit value.
	/// The actual canvas height is CanvasHeightMinusOne + 1.
	/// </summary>
	public UInt24 CanvasHeightMinusOne { get; init; }

	/// <summary>
	/// Actual canvas width in pixels.
	/// </summary>
	public uint CanvasWidth => (uint)CanvasWidthMinusOne + 1;

	/// <summary>
	/// Actual canvas height in pixels.
	/// </summary>
	public uint CanvasHeight => (uint)CanvasHeightMinusOne + 1;

	/// <summary>
	/// Set if the file contains an ICC color profile ('ICCP' chunk).
	/// </summary>
	public bool HasIccProfile => (Flags & 0x20) != 0;

	/// <summary>
	/// Set if any frame contains transparency information (alpha).
	/// </summary>
	public bool HasAlpha => (Flags & 0x10) != 0;

	/// <summary>
	/// Set if the file contains Exif metadata ('EXIF' chunk).
	/// </summary>
	public bool HasExif => (Flags & 0x08) != 0;

	/// <summary>
	/// Set if the file contains XMP metadata ('XMP ' chunk).
	/// </summary>
	public bool HasXmp => (Flags & 0x04) != 0;

	/// <summary>
	/// Set if this is an animated image ('ANIM' and 'ANMF' chunks present).
	/// </summary>
	public bool HasAnimation => (Flags & 0x02) != 0;

	public Vp8xChunk(FileReader fr) : base(fr)
	{
		Flags = fr.ReadByte();
		Reserved = fr.ReadBytes(3);
		CanvasWidthMinusOne = fr.ReadUInt24();
		CanvasHeightMinusOne = fr.ReadUInt24();
	}
}
