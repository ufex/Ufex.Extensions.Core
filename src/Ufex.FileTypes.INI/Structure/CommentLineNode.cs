using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.FileTypes.INI.Data;

namespace Ufex.FileTypes.INI.Structure;

/// <summary>
/// TreeNode for comment lines
/// </summary>
internal class CommentLineNode : LineNode
{
	public CommentLineNode(CommentLine line)
		: base(line, $"; {TruncateText(line.CommentText)}", "Comment", TreeViewIcon.Comment)
	{
	}

	private static string TruncateText(string text)
	{
		const int maxLength = 60;
		if (text.Length <= maxLength)
			return text;
		return text.Substring(0, maxLength - 3) + "...";
	}

	public override DynamicTableData TableData()
	{
		var td = base.TableData();
		var comment = (CommentLine)Line;

		td.AddRow("Delimiter", comment.Delimiter, "Comment delimiter character");
		td.AddRow("Comment Text", comment.CommentText, "Comment content");

		return td;
	}
}
