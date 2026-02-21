namespace Ufex.Extensions.Core.PDF.Data;

/// <summary>
/// Represents a PDF object value. PDF has these basic types:
/// Null, Boolean, Integer, Real, Name, String, Array, Dictionary, Stream, Reference.
/// </summary>
internal abstract class PdfObject
{
	public abstract PdfObjectType ObjectType { get; }

	public override string ToString() => $"[{ObjectType}]";
}

internal enum PdfObjectType
{
	Null,
	Boolean,
	Integer,
	Real,
	Name,
	String,
	HexString,
	Array,
	Dictionary,
	Stream,
	Reference
}

internal class PdfNull : PdfObject
{
	public static readonly PdfNull Instance = new();
	public override PdfObjectType ObjectType => PdfObjectType.Null;
	public override string ToString() => "null";
}

internal class PdfBoolean : PdfObject
{
	public bool Value { get; }
	public override PdfObjectType ObjectType => PdfObjectType.Boolean;
	public PdfBoolean(bool value) => Value = value;
	public override string ToString() => Value ? "true" : "false";
}

internal class PdfInteger : PdfObject
{
	public long Value { get; }
	public override PdfObjectType ObjectType => PdfObjectType.Integer;
	public PdfInteger(long value) => Value = value;
	public override string ToString() => Value.ToString();
}

internal class PdfReal : PdfObject
{
	public double Value { get; }
	public override PdfObjectType ObjectType => PdfObjectType.Real;
	public PdfReal(double value) => Value = value;
	public override string ToString() => Value.ToString("G");
}

internal class PdfName : PdfObject
{
	public string Value { get; }
	public override PdfObjectType ObjectType => PdfObjectType.Name;
	public PdfName(string value) => Value = value;
	public override string ToString() => "/" + Value;
}

internal class PdfString : PdfObject
{
	public byte[] RawBytes { get; }
	public override PdfObjectType ObjectType => PdfObjectType.String;
	public PdfString(byte[] rawBytes) => RawBytes = rawBytes;

	public string TextValue
	{
		get
		{
			// Check for UTF-16BE BOM
			if (RawBytes.Length >= 2 && RawBytes[0] == 0xFE && RawBytes[1] == 0xFF)
				return System.Text.Encoding.BigEndianUnicode.GetString(RawBytes, 2, RawBytes.Length - 2);
			return System.Text.Encoding.Latin1.GetString(RawBytes);
		}
	}

	public override string ToString()
	{
		if (RawBytes.Length <= 64)
			return "(" + TextValue + ")";
		return $"(string, {RawBytes.Length} bytes)";
	}
}

internal class PdfHexString : PdfObject
{
	public byte[] RawBytes { get; }
	public override PdfObjectType ObjectType => PdfObjectType.HexString;
	public PdfHexString(byte[] rawBytes) => RawBytes = rawBytes;

	public string HexValue => BitConverter.ToString(RawBytes).Replace("-", "");

	public override string ToString()
	{
		if (RawBytes.Length <= 32)
			return "<" + HexValue + ">";
		return $"<hex, {RawBytes.Length} bytes>";
	}
}

internal class PdfArray : PdfObject
{
	public List<PdfObject> Items { get; } = new();
	public override PdfObjectType ObjectType => PdfObjectType.Array;

	public int Count => Items.Count;
	public PdfObject this[int index] => Items[index];

	public override string ToString() => $"[array, {Items.Count} items]";
}

internal class PdfDictionary : PdfObject
{
	public Dictionary<string, PdfObject> Entries { get; } = new();
	public override PdfObjectType ObjectType => PdfObjectType.Dictionary;

	public PdfObject? Get(string key) => Entries.TryGetValue(key, out var val) ? val : null;
	public string? GetName(string key) => Get(key) is PdfName n ? n.Value : null;
	public long? GetInteger(string key) => Get(key) is PdfInteger i ? i.Value : null;
	public double? GetReal(string key) => Get(key) switch
	{
		PdfReal r => r.Value,
		PdfInteger i => i.Value,
		_ => null
	};
	public string? GetString(string key) => Get(key) is PdfString s ? s.TextValue : null;
	public PdfArray? GetArray(string key) => Get(key) as PdfArray;
	public PdfDictionary? GetDictionary(string key) => Get(key) as PdfDictionary;
	public PdfReference? GetReference(string key) => Get(key) as PdfReference;
	public bool ContainsKey(string key) => Entries.ContainsKey(key);

	public override string ToString()
	{
		var type = GetName("Type");
		return type != null ? $"<</{type} dict>>" : $"<<dict, {Entries.Count} entries>>";
	}
}

internal class PdfStream : PdfObject
{
	public PdfDictionary Dict { get; }
	public long DataOffset { get; }
	public long DataLength { get; }
	public byte[]? DecodedData { get; set; }
	public override PdfObjectType ObjectType => PdfObjectType.Stream;

	public PdfStream(PdfDictionary dict, long dataOffset, long dataLength)
	{
		Dict = dict;
		DataOffset = dataOffset;
		DataLength = dataLength;
	}

	public override string ToString() => $"<<stream, {DataLength} bytes>>";
}

internal class PdfReference : PdfObject
{
	public int ObjectNumber { get; }
	public int Generation { get; }
	public override PdfObjectType ObjectType => PdfObjectType.Reference;

	public PdfReference(int objectNumber, int generation)
	{
		ObjectNumber = objectNumber;
		Generation = generation;
	}

	public override string ToString() => $"{ObjectNumber} {Generation} R";
}
