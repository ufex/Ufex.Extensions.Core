using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// Generic APPn marker segment (0xFFE1 through 0xFFEF)
/// Used for application-specific data such as Exif (APP1), ICC profiles (APP2), etc.
/// </summary>
internal class AppNSegment : Segment
{
	/// <summary>
	/// Application identifier string (null-terminated, read from segment data)
	/// </summary>
	public byte[] AppIdentifier { get; init; }

	/// <summary>
	/// Remaining application data after the identifier
	/// </summary>
	public byte[] AppData { get; init; }

	/// <summary>
	/// The application identifier as a string (up to the null terminator)
	/// </summary>
	public string AppIdentifierString
	{
		get
		{
			int nullIndex = Array.IndexOf(AppIdentifier, (byte)0);
			int len = nullIndex >= 0 ? nullIndex : AppIdentifier.Length;
			return System.Text.Encoding.ASCII.GetString(AppIdentifier, 0, len);
		}
	}

	public AppNSegment(FileReader fr) : base(fr)
	{
		// Read the segment data: Length includes itself (2 bytes)
		int dataLength = Length - 2;
		if (dataLength <= 0)
		{
			AppIdentifier = [];
			AppData = [];
			return;
		}

		byte[] allData = fr.ReadBytes(dataLength);

		// Split into identifier (null-terminated string) and remaining data
		int nullIndex = Array.IndexOf(allData, (byte)0);
		if (nullIndex >= 0 && nullIndex < allData.Length - 1)
		{
			AppIdentifier = allData[..(nullIndex + 1)];
			AppData = allData[(nullIndex + 1)..];
		}
		else
		{
			AppIdentifier = allData;
			AppData = [];
		}
	}
}
