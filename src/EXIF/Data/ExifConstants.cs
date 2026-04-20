namespace Ufex.Extensions.Core.EXIF.Data;

public static class ExifConstants
{
	public const UInt16 TypeByte = 1;
	public const UInt16 TypeAscii = 2;
	public const UInt16 TypeShort = 3;
	public const UInt16 TypeLong = 4;
	public const UInt16 TypeRational = 5;
	public const UInt16 TypeSByte = 6;
	public const UInt16 TypeUndefined = 7;
	public const UInt16 TypeSShort = 8;
	public const UInt16 TypeSLong = 9;
	public const UInt16 TypeSRational = 10;

	public static readonly Dictionary<UInt16, Int32> TypeSizes = new()
	{
		{ TypeByte, 1 },
		{ TypeAscii, 1 },
		{ TypeShort, 2 },
		{ TypeLong, 4 },
		{ TypeRational, 8 },
		{ TypeSByte, 1 },
		{ TypeUndefined, 1 },
		{ TypeSShort, 2 },
		{ TypeSLong, 4 },
		{ TypeSRational, 8 },
	};

	public static readonly Dictionary<UInt16, string> TypeNames = new()
	{
		{ TypeByte, "BYTE" },
		{ TypeAscii, "ASCII" },
		{ TypeShort, "SHORT" },
		{ TypeLong, "LONG" },
		{ TypeRational, "RATIONAL" },
		{ TypeSByte, "SBYTE" },
		{ TypeUndefined, "UNDEFINED" },
		{ TypeSShort, "SSHORT" },
		{ TypeSLong, "SLONG" },
		{ TypeSRational, "SRATIONAL" },
	};

	public static readonly Dictionary<UInt16, string> Ifd0Tags = new()
	{
		{ 0x0100, "ImageWidth" },
		{ 0x0101, "ImageLength" },
		{ 0x0102, "BitsPerSample" },
		{ 0x0103, "Compression" },
		{ 0x0106, "PhotometricInterpretation" },
		{ 0x010E, "ImageDescription" },
		{ 0x010F, "Make" },
		{ 0x0110, "Model" },
		{ 0x0112, "Orientation" },
		{ 0x0115, "SamplesPerPixel" },
		{ 0x011A, "XResolution" },
		{ 0x011B, "YResolution" },
		{ 0x011C, "PlanarConfiguration" },
		{ 0x011D, "PageName" },
		{ 0x011E, "XPosition" },
		{ 0x011F, "YPosition" },
		{ 0x0120, "FreeOffsets" },
		{ 0x0121, "FreeByteCounts" },
		{ 0x0122, "GrayResponseUnit" },
		{ 0x0123, "GrayResponseCurve" },
		{ 0x0124, "T4Options" },
		{ 0x0125, "T6Options" },
		{ 0x0128, "ResolutionUnit" },
		{ 0x0131, "Software" },
		{ 0x0132, "DateTime" },
		{ 0x013B, "Artist" },
		{ 0x013C, "HostComputer" },
		{ 0x013D, "Predictor" },
		{ 0x013E, "WhitePoint" },
		{ 0x013F, "PrimaryChromaticities" },
		{ 0x0140, "ColorMap"},
		{ 0x0141, "HalftoneHints" },
		{ 0x0142, "TileWidth" },
		{ 0x0143, "TileLength" },
		{ 0x0144, "TileOffset" },
		{ 0x0145, "TileByteCounts" },
		{ 0x0146, "BadFaxLines" },
		{ 0x0147, "CleanFaxData" },
		{ 0x0148, "ConsecutiveBadFaxLines" },
		{ 0x014A, "SubIFD" },
		{ 0x014C, "InkSet" },
		{ 0x014D, "InkNames" },
		{ 0x014E, "NumberofInks" },
		{ 0x0150, "DotRange" },
		{ 0x0151, "TargetPrinter" },
		{ 0x0152, "ExtraSamples" },
		{ 0x0153, "SampleFormat" },
		{ 0x0154, "SMinSampleValue" },
		{ 0x0155, "SMaxSampleValue" },
		{ 0x0156, "TransferRange" },
		{ 0x0157, "ClipPath" },
		{ 0x0158, "XClipPathUnits" },
		{ 0x0159, "YClipPathUnits" },
		{ 0x015A, "Indexed" },
		{ 0x015B, "JPEGTables" },
		{ 0x015F, "OPIProxy" },
		{ 0x0201, "JPEGInterchangeFormat" },
		{ 0x0202, "JPEGInterchangeFormatLength" },
		{ 0x0211, "YCbCrCoefficients" },
		{ 0x0212, "YCbCrSubSampling" },
		{ 0x0213, "YCbCrPositioning" },
		{ 0x0214, "ReferenceBlackWhite" },
		{ 0x8298, "Copyright" },
		{ 0x8769, "ExifIFDPointer" },
		{ 0x8825, "GPSInfoIFDPointer" },

	};

	public static readonly Dictionary<UInt16, string> ExifIfdTags = new()
	{
		{ 0x829A, "ExposureTime" },
		{ 0x829D, "FNumber" },
		{ 0x8822, "ExposureProgram" },
		{ 0x8827, "ISOSpeedRatings" },
		{ 0x9003, "DateTimeOriginal" },
		{ 0x9004, "DateTimeDigitized" },
		{ 0x9201, "ShutterSpeedValue" },
		{ 0x9202, "ApertureValue" },
		{ 0x9203, "BrightnessValue" },
		{ 0x9204, "ExposureBiasValue" },
		{ 0x9205, "MaxApertureValue" },
		{ 0x9206, "SubjectDistance" },
		{ 0x9207, "MeteringMode" },
		{ 0x9209, "Flash" },
		{ 0x920A, "FocalLength" },
		{ 0x9214, "SubjectArea" },
		{ 0x9290, "SubSecTime" },
		{ 0x9291, "SubSecTimeOriginal" },
		{ 0x9292, "SubSecTimeDigitized" },
		{ 0xA000, "FlashpixVersion" },
		{ 0xA001, "ColorSpace" },
		{ 0xA002, "PixelXDimension" },
		{ 0xA003, "PixelYDimension" },
		{ 0xA20E, "FocalPlaneXResolution" },
		{ 0xA20F, "FocalPlaneYResolution" },
		{ 0xA217, "SensingMethod" },
		{ 0xA300, "FileSource" },
		{ 0xA301, "SceneType" },
		{ 0xA401, "CustomRendered" },
		{ 0xA402, "ExposureMode" },
		{ 0xA403, "WhiteBalance" },
		{ 0xA404, "DigitalZoomRatio" },
		{ 0xA405, "FocalLengthIn35mmFilm" },
		{ 0xA406, "SceneCaptureType" },
		{ 0xA407, "GainControl" },
		{ 0xA408, "Contrast" },
		{ 0xA409, "Saturation" },
		{ 0xA40A, "Sharpness" },
		{ 0xA40C, "SubjectDistanceRange" },
		{ 0xA431, "BodySerialNumber" },
		{ 0xA432, "LensSpecification" },
		{ 0xA433, "LensMake" },
		{ 0xA434, "LensModel" },
		{ 0xA435, "LensSerialNumber" },
	};

	public static readonly Dictionary<UInt16, string> GpsTags = new()
	{
		{ 0x0000, "GPSVersionID" },
		{ 0x0001, "GPSLatitudeRef" },
		{ 0x0002, "GPSLatitude" },
		{ 0x0003, "GPSLongitudeRef" },
		{ 0x0004, "GPSLongitude" },
		{ 0x0005, "GPSAltitudeRef" },
		{ 0x0006, "GPSAltitude" },
		{ 0x0007, "GPSTimeStamp" },
		{ 0x001D, "GPSDateStamp" },
	};

	private static readonly Dictionary<Int32, string> OrientationValues = new()
	{
		{ 1, "Top-left" },
		{ 2, "Top-right" },
		{ 3, "Bottom-right" },
		{ 4, "Bottom-left" },
		{ 5, "Left-top" },
		{ 6, "Right-top" },
		{ 7, "Right-bottom" },
		{ 8, "Left-bottom" },
	};

	private static readonly Dictionary<Int32, string> ExposureProgramValues = new()
	{
		{ 0, "Not defined" },
		{ 1, "Manual" },
		{ 2, "Normal program" },
		{ 3, "Aperture priority" },
		{ 4, "Shutter priority" },
		{ 5, "Creative program" },
		{ 6, "Action program" },
		{ 7, "Portrait mode" },
		{ 8, "Landscape mode" },
	};

	private static readonly Dictionary<Int32, string> MeteringModeValues = new()
	{
		{ 0, "Unknown" },
		{ 1, "Average" },
		{ 2, "Center-weighted" },
		{ 3, "Spot" },
		{ 4, "Multi-spot" },
		{ 5, "Pattern" },
		{ 6, "Partial" },
		{ 255, "Other" },
	};

	private static readonly Dictionary<Int32, string> FlashValues = new()
	{
		{ 0x0000, "Flash did not fire" },
		{ 0x0001, "Flash fired" },
		{ 0x0005, "Strobe return light not detected" },
		{ 0x0007, "Strobe return light detected" },
		{ 0x0009, "Flash fired, compulsory mode" },
		{ 0x0010, "Flash did not fire, compulsory mode" },
		{ 0x0018, "Flash did not fire, auto mode" },
		{ 0x0019, "Flash fired, auto mode" },
	};

	private static readonly Dictionary<Int32, string> ColorSpaceValues = new()
	{
		{ 1, "sRGB" },
		{ 0xFFFF, "Uncalibrated" },
	};

	public static Int32 GetTypeSize(UInt16 fieldType)
	{
		return TypeSizes.TryGetValue(fieldType, out Int32 size) ? size : 1;
	}

	public static string GetTypeName(UInt16 fieldType)
	{
		return TypeNames.TryGetValue(fieldType, out string? name) ? name : $"Type {fieldType}";
	}

	public static string GetTagName(IfdType ifdType, UInt16 tag)
	{
		return GetTagMap(ifdType).TryGetValue(tag, out string? name)
			? name
			: $"Unknown (0x{tag:X4})";
	}

	public static string GetTagDescription(IfdType ifdType, UInt16 tag)
	{
		return GetTagName(ifdType, tag);
	}

	public static string? GetEnumeratedValueName(IfdType ifdType, UInt16 tag, Int32 value)
	{
		if (ifdType == IfdType.IFD0 && tag == 0x0112)
			return OrientationValues.TryGetValue(value, out string? orientationName) ? orientationName : null;

		if (ifdType == IfdType.ExifIFD && tag == 0x8822)
			return ExposureProgramValues.TryGetValue(value, out string? exposureProgramName) ? exposureProgramName : null;

		if (ifdType == IfdType.ExifIFD && tag == 0x9207)
			return MeteringModeValues.TryGetValue(value, out string? meteringModeName) ? meteringModeName : null;

		if (ifdType == IfdType.ExifIFD && tag == 0x9209)
			return FlashValues.TryGetValue(value, out string? flashName) ? flashName : null;

		if (ifdType == IfdType.ExifIFD && tag == 0xA001)
			return ColorSpaceValues.TryGetValue(value, out string? colorSpaceName) ? colorSpaceName : null;

		return null;
	}

	private static Dictionary<UInt16, string> GetTagMap(IfdType ifdType)
	{
		return ifdType switch
		{
			IfdType.IFD0 => Ifd0Tags,
			IfdType.ExifIFD => ExifIfdTags,
			IfdType.GPSIFD => GpsTags,
			IfdType.IFD1 => Ifd0Tags,
			_ => Ifd0Tags,
		};
	}
}
