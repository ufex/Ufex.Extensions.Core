using System.Text;

namespace Ufex.Extensions.Core.PDF;

/// <summary>
/// Token types produced by the PDF lexer
/// </summary>
internal enum PdfTokenType
{
	/// <summary>End of data</summary>
	Eof,
	/// <summary>Integer or real number</summary>
	Number,
	/// <summary>PDF name object (starts with /)</summary>
	Name,
	/// <summary>Literal string in parentheses</summary>
	LiteralString,
	/// <summary>Hex string in angle brackets</summary>
	HexString,
	/// <summary>Keyword: true, false, null, obj, endobj, stream, endstream, xref, trailer, startxref, R, f, n</summary>
	Keyword,
	/// <summary>Array begin [</summary>
	ArrayBegin,
	/// <summary>Array end ]</summary>
	ArrayEnd,
	/// <summary>Dictionary begin &lt;&lt;</summary>
	DictBegin,
	/// <summary>Dictionary end &gt;&gt;</summary>
	DictEnd,
}

/// <summary>
/// A token produced by the PDF lexer
/// </summary>
internal readonly struct PdfToken
{
	public PdfTokenType Type { get; init; }
	public string Value { get; init; }
	public byte[]? Bytes { get; init; }
	public long Position { get; init; }

	public PdfToken(PdfTokenType type, string value, long position)
	{
		Type = type;
		Value = value;
		Bytes = null;
		Position = position;
	}

	public PdfToken(PdfTokenType type, byte[] bytes, long position)
	{
		Type = type;
		Value = "";
		Bytes = bytes;
		Position = position;
	}

	public override string ToString() => Type == PdfTokenType.LiteralString || Type == PdfTokenType.HexString
		? $"{Type}[{Bytes?.Length ?? 0} bytes]@{Position}"
		: $"{Type}[{Value}]@{Position}";
}

/// <summary>
/// Low-level PDF tokenizer/lexer. Operates on a byte array with a movable position cursor.
/// Handles PDF whitespace, comments, names, strings, hex strings, numbers, keywords, and delimiters.
/// </summary>
internal class PdfTokenizer
{
	private readonly byte[] _data;
	private int _pos;

	/// <summary>Current position in the byte array</summary>
	public int Position
	{
		get => _pos;
		set => _pos = Math.Clamp(value, 0, _data.Length);
	}

	/// <summary>Total length of data</summary>
	public int Length => _data.Length;

	/// <summary>Whether the current position is at or past the end of data</summary>
	public bool IsEof => _pos >= _data.Length;

	public PdfTokenizer(byte[] data)
	{
		_data = data;
		_pos = 0;
	}

	/// <summary>
	/// Skips whitespace and comment characters.
	/// PDF whitespace: NUL(0), TAB(9), LF(10), FF(12), CR(13), SP(32)
	/// Comments: % through end of line
	/// </summary>
	public void SkipWhitespaceAndComments()
	{
		while (_pos < _data.Length)
		{
			byte b = _data[_pos];
			if (IsWhitespace(b))
			{
				_pos++;
			}
			else if (b == (byte)'%')
			{
				// Skip comment to end of line
				_pos++;
				while (_pos < _data.Length && _data[_pos] != 10 && _data[_pos] != 13)
					_pos++;
			}
			else
			{
				break;
			}
		}
	}

	/// <summary>
	/// Reads the next token from the current position.
	/// </summary>
	public PdfToken ReadToken()
	{
		SkipWhitespaceAndComments();

		if (_pos >= _data.Length)
			return new PdfToken(PdfTokenType.Eof, "", _pos);

		long tokenStart = _pos;
		byte b = _data[_pos];

		// Name: /something
		if (b == (byte)'/')
			return ReadName(tokenStart);

		// Literal string: (...)
		if (b == (byte)'(')
			return ReadLiteralString(tokenStart);

		// Hex string or dict delimiter: < or <<
		if (b == (byte)'<')
		{
			if (_pos + 1 < _data.Length && _data[_pos + 1] == (byte)'<')
			{
				_pos += 2;
				return new PdfToken(PdfTokenType.DictBegin, "<<", tokenStart);
			}
			return ReadHexString(tokenStart);
		}

		// Dict end: >>
		if (b == (byte)'>')
		{
			if (_pos + 1 < _data.Length && _data[_pos + 1] == (byte)'>')
			{
				_pos += 2;
				return new PdfToken(PdfTokenType.DictEnd, ">>", tokenStart);
			}
			// Stray > — treat as keyword/error token
			_pos++;
			return new PdfToken(PdfTokenType.Keyword, ">", tokenStart);
		}

		// Array delimiters
		if (b == (byte)'[')
		{
			_pos++;
			return new PdfToken(PdfTokenType.ArrayBegin, "[", tokenStart);
		}
		if (b == (byte)']')
		{
			_pos++;
			return new PdfToken(PdfTokenType.ArrayEnd, "]", tokenStart);
		}

		// Number: +, -, digit, or .digit
		if (b == (byte)'+' || b == (byte)'-' || IsDigit(b) || (b == (byte)'.' && _pos + 1 < _data.Length && IsDigit(_data[_pos + 1])))
			return ReadNumber(tokenStart);

		// Regular characters → keyword
		if (IsRegular(b))
			return ReadKeyword(tokenStart);

		// Unknown byte — skip it and try again
		_pos++;
		return ReadToken();
	}

	/// <summary>
	/// Peeks at the next token without consuming it.
	/// </summary>
	public PdfToken PeekToken()
	{
		int savedPos = _pos;
		var token = ReadToken();
		_pos = savedPos;
		return token;
	}

	/// <summary>
	/// Reads the current byte without advancing.
	/// </summary>
	public byte? PeekByte() => _pos < _data.Length ? _data[_pos] : null;

	/// <summary>
	/// Reads a single byte and advances position.
	/// </summary>
	public byte ReadByte() => _data[_pos++];

	/// <summary>
	/// Reads a range of bytes without advancing position.
	/// </summary>
	public byte[] GetBytes(int offset, int count)
	{
		count = Math.Min(count, _data.Length - offset);
		if (count <= 0) return [];
		var result = new byte[count];
		Array.Copy(_data, offset, result, 0, count);
		return result;
	}

	#region Token readers

	private PdfToken ReadName(long tokenStart)
	{
		_pos++; // skip /
		var sb = new StringBuilder();
		while (_pos < _data.Length)
		{
			byte c = _data[_pos];
			if (IsWhitespace(c) || IsDelimiter(c))
				break;
			if (c == (byte)'#' && _pos + 2 < _data.Length)
			{
				// Hex-encoded byte: #XX
				int hi = HexDigit(_data[_pos + 1]);
				int lo = HexDigit(_data[_pos + 2]);
				if (hi >= 0 && lo >= 0)
				{
					sb.Append((char)((hi << 4) | lo));
					_pos += 3;
					continue;
				}
			}
			sb.Append((char)c);
			_pos++;
		}
		return new PdfToken(PdfTokenType.Name, sb.ToString(), tokenStart);
	}

	private PdfToken ReadLiteralString(long tokenStart)
	{
		_pos++; // skip (
		var bytes = new List<byte>();
		int depth = 1;
		while (_pos < _data.Length && depth > 0)
		{
			byte c = _data[_pos];
			if (c == (byte)'(')
			{
				depth++;
				bytes.Add(c);
				_pos++;
			}
			else if (c == (byte)')')
			{
				depth--;
				if (depth > 0)
				{
					bytes.Add(c);
				}
				_pos++;
			}
			else if (c == (byte)'\\')
			{
				_pos++;
				if (_pos >= _data.Length) break;
				byte escaped = _data[_pos];
				switch (escaped)
				{
					case (byte)'n': bytes.Add(10); _pos++; break;
					case (byte)'r': bytes.Add(13); _pos++; break;
					case (byte)'t': bytes.Add(9); _pos++; break;
					case (byte)'b': bytes.Add(8); _pos++; break;
					case (byte)'f': bytes.Add(12); _pos++; break;
					case (byte)'(': bytes.Add((byte)'('); _pos++; break;
					case (byte)')': bytes.Add((byte)')'); _pos++; break;
					case (byte)'\\': bytes.Add((byte)'\\'); _pos++; break;
					case 13: // \CR or \CRLF → line continuation
						_pos++;
						if (_pos < _data.Length && _data[_pos] == 10)
							_pos++;
						break;
					case 10: // \LF → line continuation
						_pos++;
						break;
					default:
						// Octal escape: 1-3 digits
						if (escaped >= (byte)'0' && escaped <= (byte)'7')
						{
							int octal = escaped - '0';
							_pos++;
							if (_pos < _data.Length && _data[_pos] >= (byte)'0' && _data[_pos] <= (byte)'7')
							{
								octal = (octal << 3) | (_data[_pos] - '0');
								_pos++;
								if (_pos < _data.Length && _data[_pos] >= (byte)'0' && _data[_pos] <= (byte)'7')
								{
									octal = (octal << 3) | (_data[_pos] - '0');
									_pos++;
								}
							}
							bytes.Add((byte)(octal & 0xFF));
						}
						else
						{
							// Unknown escape — just use the character
							bytes.Add(escaped);
							_pos++;
						}
						break;
				}
			}
			else
			{
				bytes.Add(c);
				_pos++;
			}
		}
		return new PdfToken(PdfTokenType.LiteralString, bytes.ToArray(), tokenStart);
	}

	private PdfToken ReadHexString(long tokenStart)
	{
		_pos++; // skip <
		var hexChars = new List<byte>();
		while (_pos < _data.Length)
		{
			byte c = _data[_pos];
			if (c == (byte)'>')
			{
				_pos++;
				break;
			}
			if (IsWhitespace(c))
			{
				_pos++;
				continue;
			}
			int digit = HexDigit(c);
			if (digit >= 0)
			{
				hexChars.Add((byte)digit);
			}
			_pos++;
		}
		// If odd number of hex digits, append a trailing 0
		if (hexChars.Count % 2 != 0)
			hexChars.Add(0);

		var result = new byte[hexChars.Count / 2];
		for (int i = 0; i < result.Length; i++)
			result[i] = (byte)((hexChars[i * 2] << 4) | hexChars[i * 2 + 1]);

		return new PdfToken(PdfTokenType.HexString, result, tokenStart);
	}

	private PdfToken ReadNumber(long tokenStart)
	{
		var sb = new StringBuilder();
		if (_pos < _data.Length && (_data[_pos] == (byte)'+' || _data[_pos] == (byte)'-'))
		{
			sb.Append((char)_data[_pos]);
			_pos++;
		}
		bool hasDot = false;
		while (_pos < _data.Length)
		{
			byte c = _data[_pos];
			if (IsDigit(c))
			{
				sb.Append((char)c);
				_pos++;
			}
			else if (c == (byte)'.' && !hasDot)
			{
				hasDot = true;
				sb.Append('.');
				_pos++;
			}
			else
			{
				break;
			}
		}
		return new PdfToken(PdfTokenType.Number, sb.ToString(), tokenStart);
	}

	private PdfToken ReadKeyword(long tokenStart)
	{
		var sb = new StringBuilder();
		while (_pos < _data.Length)
		{
			byte c = _data[_pos];
			if (IsWhitespace(c) || IsDelimiter(c))
				break;
			sb.Append((char)c);
			_pos++;
		}
		return new PdfToken(PdfTokenType.Keyword, sb.ToString(), tokenStart);
	}

	#endregion

	#region Line scanning helpers

	/// <summary>
	/// Scans backward from the given position to find a line containing the target string.
	/// Returns the starting byte offset of the found target, or -1 if not found.
	/// </summary>
	public int FindLastOccurrence(string target, int fromPos = -1)
	{
		if (fromPos < 0) fromPos = _data.Length - 1;
		var targetBytes = Encoding.ASCII.GetBytes(target);
		for (int i = Math.Min(fromPos, _data.Length - targetBytes.Length); i >= 0; i--)
		{
			bool match = true;
			for (int j = 0; j < targetBytes.Length; j++)
			{
				if (_data[i + j] != targetBytes[j])
				{
					match = false;
					break;
				}
			}
			if (match) return i;
		}
		return -1;
	}

	/// <summary>
	/// Reads a line of ASCII text starting at the given position (up to CR, LF, or CRLF).
	/// Advances position past the line ending.
	/// </summary>
	public string ReadLine()
	{
		var sb = new StringBuilder();
		while (_pos < _data.Length)
		{
			byte c = _data[_pos];
			if (c == 13) // CR
			{
				_pos++;
				if (_pos < _data.Length && _data[_pos] == 10) // CRLF
					_pos++;
				break;
			}
			if (c == 10) // LF
			{
				_pos++;
				break;
			}
			sb.Append((char)c);
			_pos++;
		}
		return sb.ToString();
	}

	/// <summary>
	/// Reads an ASCII line but does not skip the line ending after the content.
	/// </summary>
	public string ReadLineContent()
	{
		var sb = new StringBuilder();
		while (_pos < _data.Length)
		{
			byte c = _data[_pos];
			if (c == 13 || c == 10)
				break;
			sb.Append((char)c);
			_pos++;
		}
		return sb.ToString();
	}

	/// <summary>
	/// Skips a single end-of-line sequence (CR, LF, or CRLF).
	/// </summary>
	public void SkipEol()
	{
		if (_pos < _data.Length && _data[_pos] == 13)
			_pos++;
		if (_pos < _data.Length && _data[_pos] == 10)
			_pos++;
	}

	#endregion

	#region Character classification

	private static bool IsWhitespace(byte b)
		=> b == 0 || b == 9 || b == 10 || b == 12 || b == 13 || b == 32;

	private static bool IsDelimiter(byte b)
		=> b == (byte)'(' || b == (byte)')' || b == (byte)'<' || b == (byte)'>'
		|| b == (byte)'[' || b == (byte)']' || b == (byte)'{' || b == (byte)'}'
		|| b == (byte)'/' || b == (byte)'%';

	private static bool IsDigit(byte b)
		=> b >= (byte)'0' && b <= (byte)'9';

	/// <summary>Regular character = not whitespace or delimiter</summary>
	private static bool IsRegular(byte b)
		=> !IsWhitespace(b) && !IsDelimiter(b);

	private static int HexDigit(byte b)
	{
		if (b >= (byte)'0' && b <= (byte)'9') return b - '0';
		if (b >= (byte)'a' && b <= (byte)'f') return b - 'a' + 10;
		if (b >= (byte)'A' && b <= (byte)'F') return b - 'A' + 10;
		return -1;
	}

	#endregion
}
