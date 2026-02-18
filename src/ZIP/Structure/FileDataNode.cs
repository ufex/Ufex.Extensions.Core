using Ufex.API.Tree;
using Ufex.Extensions.Core.ZIP.Data;

namespace Ufex.Extensions.Core.ZIP.Structure;

internal class FileDataNode : SectionNode
{
	public FileDataNode(FileData fileData)
		: base(fileData, "File Data", TreeViewIcon.Table)
	{
		if(fileData.Blocks != null)
		{
			foreach(var block in fileData.Blocks)
			{
				Nodes.Add(new DeflateBlockNode(block));
			}
		}
	}

	public override string Description
	{
		get { return "File Data"; }
	}
}
