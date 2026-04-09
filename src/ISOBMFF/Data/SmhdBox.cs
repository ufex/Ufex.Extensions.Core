using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// smhd — Sound Media Header Box.
/// Contains audio-specific media information.
/// </summary>
internal class SmhdBox : Box
{
	/// <summary>
	/// Audio balance as fixed8.8. 0 = centre, -1.0 = full left, 1.0 = full right.
	/// </summary>
	public Int16 Balance { get; init; }

	/// <summary>
	/// Reserved, set to zero.
	/// </summary>
	public UInt16 Reserved1 { get; init; }

	public SmhdBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		Balance = fr.ReadInt16();
		Reserved1 = fr.ReadUInt16();
	}
}
