using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ZIP.Data;

namespace Ufex.Extensions.Core.ZIP.Structure;

public class FileDataNode : SectionNode
{
	public override bool HasDeferredChildren =>
		((FileData)Section).CompressionMethod == (ushort)CompressionMethod.Deflate
		&& ((FileData)Section).CompressedSize > 0;

	public FileDataNode(FileData fileData)
		: base(fileData, "File Data", TreeViewIcon.Table)
	{
	}

	public override void LoadChildren(IFileContext context)
	{
		var fileData = (FileData)Section;
		fileData.ReadBlocks(context.FileStream);

		if (fileData.Blocks != null)
		{
			foreach (var block in fileData.Blocks)
			{
				Nodes.Add(new DeflateBlockNode(block));
			}
		}

		base.LoadChildren(context);
	}

	public override string Description
	{
		get { return "File Data"; }
	}
}
