using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// SOI - Start of Image marker (0xFFD8)
/// Standalone marker with no data payload.
/// Must be the first two bytes of every JPEG file.
/// </summary>
internal class SoiSegment : Segment
{
	public SoiSegment(FileReader fr) : base(fr, standalone: true)
	{
	}
}
