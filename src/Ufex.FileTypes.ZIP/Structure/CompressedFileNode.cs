using Ufex.API.Tree;
using Ufex.FileTypes.ZIP.Data;

namespace Ufex.FileTypes.ZIP.Structure;

internal class CompressedFileNode : SectionNode
{
	public CompressedFileNode(CompressedFile compressedFile)
		: base(compressedFile, compressedFile.Header.FileNameText, TreeViewIcon.Document)
	{
		Nodes.Add(new LocalFileHeaderNode(compressedFile.Header));
		if(compressedFile.FileData.CompressedSize > 0) 
		{
			Nodes.Add(new FileDataNode(compressedFile.FileData));
		}
	}

	public override string Description
	{
		get { return "Compressed File (" + ((CompressedFile)Section).Header.FileNameText + ")"; }
	}
}
