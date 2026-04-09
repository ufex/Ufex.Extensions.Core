namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// FourCC constants and descriptions for known box types.
/// </summary>
internal static class BoxTypes
{
	/// <summary>
	/// Human-readable descriptions for known box types.
	/// </summary>
	public static readonly Dictionary<string, string> Descriptions = new()
	{
		// File-level
		{ "ftyp", "File Type" },
		{ "styp", "Segment Type" },
		{ "pdin", "Progressive Download Info" },
		{ "mdat", "Media Data" },
		{ "free", "Free Space" },
		{ "skip", "Free Space" },
		{ "wide", "Wide Placeholder (QTFF)" },

		// Movie
		{ "moov", "Movie Container" },
		{ "mvhd", "Movie Header" },
		{ "udta", "User Data" },

		// Track
		{ "trak", "Track Container" },
		{ "tkhd", "Track Header" },
		{ "edts", "Edit List Container" },
		{ "elst", "Edit List" },
		{ "tref", "Track Reference" },

		// Media
		{ "mdia", "Media Container" },
		{ "mdhd", "Media Header" },
		{ "hdlr", "Handler Reference" },
		{ "minf", "Media Information Container" },

		// Media information headers
		{ "vmhd", "Video Media Header" },
		{ "smhd", "Sound Media Header" },
		{ "hmhd", "Hint Media Header" },
		{ "nmhd", "Null Media Header" },
		{ "sthd", "Subtitle Media Header" },
		{ "gmhd", "Generic Media Header (QTFF)" },

		// Data reference
		{ "dinf", "Data Information Container" },
		{ "dref", "Data Reference" },
		{ "url ", "Data Entry URL" },
		{ "urn ", "Data Entry URN" },
		{ "alis", "Data Entry Alias (QTFF)" },

		// Sample table
		{ "stbl", "Sample Table Container" },
		{ "stsd", "Sample Descriptions" },
		{ "stts", "Decoding Time-to-Sample" },
		{ "ctts", "Composition Time Offset" },
		{ "cslg", "Composition to Decode Timeline" },
		{ "stss", "Sync Sample Table" },
		{ "stsc", "Sample-to-Chunk" },
		{ "stsz", "Sample Sizes" },
		{ "stz2", "Compact Sample Sizes" },
		{ "stco", "Chunk Offsets (32-bit)" },
		{ "co64", "Chunk Offsets (64-bit)" },
		{ "stsh", "Shadow Sync Table (QTFF)" },
		{ "stdp", "Degradation Priority (QTFF)" },
		{ "sdtp", "Independent/Disposable Samples" },
		{ "sbgp", "Sample-to-Group" },
		{ "sgpd", "Sample Group Description" },
		{ "subs", "Sub-Sample Information" },

		// Movie fragments
		{ "mvex", "Movie Extends Container" },
		{ "mehd", "Movie Extends Header" },
		{ "trex", "Track Extends" },
		{ "moof", "Movie Fragment" },
		{ "mfhd", "Movie Fragment Header" },
		{ "traf", "Track Fragment" },
		{ "tfhd", "Track Fragment Header" },
		{ "tfdt", "Track Fragment Decode Time" },
		{ "trun", "Track Fragment Run" },
		{ "mfra", "Movie Fragment Random Access" },
		{ "tfra", "Track Fragment Random Access" },
		{ "mfro", "Movie Fragment Random Access Offset" },

		// Metadata
		{ "meta", "Metadata Container" },
		{ "iinf", "Item Information" },
		{ "infe", "Item Information Entry" },
		{ "iloc", "Item Location" },
		{ "iref", "Item Reference" },
		{ "idat", "Item Data" },
		{ "pitm", "Primary Item" },
		{ "iprp", "Item Properties" },
		{ "ilst", "iTunes Metadata List" },

		// QTFF-only
		{ "clip", "Clipping Region (QTFF)" },
		{ "crgn", "Clipping Region Data (QTFF)" },
		{ "matt", "Track Matte (QTFF)" },
		{ "kmat", "Compressed Matte (QTFF)" },
		{ "load", "Track Load Settings (QTFF)" },
		{ "imap", "Input Map (QTFF)" },
		{ "ctab", "Color Table (QTFF)" },
		{ "prof", "Profile (QTFF)" },
	};

	/// <summary>
	/// Handler type descriptions.
	/// </summary>
	public static readonly Dictionary<string, string> HandlerTypes = new()
	{
		{ "vide", "Video" },
		{ "soun", "Sound" },
		{ "hint", "Hint" },
		{ "meta", "Metadata" },
		{ "text", "Text" },
		{ "subt", "Subtitle" },
		{ "tmcd", "Timecode" },
		{ "auxv", "Auxiliary Video" },
		{ "mdta", "Metadata Tags" },
		{ "mdir", "iTunes Metadata" },
	};

	/// <summary>
	/// Known major brand descriptions.
	/// </summary>
	public static readonly Dictionary<string, string> Brands = new()
	{
		{ "isom", "ISO Base Media File" },
		{ "iso2", "ISO Base Media File v2" },
		{ "iso3", "ISO Base Media File v3" },
		{ "iso4", "ISO Base Media File v4" },
		{ "iso5", "ISO Base Media File v5" },
		{ "iso6", "ISO Base Media File v6" },
		{ "mp41", "MP4 v1" },
		{ "mp42", "MP4 v2" },
		{ "mp71", "MP4 for MPEG-7" },
		{ "M4A ", "Apple M4A Audio" },
		{ "M4B ", "Apple M4B Audiobook" },
		{ "M4P ", "Apple M4P Protected" },
		{ "M4V ", "Apple M4V Video" },
		{ "qt  ", "Apple QuickTime MOV" },
		{ "3gp4", "3GPP Release 4" },
		{ "3gp5", "3GPP Release 5" },
		{ "3gp6", "3GPP Release 6" },
		{ "3g2a", "3GPP2" },
		{ "avc1", "AVC/H.264" },
		{ "heic", "HEIF Image (HEVC)" },
		{ "heix", "HEIF Image (Extended)" },
		{ "mif1", "HEIF Image" },
		{ "msf1", "HEIF Image Sequence" },
		{ "avif", "AV1 Image" },
		{ "f4v ", "Adobe Flash Video" },
		{ "f4p ", "Adobe Flash Protected" },
		{ "dash", "MPEG-DASH" },
		{ "cmf2", "CMAF" },
		{ "cmfc", "CMAF (Constrained)" },
	};
}
