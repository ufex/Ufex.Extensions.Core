using System;
using System.Collections;
using System.Windows.Forms;

using Ufex.API;
using Ufex.API.Tables;

namespace Ufex.FileTypes
{
	class Member
	{
		byte id1;
		byte id2;
		byte compMethod;
		byte flags;
		uint modTime;
		byte extraFlags;
		public byte operatingSystem;

		ushort extraLength;
		byte[] extraBytes;
		string fileName;
		string fileComment;
		ushort crc16;

		public Member()
		{
			extraBytes = null;
			fileName = null;
			fileComment = null;
		}

		public void Read(FileReader fr)
		{
			id1 = fr.ReadByte();
			id2 = fr.ReadByte();
			compMethod = fr.ReadByte();
			flags = fr.ReadByte();
			modTime = fr.ReadUInt32();
			extraFlags = fr.ReadByte();
			operatingSystem = fr.ReadByte();

			// Read the FEXTRA
			if (DataManip.GetBit(flags, 2))
			{
				extraLength = fr.ReadUInt16();
				extraBytes = fr.ReadBytes(extraLength);
			}

			// Read the FNAME
			if (DataManip.GetBit(flags, 3))
				fileName = fr.ReadNullTermString();

			// Read the FCOMMENT
			if (DataManip.GetBit(flags, 4))
				fileComment = fr.ReadNullTermString();

			// Read the FHCRC
			if (DataManip.GetBit(flags, 1))
				crc16 = fr.ReadUInt16();
		}

		public DynamicTableData GetTableData(DynamicTableData td)
		{
			td.AddRow("ID1", id1);
			td.AddRow("ID2", id2);
			td.AddRow("Compression Method", compMethod);
			td.AddRow("Flags", flags);
			td.AddRow("Modification Time", modTime);
			td.AddRow("Extra Flags", extraFlags);
			td.AddRow("Operating System", operatingSystem);

			if (extraBytes != null)
			{
				td.AddRow("Extra Bytes Length", extraLength);
				td.AddRow("Extra Bytes", extraBytes);
			}

			if (fileName != null)
				td.AddRow("File Name", fileName);

			if (fileComment != null)
				td.AddRow("File Comment", fileComment);

			if (DataManip.GetBit(flags, 1))
				td.AddRow("CRC16", crc16);

			return td;
		}

		public TreeNode GetTreeNode()
		{
			TreeNode tn = new TreeNode(fileName, (int)TreeViewIcon.Document, (int)TreeViewIcon.Document);
			return tn;
		}

	}


	/// <summary>
	/// Summary description for GZip.
	/// </summary>
	public class GZip : FileType
	{

		Member[] members;

		readonly static string[] OS = {
			"FAT filesystem (MS-DOS, OS/2, NT/Win32)", 
			"Amiga", 
			"VMS (or OpenVMS)", 
			"Unix", 
			"VM/CMS", 
			"Atari TOS", 
			"HPFS filesystem (OS/2, NT)",	
			"Macintosh", 
			"Z-System", 
			"CP/M", 
			"TOPS-20",
			"NTFS filesystem (NT)",
			"QDOS",
			"Acorn RISCOS"
		};

		public GZip()
		{
			ShowTechnical = true;
			ShowGraphic = false;
			ShowFileCheck = false;
			UseTreeViewIcons = true;
		}

		public override bool ProcessFile()
		{
			FileReader fr = new FileReader(m_FileStream);

			members = new Member[1];

			for (int i = 0; i < members.Length; i++)
			{
				members[i] = new Member();
				members[i].Read(fr);

				TreeNode tn = members[i].GetTreeNode();
				tn.Tag = i;
				TreeNodes.Add(tn);
			}

			return true;
		}

		public override TableData GetData(TreeNode tn)
		{
			if (tn == null)
				return null;

			if (tn.Tag != null)
			{
				DynamicTableData td = new DynamicTableData(2);
				td.SetColumn(0, "Property");
				td.SetColumn(1, "Value");
				td = members[(int)tn.Tag].GetTableData(td);
				return td;
			}
			else
			{
				return null;
			}
		}

		public override QuickInfoTableData GetQuickInfo()
		{

			QuickInfoTableData td = new QuickInfoTableData();

			if (members[0].operatingSystem <= 13)
				td.AddRow("Operating System", OS[members[0].operatingSystem]);
			else if (members[0].operatingSystem == 255)
				td.AddRow("Operating System", "Unknown");

			return td;
		}

	}
}
