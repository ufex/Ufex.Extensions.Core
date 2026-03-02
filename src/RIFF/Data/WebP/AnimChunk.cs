using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.WebP;

/// <summary>
/// ANIM - Animation chunk. Contains global animation parameters.
/// This chunk must appear if the Animation flag in the VP8X chunk is set.
/// </summary>
internal class AnimChunk : Chunk
{
	/// <summary>
	/// The default background color of the canvas in [Blue, Green, Red, Alpha]
	/// byte order. Used to fill unused space on the canvas and transparent
	/// pixels of the first frame.
	/// </summary>
	public UInt32 BackgroundColor { get; init; }

	/// <summary>
	/// The number of times to loop the animation. 0 means infinite.
	/// </summary>
	public UInt16 LoopCount { get; init; }

	public AnimChunk(FileReader fr) : base(fr)
	{
		BackgroundColor = fr.ReadUInt32();
		LoopCount = fr.ReadUInt16();
	}
}
