using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// COM - Comment marker segment (0xFFFE)
/// Contains a text comment embedded in the JPEG file.
/// </summary>
internal class ComSegment : Segment
{
	/// <summary>
	/// Comment text as raw bytes
	/// </summary>
	public byte[] CommentBytes { get; init; }

	/// <summary>
	/// Comment text as ASCII string
	/// </summary>
	public string CommentText => System.Text.Encoding.ASCII.GetString(CommentBytes);

	public ComSegment(FileReader fr) : base(fr)
	{
		// Length includes the 2-byte length field itself
		int dataLength = Length - 2;
		CommentBytes = dataLength > 0 ? fr.ReadBytes(dataLength) : [];
	}
}
