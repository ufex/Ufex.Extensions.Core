using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// smpl - Sampler chunk. Contains information needed by a sampler to 
/// correctly play back this waveform data.
/// </summary>
internal class SmplChunk : Chunk
{
	/// <summary>
	/// The MIDI Manufacturer's Association manufacturer code for the 
	/// intended target device.
	/// </summary>
	public UInt32 Manufacturer { get; init; }

	/// <summary>
	/// The product code of the intended target device.
	/// </summary>
	public UInt32 Product { get; init; }

	/// <summary>
	/// The period of one sample in nanoseconds (e.g., 22675 for 44.1 kHz).
	/// </summary>
	public UInt32 SamplePeriod { get; init; }

	/// <summary>
	/// The MIDI note number of the recorded pitch (0-127).
	/// </summary>
	public UInt32 MidiUnityNote { get; init; }

	/// <summary>
	/// The fraction of a semitone above the specified MIDI note.
	/// A value of 0x80000000 is 1/2 semitone (50 cents).
	/// </summary>
	public UInt32 MidiPitchFraction { get; init; }

	/// <summary>
	/// The SMPTE format (0, 24, 25, 29, 30).
	/// </summary>
	public UInt32 SmpteFormat { get; init; }

	/// <summary>
	/// The SMPTE time offset.
	/// </summary>
	public UInt32 SmpteOffset { get; init; }

	/// <summary>
	/// The number of sample loop definitions.
	/// </summary>
	public UInt32 SampleLoopCount { get; init; }

	/// <summary>
	/// The size, in bytes, of the optional sampler-specific data 
	/// that follows the sample loop definitions.
	/// </summary>
	public UInt32 SamplerDataSize { get; init; }

	/// <summary>
	/// Array of sample loop definitions.
	/// </summary>
	public SampleLoop[] SampleLoops { get; init; }

	public SmplChunk(FileReader fr) : base(fr)
	{
		Manufacturer = fr.ReadUInt32();
		Product = fr.ReadUInt32();
		SamplePeriod = fr.ReadUInt32();
		MidiUnityNote = fr.ReadUInt32();
		MidiPitchFraction = fr.ReadUInt32();
		SmpteFormat = fr.ReadUInt32();
		SmpteOffset = fr.ReadUInt32();
		SampleLoopCount = fr.ReadUInt32();
		SamplerDataSize = fr.ReadUInt32();
		SampleLoops = new SampleLoop[SampleLoopCount];
		for (int i = 0; i < SampleLoopCount; i++)
		{
			SampleLoops[i] = new SampleLoop(fr);
		}
		// Skip any remaining sampler-specific data
	}
}

/// <summary>
/// A single sample loop definition within a sampler chunk.
/// </summary>
internal class SampleLoop
{
	/// <summary>
	/// Unique identifier for this loop.
	/// </summary>
	public UInt32 ID { get; init; }

	/// <summary>
	/// The loop type: 0 = Forward, 1 = Ping Pong, 2 = Reverse.
	/// </summary>
	public UInt32 Type { get; init; }

	/// <summary>
	/// The byte offset into the waveform data of the beginning of the loop.
	/// </summary>
	public UInt32 Start { get; init; }

	/// <summary>
	/// The byte offset into the waveform data of the end of the loop.
	/// </summary>
	public UInt32 End { get; init; }

	/// <summary>
	/// The fraction of a sample at which the loop begins.
	/// </summary>
	public UInt32 Fraction { get; init; }

	/// <summary>
	/// The number of times to play the loop. 0 = infinite.
	/// </summary>
	public UInt32 PlayCount { get; init; }

	public SampleLoop(FileReader fr)
	{
		ID = fr.ReadUInt32();
		Type = fr.ReadUInt32();
		Start = fr.ReadUInt32();
		End = fr.ReadUInt32();
		Fraction = fr.ReadUInt32();
		PlayCount = fr.ReadUInt32();
	}
}
