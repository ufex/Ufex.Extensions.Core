using Ufex.API.Tree;
using Ufex.FileTypes.ZIP.Data;

namespace Ufex.FileTypes.ZIP.Structure;

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
