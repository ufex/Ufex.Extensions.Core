using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.Extensions.Core.INI.Data;

namespace Ufex.Extensions.Core.INI.Structure;

/// <summary>
/// TreeNode for INI properties (key=value pairs)
/// </summary>
internal class PropertyNode : TreeNode
{
	public Property Property { get; }
	public string Description { get; }

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Property Details")];

	public PropertyNode(Property property)
		: base(
			$"{property.Key} = {TruncateValue(property.Value)}",
			TreeViewIcon.Properties,
			TreeViewIcon.Properties)
	{
		Property = property;
		Description = $"Property: {property.Key}";
	}

	private static string TruncateValue(string value)
	{
		const int maxLength = 50;
		if (value.Length <= maxLength)
			return value;
		return value.Substring(0, maxLength - 3) + "...";
	}

	public DynamicTableData TableData()
	{
		var td = new DynamicTableData(3, "INI.PropertyDetails");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");

		var line = Property.SourceLine;

		td.AddRow("Key", Property.Key, "Normalized key name");
		td.AddRow("Value", Property.Value, "Normalized value");
		td.AddRow("Raw Key", line.RawKey, "Key as it appears in file");
		td.AddRow("Raw Value", line.RawValue, "Value as it appears in file");
		td.AddRow("Delimiter", line.Delimiter, "Key-value delimiter");
		td.AddRow("Line Number", line.LineNumber + 1, "1-based line number");
		td.AddRow("Offset", line.Offset, "Byte offset in file");
		td.AddRow("Length", line.Length, "Length in bytes");

		return td;
	}
}
