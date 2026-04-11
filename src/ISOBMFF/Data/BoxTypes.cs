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

		// 3GPP metadata
		{ "titl", "Title (3GPP)" },
		{ "dscp", "Description (3GPP)" },
		{ "cprt", "Copyright (3GPP)" },
		{ "perf", "Performer (3GPP)" },
		{ "auth", "Author (3GPP)" },
		{ "gnre", "Genre (3GPP)" },
		{ "rtng", "Rating (3GPP)" },
		{ "clsf", "Classification (3GPP)" },
		{ "kywd", "Keywords (3GPP)" },
		{ "loci", "Location Information (3GPP)" },
		{ "albm", "Album (3GPP)" },
		{ "yrrc", "Recording Year (3GPP)" },
		{ "tsel", "Track Selection (3GPP)" },
		{ "strk", "Sub Track (3GPP)" },
		{ "stri", "Sub Track Information (3GPP)" },
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
		{ "3gp7", "3GPP Release 7" },
		{ "3gp8", "3GPP Release 8" },
		{ "3gp9", "3GPP Release 9" },
		{ "3ge6", "3GPP Release 6 Extended" },
		{ "3ge7", "3GPP Release 7 Extended" },
		{ "3ge9", "3GPP Release 9 Extended" },
		{ "3gg6", "3GPP Release 6 Streaming" },
		{ "3gs6", "3GPP Release 6 Progressive" },
		{ "3gr6", "3GPP Release 6 Adaptive" },
		{ "3g2a", "3GPP2" },
		{ "3g2b", "3GPP2 v2" },
		{ "3g2c", "3GPP2 v3" },
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
