using System;
using System.Collections.Generic;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;

namespace Ufex.Extensions.Core.GZIP;

/// <summary>
/// TreeNode for GZIP members - displays member information and contains property nodes
/// </summary>
internal class MemberNode : TreeNode
{
	public Member Member { get; }

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Member Properties")];

	public MemberNode(Member member)
		: base(member.FileNameString != null ? member.FileNameString : "(Unnamed Member)", TreeViewIcon.Section, TreeViewIcon.Section)
	{
		Member = member;
	}

	public DynamicTableData TableData()
	{
		var d = Member;
		var td = new DynamicTableData(4, "GZIP.MemberProperties");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		List<object[]> rows = new List<object[]>();
		rows.Add(["ID1", d.ID1]);
		rows.Add(["ID2", d.ID2]);

		string compMethodString = Member.CompressionMethods.TryGetValue(d.CompressionMethod, out var cm)
			? cm
			: "unknown";
		rows.Add(["Compression Method", d.CompressionMethod, compMethodString]);

		List<string> flagNames = new List<string>();
		if (ByteUtil.GetBit(d.Flags, 0)) flagNames.Add("FTEXT");
		if (ByteUtil.GetBit(d.Flags, 1)) flagNames.Add("FHCRC");
		if (ByteUtil.GetBit(d.Flags, 2)) flagNames.Add("FEXTRA");
		if (ByteUtil.GetBit(d.Flags, 3)) flagNames.Add("FNAME");
		if (ByteUtil.GetBit(d.Flags, 4)) flagNames.Add("FCOMMENT");
		rows.Add(["Flags", d.Flags, string.Join(" | ", flagNames)]);

		rows.Add(["Modification Time", d.ModTime]);
		rows.Add(["Extra Flags", d.ExtraFlags]);
		rows.Add(["Operating System", d.OperatingSystem]);

		if (d.ExtraBytes != null)
		{
			rows.Add(["Extra Bytes Length", d.ExtraLength]);
			rows.Add(["Extra Bytes", d.ExtraBytes]);
		}

		if (d.FileName != null)
			rows.Add(["File Name", d.FileName, d.FileNameString]);

		if (d.FileComment != null)
			rows.Add(["File Comment", d.FileComment, d.FileCommentString]);

		if (ByteUtil.GetBit(d.Flags, 1))
			rows.Add(["CRC16", d.Crc16]);

		long offset = d.Offset;
		foreach(var row in rows)
		{
			td.AddRow(row[0], row[1], row.Length > 2 ? row[2] : "", new FileOffset(offset));
			offset += ByteUtil.GetObjectSize(row[1]);
		}
		return td;
	}
}
