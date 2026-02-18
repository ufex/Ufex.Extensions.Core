using Ufex.API;

namespace Ufex.Extensions.Core.GIF.Data;

/// <summary>
/// GIF Header - First 6 bytes of the file
/// Contains signature ("GIF") and version ("87a" or "89a")
/// </summary>
internal class Header : GifBlock
{
	/// <summary>
	/// Signature bytes - should be "GIF"
	/// </summary>
	public string Signature { get; init; }

	/// <summary>
	/// Version string - "87a" or "89a"
	/// </summary>
	public string Version { get; init; }

	/// <summary>
	/// Whether this is GIF87a format
	/// </summary>
	public bool IsGif87a => Version == "87a";

	/// <summary>
	/// Whether this is GIF89a format
	/// </summary>
	public bool IsGif89a => Version == "89a";

	public Header(FileReader fr) : base(fr)
	{
		Signature = new string(fr.ReadChars(3));
		Version = new string(fr.ReadChars(3));
	}
}
