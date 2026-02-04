using Ufex.API.Tree;
using Ufex.API.Format;
using Ufex.FileTypes.BMP.Data;

namespace Ufex.FileTypes.BMP.Structure;

/// <summary>
/// BITMAPINFOHEADER - BMP info header node (v3, v4, v5)
/// </summary>
internal class BitmapInfoHeaderNode : HeaderNode
{
	protected readonly BitmapInfoHeader _header;

	public BitmapInfoHeaderNode(BitmapInfoHeader header)
		: base("Info Header", GetHeaderDescription(header), header.Offset, TreeViewIcon.Header)
	{
		_header = header;
	}

	private static string GetHeaderDescription(BitmapInfoHeader header)
	{
		return header switch
		{
			BitmapV5Header => "BITMAPV5HEADER - 124 bytes",
			BitmapV4Header => "BITMAPV4HEADER - 108 bytes",
			_ => "BITMAPINFOHEADER - 40 bytes"
		};
	}

	protected override object[][] GetRows()
	{
		var rows = new List<object[]>{};
		rows.AddRange([
			[ "biSize", _header.Size, ByteCountFormatter.Format(_header.Size) ],
			[ "biWidth", _header.Width, $"{_header.Width} pixels" ],
			[ "biHeight", _header.Height, GetHeightDescription() ],
			[ "biPlanes", _header.Planes, _header.Planes == 1 ? "" : "Should be 1" ],
			[ "biBitCount", _header.BitsPerPixel, GetBitDepthDescription() ],
			[ "biCompression", _header.Compression, Constants.GetCompressionDescription(_header.Compression) ],
			[ "biSizeImage", _header.SizeOfBitmap, ByteCountFormatter.Format(_header.SizeOfBitmap) ],
			[ "biXPelsPerMeter", _header.HorzResolution, GetDpiDescription(_header.HorzResolution) ],
			[ "biYPelsPerMeter", _header.VertResolution, GetDpiDescription(_header.VertResolution) ],
			[ "biClrUsed", _header.ColorsUsed ],
			[ "biClrImportant", _header.ColorsImportant ]
		]);

		// Add V4 fields if applicable
		if (_header is BitmapV4Header v4)
		{
			rows.AddRange(GetV4Rows(v4));
		}

		// Add V5 fields if applicable
		if (_header is BitmapV5Header v5)
		{
			rows.AddRange(GetV5Rows(v5));
		}

		return rows.ToArray();
	}

	private string GetHeightDescription()
	{
		int absHeight = Math.Abs(_header.Height);
		string orientation = _header.Height < 0 ? " (top-down)" : " (bottom-up)";
		return $"{absHeight} pixels{orientation}";
	}

	private string GetBitDepthDescription()
	{
		return _header.BitsPerPixel switch
		{
			1 => "Monochrome",
			4 => "16 colors",
			8 => "256 colors",
			16 => "High Color (16-bit)",
			24 => "True Color (24-bit)",
			32 => "True Color + Alpha (32-bit)",
			_ => $"{_header.BitsPerPixel}-bit"
		};
	}

	private static string GetDpiDescription(int pixelsPerMeter)
	{
		if (pixelsPerMeter == 0)
			return "";
		double dpi = pixelsPerMeter * 0.0254;
		return $"~{dpi:F0} DPI";
	}

	private static object[][] GetV4Rows(BitmapV4Header v4)
	{
		return [
			["bV4RedMask", v4.RedMask, $"0x{v4.RedMask:X8}"],
			["bV4GreenMask", v4.GreenMask, $"0x{v4.GreenMask:X8}"],
			["bV4BlueMask", v4.BlueMask, $"0x{v4.BlueMask:X8}"],
			["bV4AlphaMask", v4.AlphaMask, $"0x{v4.AlphaMask:X8}"],
			["bV4CSType", v4.CSType, Constants.GetColorSpaceDescription(v4.CSType)],
			["bV4Endpoints.Red", v4.Endpoints.CieXyzRed.ToString()],
			["bV4Endpoints.Green", v4.Endpoints.CieXyzGreen.ToString()],
			["bV4Endpoints.Blue", v4.Endpoints.CieXyzBlue.ToString()],
			["bV4GammaRed", v4.GammaRed],
			["bV4GammaGreen", v4.GammaGreen],
			["bV4GammaBlue", v4.GammaBlue],
		];
	}

	private static object[][] GetV5Rows(BitmapV5Header v5)
	{
		return [
			["bV5Intent", v5.Intent, Constants.GetRenderingIntentDescription(v5.Intent)],
			["bV5ProfileData", v5.ProfileData],
			["bV5ProfileSize", v5.ProfileSize, ByteCountFormatter.Format(v5.ProfileSize)],
			["bV5Reserved", v5.Reserved, v5.Reserved == 0 ? "" : "Should be 0"],
		];
	}
}
