using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// DRI - Define Restart Interval marker segment (0xFFDD)
/// Specifies the number of MCU (Minimum Coded Units) between restart markers.
/// A restart interval of 0 disables restart markers.
/// </summary>
internal class DriSegment : Segment
{
	/// <summary>
	/// Restart interval (number of MCUs between restart markers)
	/// </summary>
	public ushort RestartInterval { get; init; }

	public DriSegment(FileReader fr) : base(fr)
	{
		RestartInterval = fr.ReadUInt16();
	}
}
