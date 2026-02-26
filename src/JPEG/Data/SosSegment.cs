using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// SOS - Start of Scan marker segment (0xFFDA)
/// Contains scan parameters (component selectors and Huffman table assignments).
/// The entropy-coded image data follows immediately after the SOS header.
/// </summary>
internal class SosSegment : Segment
{
	/// <summary>
	/// Number of components in scan
	/// </summary>
	public byte NumberOfComponents { get; init; }

	/// <summary>
	/// Component selectors (one per component in scan)
	/// </summary>
	public byte[] ComponentSelectors { get; init; }

	/// <summary>
	/// DC and AC Huffman table selectors packed byte for each component:
	/// High nibble = DC table selector (0-3)
	/// Low nibble = AC table selector (0-3)
	/// </summary>
	public byte[] HuffmanTableSelectors { get; init; }

	/// <summary>
	/// Start of spectral or predictor selection
	/// </summary>
	public byte SpectralSelectionStart { get; init; }

	/// <summary>
	/// End of spectral selection
	/// </summary>
	public byte SpectralSelectionEnd { get; init; }

	/// <summary>
	/// Successive approximation bit position (high and low)
	/// </summary>
	public byte SuccessiveApproximation { get; init; }

	/// <summary>
	/// File offset where the entropy-coded scan data begins
	/// (set by the StreamReader after scanning past SOS header)
	/// </summary>
	public long ScanDataOffset { get; set; }

	/// <summary>
	/// Length of the entropy-coded scan data in bytes
	/// (set by the StreamReader after scanning to the next marker)
	/// </summary>
	public long ScanDataLength { get; set; }

	public SosSegment(FileReader fr) : base(fr)
	{
		NumberOfComponents = fr.ReadByte();

		ComponentSelectors = new byte[NumberOfComponents];
		HuffmanTableSelectors = new byte[NumberOfComponents];

		for (int i = 0; i < NumberOfComponents; i++)
		{
			ComponentSelectors[i] = fr.ReadByte();
			HuffmanTableSelectors[i] = fr.ReadByte();
		}

		SpectralSelectionStart = fr.ReadByte();
		SpectralSelectionEnd = fr.ReadByte();
		SuccessiveApproximation = fr.ReadByte();
	}

	/// <summary>
	/// Gets the DC Huffman table selector for a component
	/// </summary>
	public int GetDcTableSelector(int component) => (HuffmanTableSelectors[component] >> 4) & 0x0F;

	/// <summary>
	/// Gets the AC Huffman table selector for a component
	/// </summary>
	public int GetAcTableSelector(int component) => HuffmanTableSelectors[component] & 0x0F;
}
