using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// pitm — Primary Item Box. Identifies the primary (default) image item in a HEIF file.
/// </summary>
internal class PitmBox : Box
{
	/// <summary>
	/// The item ID of the primary image. UInt16 for version 0, UInt32 for version 1.
	/// </summary>
	public UInt32 ItemId { get; init; }

	public PitmBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		if (Version == 0)
			ItemId = fr.ReadUInt16();
		else
			ItemId = fr.ReadUInt32();
	}
}
