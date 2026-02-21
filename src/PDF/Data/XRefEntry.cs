namespace Ufex.Extensions.Core.PDF.Data;

/// <summary>
/// Represents one entry in the cross-reference table.
/// </summary>
internal class XRefEntry
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
	/// Entry type: 0=free, 1=uncompressed, 2=compressed (in object stream)
	/// </summary>
	public int Type { get; init; }

	/// <summary>
	/// For type 1: byte offset in file. For type 2: object number of containing object stream.
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// For type 2: index within the object stream
	/// </summary>
	public int StreamIndex { get; init; }

	/// <summary>
	/// Whether this entry is in use (type 1 or 2)
	/// </summary>
	public bool InUse => Type != 0;

	public override string ToString()
	{
		return Type switch
		{
			0 => $"Object {ObjectNumber} gen {Generation} [free]",
			1 => $"Object {ObjectNumber} gen {Generation} @ offset {Offset}",
			2 => $"Object {ObjectNumber} [compressed in ObjStm {Offset} index {StreamIndex}]",
			_ => $"Object {ObjectNumber} [unknown type {Type}]"
		};
	}
}
