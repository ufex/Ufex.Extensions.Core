using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.EXIF.Data;

public readonly record struct ExifRational(UInt32 Numerator, UInt32 Denominator)
{
	public Double ToDouble()
	{
		if (Denominator == 0)
			return 0;
		return (Double)Numerator / Denominator;
	}

	public override string ToString() => Denominator == 0 ? "0" : $"{Numerator}/{Denominator}";
}

public class IfdEntry
{
	public long Offset { get; init; }
	public UInt16 Tag { get; init; }
	public UInt16 FieldType { get; init; }
	public UInt32 Count { get; init; }
	public UInt32 ValueOffset { get; init; }
	public byte[] InlineValue { get; init; } = [];
	public byte[] RawValue { get; set; } = [];

	public string GetTypeName() => ExifConstants.GetTypeName(FieldType);

	public string GetTagName(IfdType ifdType) => ExifConstants.GetTagName(ifdType, Tag);

	public string GetFormattedValue(IfdType ifdType, Endian endian)
	{
		if (RawValue.Length == 0)
			return "";

		if (FieldType == ExifConstants.TypeAscii)
			return GetAsciiString();

		if (FieldType == ExifConstants.TypeRational && Count == 1)
		{
			ExifRational[] rationals = GetRationals(endian);
			if (rationals.Length == 1)
			{
				ExifRational value = rationals[0];
				if (ifdType == IfdType.ExifIFD && Tag == 0x829A)
					return $"{value} s";
				if (ifdType == IfdType.ExifIFD && Tag == 0x829D)
					return $"f/{value.ToDouble():0.0#}";
				if (ifdType == IfdType.ExifIFD && Tag == 0x920A)
					return $"{value.ToDouble():0.0#} mm";
				return value.ToString();
			}
		}

		Int32? enumValue = GetFirstIntValue(endian);
		if (enumValue.HasValue)
		{
			string? enumName = ExifConstants.GetEnumeratedValueName(ifdType, Tag, enumValue.Value);
			if (!String.IsNullOrWhiteSpace(enumName))
				return $"{enumValue.Value} ({enumName})";
		}

		return FieldType switch
		{
			ExifConstants.TypeByte or ExifConstants.TypeUndefined => FormatByteValues(),
			ExifConstants.TypeShort => FormatUInt16Values(endian),
			ExifConstants.TypeLong => FormatUInt32Values(endian),
			ExifConstants.TypeRational => String.Join(", ", GetRationals(endian).Select(r => r.ToString())),
			ExifConstants.TypeSShort => FormatInt16Values(endian),
			ExifConstants.TypeSLong => FormatInt32Values(endian),
			ExifConstants.TypeSRational => FormatSignedRationalValues(endian),
			_ => FormatByteValues(),
		};
	}

	public string GetAsciiString()
	{
		if (RawValue.Length == 0)
			return String.Empty;

		Int32 nullIndex = Array.IndexOf(RawValue, (Byte)0);
		Int32 len = nullIndex >= 0 ? nullIndex : RawValue.Length;
		return Encoding.ASCII.GetString(RawValue, 0, len);
	}

	public UInt32 GetFirstUInt32Value(Endian endian)
	{
		if (FieldType == ExifConstants.TypeShort)
		{
			UInt16[] values = GetUInt16Values(endian);
			return values.Length > 0 ? values[0] : 0u;
		}

		if (FieldType == ExifConstants.TypeLong)
		{
			UInt32[] values = GetUInt32Values(endian);
			return values.Length > 0 ? values[0] : 0u;
		}

		return 0u;
	}

	public UInt16[] GetUInt16Values(Endian endian)
	{
		if (RawValue.Length < 2)
			return [];

		Int32 count = RawValue.Length / 2;
		UInt16[] values = new UInt16[count];
		for (Int32 i = 0; i < count; i++)
		{
			values[i] = ReadUInt16(RawValue, i * 2, endian);
		}
		return values;
	}

	public UInt32[] GetUInt32Values(Endian endian)
	{
		if (RawValue.Length < 4)
			return [];

		Int32 count = RawValue.Length / 4;
		UInt32[] values = new UInt32[count];
		for (Int32 i = 0; i < count; i++)
		{
			values[i] = ReadUInt32(RawValue, i * 4, endian);
		}
		return values;
	}

	public ExifRational[] GetRationals(Endian endian)
	{
		if (RawValue.Length < 8)
			return [];

		Int32 count = RawValue.Length / 8;
		ExifRational[] values = new ExifRational[count];
		for (Int32 i = 0; i < count; i++)
		{
			Int32 o = i * 8;
			UInt32 numerator = ReadUInt32(RawValue, o, endian);
			UInt32 denominator = ReadUInt32(RawValue, o + 4, endian);
			values[i] = new ExifRational(numerator, denominator);
		}
		return values;
	}

	private Int32? GetFirstIntValue(Endian endian)
	{
		if (FieldType == ExifConstants.TypeByte && RawValue.Length >= 1)
			return RawValue[0];

		if ((FieldType == ExifConstants.TypeShort || FieldType == ExifConstants.TypeSShort) && RawValue.Length >= 2)
			return FieldType == ExifConstants.TypeShort ? ReadUInt16(RawValue, 0, endian) : ReadInt16(RawValue, 0, endian);

		if ((FieldType == ExifConstants.TypeLong || FieldType == ExifConstants.TypeSLong) && RawValue.Length >= 4)
			return FieldType == ExifConstants.TypeLong ? (Int32)ReadUInt32(RawValue, 0, endian) : ReadInt32(RawValue, 0, endian);

		return null;
	}

	private string FormatByteValues()
	{
		if (RawValue.Length == 1)
			return RawValue[0].ToString();
		return String.Join(" ", RawValue.Select(b => $"{b:X2}"));
	}

	private string FormatUInt16Values(Endian endian)
	{
		UInt16[] values = GetUInt16Values(endian);
		if (values.Length == 1)
			return values[0].ToString();
		return String.Join(", ", values);
	}

	private string FormatUInt32Values(Endian endian)
	{
		UInt32[] values = GetUInt32Values(endian);
		if (values.Length == 1)
			return values[0].ToString();
		return String.Join(", ", values);
	}

	private string FormatInt16Values(Endian endian)
	{
		if (RawValue.Length < 2)
			return String.Empty;

		Int32 count = RawValue.Length / 2;
		Int16[] values = new Int16[count];
		for (Int32 i = 0; i < count; i++)
		{
			values[i] = ReadInt16(RawValue, i * 2, endian);
		}

		if (values.Length == 1)
			return values[0].ToString();
		return String.Join(", ", values);
	}

	private string FormatInt32Values(Endian endian)
	{
		if (RawValue.Length < 4)
			return String.Empty;

		Int32 count = RawValue.Length / 4;
		Int32[] values = new Int32[count];
		for (Int32 i = 0; i < count; i++)
		{
			values[i] = ReadInt32(RawValue, i * 4, endian);
		}

		if (values.Length == 1)
			return values[0].ToString();
		return String.Join(", ", values);
	}

	private string FormatSignedRationalValues(Endian endian)
	{
		if (RawValue.Length < 8)
			return String.Empty;

		Int32 count = RawValue.Length / 8;
		List<string> values = [];
		for (Int32 i = 0; i < count; i++)
		{
			Int32 o = i * 8;
			Int32 numerator = ReadInt32(RawValue, o, endian);
			Int32 denominator = ReadInt32(RawValue, o + 4, endian);
			if (denominator == 0)
			{
				values.Add("0");
			}
			else
			{
				values.Add($"{numerator}/{denominator}");
			}
		}

		return values.Count == 1 ? values[0] : String.Join(", ", values);
	}

	private static UInt16 ReadUInt16(byte[] data, Int32 offset, Endian endian)
	{
		return endian == Endian.Little
			? (UInt16)(data[offset] | (data[offset + 1] << 8))
			: (UInt16)(data[offset + 1] | (data[offset] << 8));
	}

	private static Int16 ReadInt16(byte[] data, Int32 offset, Endian endian)
	{
		return unchecked((Int16)ReadUInt16(data, offset, endian));
	}

	private static UInt32 ReadUInt32(byte[] data, Int32 offset, Endian endian)
	{
		return endian == Endian.Little
			? (UInt32)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24))
			: (UInt32)(data[offset + 3] | (data[offset + 2] << 8) | (data[offset + 1] << 16) | (data[offset] << 24));
	}

	private static Int32 ReadInt32(byte[] data, Int32 offset, Endian endian)
	{
		return unchecked((Int32)ReadUInt32(data, offset, endian));
	}
}
