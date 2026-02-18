using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.Extensions.Core.INI.Data;

namespace Ufex.Extensions.Core.INI.Structure;

/// <summary>
/// TreeNode for invalid lines
/// </summary>
internal class InvalidLineNode : LineNode
{
	public InvalidLineNode(InvalidLine line)
		: base(line, $"[Invalid] {TruncateText(line.RawText)}", line.Reason, TreeViewIcon.NullIcon)
	{
	}

	private static string TruncateText(string text)
	{
		const int maxLength = 50;
		if (text.Length <= maxLength)
			return text;
		return text.Substring(0, maxLength - 3) + "...";
	}

	public override DynamicTableData TableData()
	{
		var td = base.TableData();
		var invalid = (InvalidLine)Line;

		td.AddRow("Reason", invalid.Reason, "Why this line is invalid");

		return td;
	}
}
