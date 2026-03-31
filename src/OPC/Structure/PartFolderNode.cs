using Ufex.API.Tree;

namespace Ufex.Extensions.Core.OPC.Structure;

/// <summary>
/// Represents a folder/directory node in the OPC package tree.
/// This is used to group parts by their directory structure (e.g., "word/", "_rels/").
/// </summary>
public class PartFolderNode : TreeNode
{
	public PartFolderNode(string folderName)
		: base(folderName, TreeViewIcon.FolderClosed, TreeViewIcon.FolderOpen)
	{
	}
}
