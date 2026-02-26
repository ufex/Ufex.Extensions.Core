using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// SOFn - Start of Frame marker segment (0xFFC0 through 0xFFC3)
/// Contains the image dimensions, sample precision, and component information.
/// The marker type indicates the compression method:
///   SOF0 (0xC0) = Baseline DCT
///   SOF1 (0xC1) = Extended Sequential DCT
///   SOF2 (0xC2) = Progressive DCT
///   SOF3 (0xC3) = Lossless (Sequential)
/// </summary>
internal class SofSegment : Segment
{
	/// <summary>
	/// Sample precision in bits (typically 8)
	/// </summary>
	public byte SamplePrecision { get; init; }

	/// <summary>
	/// Number of lines (image height)
	/// </summary>
	public ushort NumberOfLines { get; init; }

	/// <summary>
	/// Samples per line (image width)
	/// </summary>
	public ushort SamplesPerLine { get; init; }

	/// <summary>
	/// Number of image components (1 = grayscale, 3 = YCbCr)
	/// </summary>
	public byte NumberOfComponents { get; init; }

	/// <summary>
	/// Component IDs (one per component)
	/// </summary>
	public byte[] ComponentIds { get; init; }

	/// <summary>
	/// Sampling factors packed as (H &lt;&lt; 4 | V) for each component
	/// </summary>
	public byte[] SamplingFactors { get; init; }

	/// <summary>
	/// Quantization table selectors (one per component)
	/// </summary>
	public byte[] QuantizationTableSelectors { get; init; }

	public SofSegment(FileReader fr) : base(fr)
	{
		SamplePrecision = fr.ReadByte();
		NumberOfLines = fr.ReadUInt16();
		SamplesPerLine = fr.ReadUInt16();
		NumberOfComponents = fr.ReadByte();

		ComponentIds = new byte[NumberOfComponents];
		SamplingFactors = new byte[NumberOfComponents];
		QuantizationTableSelectors = new byte[NumberOfComponents];

		for (int i = 0; i < NumberOfComponents; i++)
		{
			ComponentIds[i] = fr.ReadByte();
			SamplingFactors[i] = fr.ReadByte();
			QuantizationTableSelectors[i] = fr.ReadByte();
		}
	}

	/// <summary>
	/// Gets the horizontal sampling factor for a component
	/// </summary>
	public int GetHorizontalSampling(int component) => (SamplingFactors[component] >> 4) & 0x0F;

	/// <summary>
	/// Gets the vertical sampling factor for a component
	/// </summary>
	public int GetVerticalSampling(int component) => SamplingFactors[component] & 0x0F;

	/// <summary>
	/// Gets the component name based on component ID and index
	/// </summary>
	public static string GetComponentName(byte componentId, int index)
	{
		return componentId switch
		{
			1 => "Y (Luminance)",
			2 => "Cb (Chrominance Blue)",
			3 => "Cr (Chrominance Red)",
			_ => $"Component {index + 1} (ID={componentId})",
		};
	}
}
