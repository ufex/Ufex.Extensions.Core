using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.FileTypes.INI.Data;

namespace Ufex.FileTypes.INI.Structure;

/// <summary>
/// TreeNode for INI sections - displays section name and contains property nodes
/// </summary>
internal class SectionNode : TreeNode
{
	public Section Section { get; }
	public string Description { get; }

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Section Properties")];

	public SectionNode(Section section)
		: base(
			section.IsGlobal ? "(Global)" : $"[{section.Name}]",
			TreeViewIcon.Section,
			TreeViewIcon.Section)
	{
		Section = section;
		Description = section.IsGlobal
			? "Global properties (before any section header)"
			: $"Section: {section.Name}";

		// Add child nodes for each property
		foreach (var property in section.Properties)
		{
			Nodes.Add(new PropertyNode(property));
		}
	}

	public DynamicTableData TableData()
	{
		var td = new DynamicTableData(2, "INI.SectionProperties");
		td.SetColumn(0, "Key");
		td.SetColumn(1, "Value");

		foreach (var property in Section.Properties)
		{
			td.AddRow(property.Key, property.Value);
		}

		return td;
	}
}
