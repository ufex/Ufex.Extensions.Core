using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// vmhd — Video Media Header Box.
/// Contains video-specific media information.
/// </summary>
internal class VmhdBox : Box
{
	/// <summary>
	/// Composition mode for compositing this video track.
	/// 0 = copy (default).
	/// </summary>
	public UInt16 GraphicsMode { get; init; }

	/// <summary>
	/// RGB colour values for the graphics mode (3 x UInt16).
	/// </summary>
	public UInt16[] OpColor { get; init; }   // 3 x 2 bytes

	public VmhdBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		GraphicsMode = fr.ReadUInt16();
		OpColor = [ fr.ReadUInt16(), fr.ReadUInt16(), fr.ReadUInt16() ];
	}
}
