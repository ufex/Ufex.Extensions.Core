using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Format;
using Ufex.FileTypes.ZIP.Data;

namespace Ufex.FileTypes.ZIP.Structure;

internal class DataDescriptorNode : SectionNode
{
	public DataDescriptorNode(DataDescriptor dataDescriptor)
		: base(dataDescriptor, "Data Descriptor", TreeViewIcon.Table)
	{
	}

	public override string Description
	{
		get { return "Data Descriptor"; }
	}

	public override object[][] GetRows()
	{
		var d = (DataDescriptor)Section;
		object[][] rows = [
			["CRC-32", d.Crc32],
			["Compressed Size", d.CompressedSize, ByteCountFormatter.Format(d.CompressedSize)],
			["Uncompressed Size", d.UncompressedSize, ByteCountFormatter.Format(d.UncompressedSize)],
		];
		return rows;
	}
}
