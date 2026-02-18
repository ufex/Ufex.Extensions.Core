using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.Extensions.Core.ZIP.Data;

namespace Ufex.Extensions.Core.ZIP.Structure;

internal class SectionNode : TreeNode
{
	public Section Section;

	public virtual string Description
	{
		get { throw new Exception("Description must be implemented"); }
	}

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get { return [ new DataGridVisual(TableData(), "Data") ]; }
	}

	public SectionNode(Section section, string text, TreeViewIcon imageIndex) 
		: base(text, imageIndex, imageIndex)
	{
		Section = section;
	}

	public static SectionNode FromSection(Section section)
	{
		switch(section)
		{
			case CompressedFile compFile:
				return new CompressedFileNode(compFile);
			case CentralDirectoryHeader centralDir:
				return new CentralDirectoryHeaderNode(centralDir);
			case EndOfCentralDirectoryRecord endRecord:
				return new EndOfCentralDirectoryRecordNode(endRecord);
			case LocalFileHeader localFileHeader:
				return new LocalFileHeaderNode(localFileHeader);
			case FileData fileData:
				return new FileDataNode(fileData);
			case DataDescriptor dataDescriptor:
				return new DataDescriptorNode(dataDescriptor);
			default:
				throw new Exception("Unknown section type");
		}
	}

	public virtual DynamicTableData TableData()
	{ 
		DynamicTableData td = new DynamicTableData(4, "Zip.PropertyValueDescription");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");
		
		var rows = GetRows();
		long offset = Section.StartPosition;
		for(int i = 0; i < rows.Length; i++)
		{
			td.AddRow(rows[i][0], rows[i][1], rows[i].Length > 2 ? rows[i][2] : "", new FileOffset(offset));
			offset += ByteUtil.GetObjectSize(rows[i][1]);
		}
		return td;
	}

	public virtual object[][] GetRows()
	{
		return [];
	}
}
