using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// imir — Image Mirror property. Specifies a reflection axis.
/// Note: imir is a plain box, NOT a FullBox.
/// </summary>
internal class ImirBox : Box
{
	/// <summary>
	/// Mirror axis. Low bit: 0 = vertical axis (horizontal flip), 1 = horizontal axis (vertical flip).
	/// </summary>
	public Byte Axis { get; init; }

	public string AxisDescription => (Axis & 0x01) == 0
		? "Vertical axis (horizontal flip)"
		: "Horizontal axis (vertical flip)";

	public ImirBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, false)
	{
		Axis = fr.ReadByte();
	}
}
