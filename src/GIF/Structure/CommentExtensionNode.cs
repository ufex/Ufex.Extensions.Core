using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.Extensions.Core.GIF.Data;

namespace Ufex.Extensions.Core.GIF.Structure;

/// <summary>
/// Comment Extension node
/// </summary>
internal class CommentExtensionNode : TreeNode
{
	private readonly CommentExtension _extension;

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Comment")];

	public CommentExtensionNode(CommentExtension extension)
		: base("Comment Extension", TreeViewIcon.Comment, TreeViewIcon.Comment)
	{
		_extension = extension;
	}

	public DynamicTableData TableData()
	{
		var td = new DynamicTableData(2, "GIF.CommentExtension");
		td.SetColumn(0, "Field");
		td.SetColumn(1, "Value");

		td.AddRow("Extension Introducer", $"0x{_extension.ExtensionIntroducer:X2}");
		td.AddRow("Comment Label", $"0x{_extension.CommentLabel:X2}");
		td.AddRow("Comment Text", _extension.CommentText);

		return td;
	}
}
