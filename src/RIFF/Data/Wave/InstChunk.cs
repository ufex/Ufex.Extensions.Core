using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// inst - Instrument chunk. Contains information for playing the waveform 
/// data as a musical instrument sound.
/// </summary>
internal class InstChunk : Chunk
{
	/// <summary>
	/// The MIDI note number that corresponds to the unshifted pitch 
	/// of the sample (0-127).
	/// </summary>
	public Byte UnshiftedNote { get; init; }

	/// <summary>
	/// Fine tune in cents (-50 to +50). A negative value means that 
	/// the pitch should be played lower, positive higher.
	/// </summary>
	public SByte FineTune { get; init; }

	/// <summary>
	/// The suggested gain in decibels to apply to the sample.
	/// </summary>
	public SByte Gain { get; init; }

	/// <summary>
	/// The lowest MIDI note number for which the waveform should be played (0-127).
	/// </summary>
	public Byte LowNote { get; init; }

	/// <summary>
	/// The highest MIDI note number for which the waveform should be played (0-127).
	/// </summary>
	public Byte HighNote { get; init; }

	/// <summary>
	/// The lowest MIDI velocity for which the waveform should be played (1-127).
	/// </summary>
	public Byte LowVelocity { get; init; }

	/// <summary>
	/// The highest MIDI velocity for which the waveform should be played (1-127).
	/// </summary>
	public Byte HighVelocity { get; init; }

	public InstChunk(FileReader fr) : base(fr)
	{
		UnshiftedNote = fr.ReadByte();
		FineTune = fr.ReadSByte();
		Gain = fr.ReadSByte();
		LowNote = fr.ReadByte();
		HighNote = fr.ReadByte();
		LowVelocity = fr.ReadByte();
		HighVelocity = fr.ReadByte();
	}
}
