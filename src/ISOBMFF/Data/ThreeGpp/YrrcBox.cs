using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

/// <summary>
/// yrrc — Recording Year Box. Stores the year when the content was recorded.
/// Note: yrrc is a FullBox but has NO language field (unlike other 3GPP metadata boxes).
/// </summary>
internal class YrrcBox : Box
{
	/// <summary>
	/// Recording year (e.g. 2024).
	/// </summary>
	public UInt16 RecordingYear { get; init; }

	public YrrcBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		RecordingYear = fr.ReadUInt16();
	}
}
