using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.API.Format;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// Base TreeNode class for ISOBMFF/QTFF boxes. Handles displaying
/// common box header fields and factory creation of specialized nodes.
/// </summary>
internal class BoxNode : TreeNode
{
	private static readonly Dictionary<Type, Type> BOX_NODE_TYPES = new()
	{
		{ typeof(FtypBox), typeof(FtypBoxNode) },
		{ typeof(MvhdBox), typeof(MvhdBoxNode) },
		{ typeof(TkhdBox), typeof(TkhdBoxNode) },
		{ typeof(MdhdBox), typeof(MdhdBoxNode) },
		{ typeof(HdlrBox), typeof(HdlrBoxNode) },
		{ typeof(VmhdBox), typeof(VmhdBoxNode) },
		{ typeof(SmhdBox), typeof(SmhdBoxNode) },
		{ typeof(ElstBox), typeof(ElstBoxNode) },
		{ typeof(StsdBox), typeof(StsdBoxNode) },
		{ typeof(SttsBox), typeof(SttsBoxNode) },
		{ typeof(StscBox), typeof(StscBoxNode) },
		{ typeof(StszBox), typeof(StszBoxNode) },
		{ typeof(StcoBox), typeof(StcoBoxNode) },
		{ typeof(Co64Box), typeof(Co64BoxNode) },
		{ typeof(StssBox), typeof(StssBoxNode) },
		{ typeof(CttsBox), typeof(CttsBoxNode) },
		// 3GPP metadata boxes
		{ typeof(Data.ThreeGpp.TextMetadataBox), typeof(ThreeGpp.TextMetadataBoxNode) },
		{ typeof(Data.ThreeGpp.RtngBox), typeof(ThreeGpp.RtngBoxNode) },
		{ typeof(Data.ThreeGpp.ClsfBox), typeof(ThreeGpp.ClsfBoxNode) },
		{ typeof(Data.ThreeGpp.KywdBox), typeof(ThreeGpp.KywdBoxNode) },
		{ typeof(Data.ThreeGpp.LociBox), typeof(ThreeGpp.LociBoxNode) },
		{ typeof(Data.ThreeGpp.AlbmBox), typeof(ThreeGpp.AlbmBoxNode) },
		{ typeof(Data.ThreeGpp.YrrcBox), typeof(ThreeGpp.YrrcBoxNode) },
		// HEIF boxes
		{ typeof(Data.Heif.PitmBox), typeof(Heif.PitmBoxNode) },
		{ typeof(Data.Heif.IlocBox), typeof(Heif.IlocBoxNode) },
		{ typeof(Data.Heif.IinfBox), typeof(Heif.IinfBoxNode) },
		{ typeof(Data.Heif.InfeBox), typeof(Heif.InfeBoxNode) },
		{ typeof(Data.Heif.IspeBox), typeof(Heif.IspeBoxNode) },
		{ typeof(Data.Heif.PixiBox), typeof(Heif.PixiBoxNode) },
		{ typeof(Data.Heif.ColrBox), typeof(Heif.ColrBoxNode) },
		{ typeof(Data.Heif.IrotBox), typeof(Heif.IrotBoxNode) },
		{ typeof(Data.Heif.ImirBox), typeof(Heif.ImirBoxNode) },
		{ typeof(Data.Heif.ClapBox), typeof(Heif.ClapBoxNode) },
		{ typeof(Data.Heif.AuxcBox), typeof(Heif.AuxcBoxNode) },
		{ typeof(Data.Heif.IpmaBox), typeof(Heif.IpmaBoxNode) },
		{ typeof(Data.Heif.PaspBox), typeof(Heif.PaspBoxNode) },
		{ typeof(Data.Heif.ClliBox), typeof(Heif.ClliBoxNode) },
		{ typeof(Data.Heif.MdcvBox), typeof(Heif.MdcvBoxNode) },
	};

	protected Box _box;

	public string Description { get; protected set; }

	public override Visual[] Visuals
	{
		get { return [ new DataGridVisual(TableData(), "Data") ]; }
	}

	public BoxNode(Box box, string boxType, string boxDescription, TreeViewIcon icon)
		: base(boxType, icon, icon)
	{
		_box = box;
		Description = boxType + " — " + boxDescription;

		// Recursively add child box nodes
		foreach (Box child in box.Children)
		{
			Nodes.Add(FromBox(child));
		}
	}

	/// <summary>
	/// Factory method to create the appropriate BoxNode subclass based on the Box type.
	/// </summary>
	public static BoxNode FromBox(Box box)
	{
		if (BOX_NODE_TYPES.TryGetValue(box.GetType(), out var nodeClass))
		{
			foreach (var ctor in nodeClass.GetConstructors())
			{
				var parameters = ctor.GetParameters();
				if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(box.GetType()))
				{
					return (BoxNode)ctor.Invoke(new object[] { box });
				}
			}
			throw new InvalidOperationException($"No suitable constructor found for box node type {nodeClass.Name}.");
		}

		// Fallback: generic box node
		string typeStr = box.TypeString.Trim();
		string description = BoxTypes.Descriptions.GetValueOrDefault(box.TypeString, "Unknown");
		TreeViewIcon icon = box.Children.Count > 0 ? TreeViewIcon.FolderClosed : TreeViewIcon.Section;
		return new BoxNode(box, typeStr, description, icon);
	}

	public virtual DynamicTableData TableData()
	{
		DynamicTableData td = new DynamicTableData(4, "ISOBMFF.PropertyValueDescriptionOffset");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		td.AddRow("Size", _box.Size, _box.Size == 1 ? "Extended size follows" : _box.Size == 0 ? "Extends to EOF" : ByteCountFormatter.Format(_box.ActualSize), new FileOffset((UInt32)_box.Offset));
		td.AddRow("Type", _box.Type, _box.TypeString, new FileOffset((UInt32)(_box.Offset + 4)));

		long offset = _box.Offset + 8;

		if (_box.Size == 1)
		{
			td.AddRow("Extended Size", _box.ExtendedSize, ByteCountFormatter.Format(_box.ExtendedSize), new FileOffset((UInt32)offset));
			offset += 8;
		}

		if (_box.Version != null)
		{
			td.AddRow("Version", _box.Version, "", new FileOffset((UInt32)offset));
			offset += 1;
			td.AddRow("Flags", _box.Flags, $"0x{_box.Flags:X6}", new FileOffset((UInt32)offset));
			offset += 3;
		}

		var rows = GetRows();
		for (int i = 0; i < rows.Length; i++)
		{
			td.AddRow(rows[i][0], rows[i][1], rows[i].Length > 2 ? rows[i][2] : "", new FileOffset((UInt32)offset));
			offset += ByteUtil.GetObjectSize(rows[i][1]);
		}

		return td;
	}

	public virtual object[][] GetRows()
	{
		return [];
	}
}
