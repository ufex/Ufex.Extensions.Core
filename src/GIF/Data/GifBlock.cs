using Ufex.API;

namespace Ufex.Extensions.Core.GIF.Data;

/// <summary>
/// Base class for all GIF data blocks
/// Handles common functionality like tracking file offset
/// </summary>
internal abstract class GifBlock
{
	/// <summary>
	/// File offset where this block starts
	/// </summary>
	public long Offset { get; init; }

	protected GifBlock(FileReader fr)
	{
		Offset = fr.BaseStream.Position;
	}

	/// <summary>
	/// Constructor for blocks that don't read from a FileReader
	/// (e.g., when offset is set separately)
	/// </summary>
	protected GifBlock(long offset)
	{
		Offset = offset;
	}
}
