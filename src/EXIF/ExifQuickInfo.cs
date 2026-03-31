using Ufex.API.Tables;
using Ufex.Extensions.Core.EXIF.Data;

namespace Ufex.Extensions.Core.EXIF;

public static class ExifQuickInfo
{
	public static void Populate(QuickInfoTableData table, ExifData exifData)
	{
		AddIfPresent(table, "Camera Make", exifData.GetEntry(IfdType.IFD0, 0x010F), IfdType.IFD0, exifData);
		AddIfPresent(table, "Camera Model", exifData.GetEntry(IfdType.IFD0, 0x0110), IfdType.IFD0, exifData);
		AddIfPresent(table, "Date/Time", exifData.GetEntry(IfdType.IFD0, 0x0132), IfdType.IFD0, exifData);
		AddIfPresent(table, "Software", exifData.GetEntry(IfdType.IFD0, 0x0131), IfdType.IFD0, exifData);
		AddIfPresent(table, "Orientation", exifData.GetEntry(IfdType.IFD0, 0x0112), IfdType.IFD0, exifData);
		AddIfPresent(table, "Artist", exifData.GetEntry(IfdType.IFD0, 0x013B), IfdType.IFD0, exifData);

		AddIfPresent(table, "Date Taken", exifData.GetEntry(IfdType.ExifIFD, 0x9003), IfdType.ExifIFD, exifData);
		AddIfPresent(table, "Exposure Time", exifData.GetEntry(IfdType.ExifIFD, 0x829A), IfdType.ExifIFD, exifData);
		AddIfPresent(table, "F Number", exifData.GetEntry(IfdType.ExifIFD, 0x829D), IfdType.ExifIFD, exifData);
		AddIfPresent(table, "ISO", exifData.GetEntry(IfdType.ExifIFD, 0x8827), IfdType.ExifIFD, exifData);
		AddIfPresent(table, "Focal Length", exifData.GetEntry(IfdType.ExifIFD, 0x920A), IfdType.ExifIFD, exifData);
		AddIfPresent(table, "Lens Model", exifData.GetEntry(IfdType.ExifIFD, 0xA434), IfdType.ExifIFD, exifData);
		AddIfPresent(table, "Flash", exifData.GetEntry(IfdType.ExifIFD, 0x9209), IfdType.ExifIFD, exifData);

		string? latitude = GetGpsCoordinate(exifData, 0x0001, 0x0002);
		if (!String.IsNullOrWhiteSpace(latitude))
			table.AddRow("GPS Latitude", latitude);

		string? longitude = GetGpsCoordinate(exifData, 0x0003, 0x0004);
		if (!String.IsNullOrWhiteSpace(longitude))
			table.AddRow("GPS Longitude", longitude);
	}

	private static void AddIfPresent(QuickInfoTableData table, string label, IfdEntry? entry, IfdType ifdType, ExifData exifData)
	{
		if (entry == null)
			return;

		string value = entry.GetFormattedValue(ifdType, exifData.ByteOrder);
		if (String.IsNullOrWhiteSpace(value))
			return;

		table.AddRow(label, value);
	}

	private static string? GetGpsCoordinate(ExifData exifData, UInt16 refTag, UInt16 valueTag)
	{
		IfdEntry? refEntry = exifData.GetEntry(IfdType.GPSIFD, refTag);
		IfdEntry? valueEntry = exifData.GetEntry(IfdType.GPSIFD, valueTag);

		if (refEntry == null || valueEntry == null)
			return null;

		string direction = refEntry.GetAsciiString().Trim();
		ExifRational[] values = valueEntry.GetRationals(exifData.ByteOrder);
		if (values.Length < 3)
			return null;

		Double degrees = values[0].ToDouble();
		Double minutes = values[1].ToDouble();
		Double seconds = values[2].ToDouble();
		return $"{degrees:0}° {minutes:0}' {seconds:0.##}\" {direction}";
	}
}
