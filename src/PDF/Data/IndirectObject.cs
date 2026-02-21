namespace Ufex.Extensions.Core.PDF.Data;

/// <summary>
/// Represents a parsed indirect object from the PDF body (number generation obj ... endobj)
/// </summary>
internal class IndirectObject
{
	/// <summary>
	/// Object number
	/// </summary>
	public int ObjectNumber { get; init; }

	/// <summary>
	/// Generation number
	/// </summary>
	public int Generation { get; init; }

	/// <summary>
	/// Byte offset of the start of this object definition in the file
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// Byte offset of the end of this object definition (after endobj)
	/// </summary>
	public long EndOffset { get; set; }

	/// <summary>
	/// The parsed value of this object
	/// </summary>
	public PdfObject Value { get; init; }

	/// <summary>
	/// The /Type name if this object is a typed dictionary (e.g., "Catalog", "Page", "Font")
	/// </summary>
	public string? TypeName
	{
		get
		{
			if (Value is PdfDictionary dict)
				return dict.GetName("Type");
			if (Value is PdfStream stream)
				return stream.Dict.GetName("Type");
			return null;
		}
	}

	/// <summary>
	/// The /Subtype name if present
	/// </summary>
	public string? SubtypeName
	{
		get
		{
			if (Value is PdfDictionary dict)
				return dict.GetName("Subtype") ?? dict.GetName("S");
			if (Value is PdfStream stream)
				return stream.Dict.GetName("Subtype") ?? stream.Dict.GetName("S");
			return null;
		}
	}

	/// <summary>
	/// Gets the dictionary for this object, whether it's a plain dictionary or a stream
	/// </summary>
	public PdfDictionary? Dictionary
	{
		get
		{
			if (Value is PdfDictionary dict)
				return dict;
			if (Value is PdfStream stream)
				return stream.Dict;
			return null;
		}
	}

	/// <summary>
	/// Short description of this object for display
	/// </summary>
	public string DisplayName
	{
		get
		{
			var type = TypeName;
			var subtype = SubtypeName;
			if (type != null && subtype != null)
				return $"{type}/{subtype}";
			if (type != null)
				return type;
			if (subtype != null)
				return subtype;
			if (Value is PdfStream)
				return "Stream";
			return Value.ObjectType.ToString();
		}
	}

	public IndirectObject(int objectNumber, int generation, long offset, PdfObject value)
	{
		ObjectNumber = objectNumber;
		Generation = generation;
		Offset = offset;
		Value = value;
	}

	public override string ToString() => $"{ObjectNumber} {Generation} obj ({DisplayName})";
}
