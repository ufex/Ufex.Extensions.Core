using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// EOI - End of Image marker (0xFFD9)
/// Standalone marker with no data payload.
/// Marks the end of the JPEG bitstream.
/// </summary>
internal class EoiSegment : Segment
{
	public EoiSegment(FileReader fr) : base(fr, standalone: true)
	{
	}
}
