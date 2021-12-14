using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Text;
using Ufex.API;
using Ufex.API.Tables;

namespace Ufex.FileTypes
{

	internal class Section
	{
		public virtual void Read(BinaryReader br)
		{

		}

		public virtual DynamicTableData GetTableData()
		{ 
			DynamicTableData td = new DynamicTableData(2);
			td.SetColumn(0, "Property");
			td.SetColumn(1, "Value");

			Add2TableData(td);
			return td;
		}

		public virtual void Add2TableData(DynamicTableData td)
		{
		
		}

		public virtual TreeNode GetTreeNode()
		{
			return new TreeNode("error");
		}
	}

	class LocalFileHeader : Section
	{
		UInt32 locFileHeadSign;
		UInt16 versionToExtract;
		UInt16 generalBitFlag;
		UInt16 compMethod;
		UInt16 lastModFileTime;
		UInt16 lastModFileDate;
		UInt32 crc32;
		UInt32 compSize;
		UInt32 unCompSize;
		UInt16 fileNameLen;
		UInt16 extraFieldLen;
		sbyte [] fileName;
		byte [] extraField;

		public override void Read(BinaryReader br)
		{
			locFileHeadSign = br.ReadUInt32();
			versionToExtract = br.ReadUInt16();
			generalBitFlag = br.ReadUInt16();
			compMethod = br.ReadUInt16();
			lastModFileTime = br.ReadUInt16();
			lastModFileDate = br.ReadUInt16();
			crc32 = br.ReadUInt32();
			compSize = br.ReadUInt32();
			unCompSize = br.ReadUInt32();
			fileNameLen = br.ReadUInt16();
			extraFieldLen = br.ReadUInt16();
			fileName = new sbyte[fileNameLen];
			for(int i = 0; i < fileNameLen; i++)
				fileName[i] = br.ReadSByte();
			extraField = br.ReadBytes(extraFieldLen);
		}

		public override void Add2TableData(DynamicTableData td)
		{
			td.AddRow("Local Header Signature", locFileHeadSign);
			td.AddRow("Version Needed To Extract", versionToExtract);
			td.AddRow("General Purpose Bit Flag", generalBitFlag);
			td.AddRow("Compression Method", compMethod);
			td.AddRow("Last Mod File Time", lastModFileTime);
			td.AddRow("Last Mod File Date", lastModFileDate);
			td.AddRow("CRC-32", crc32);
			td.AddRow("Compressed Size", compSize);
			td.AddRow("Uncompressed Size", unCompSize);
			td.AddRow("File Name Length", fileNameLen);
			td.AddRow("Extra Field Length", extraFieldLen);
			td.AddRow("File Name", fileName);
			td.AddRow("Extra Field", extraField);
		}

		public UInt32 GetFileDataLength()
		{
			return compSize;
		}

		public bool HasDataDescriptor()
		{
			return false;
		}

		public string GetFileName()
		{
			StringBuilder sb = new StringBuilder(fileNameLen);
			for(int i = 0; i < fileNameLen; i++)
				sb.Append(Convert.ToChar(fileName[i]));

			return sb.ToString();
		}

		public override TreeNode GetTreeNode()
		{
			return new TreeNode("Local File Header", (int)TreeViewIcon.Header, (int)TreeViewIcon.Header);
		}
	}


	class FileData : Section
	{
		//Byte [] compData;
		public void Read(BinaryReader br, UInt32 size)
		{
			//problem!!!!!
			//compData = br.ReadBytes((int)size);
			br.BaseStream.Seek((long)size, SeekOrigin.Current);

		}		
		
		public override TreeNode GetTreeNode()
		{
			return new TreeNode("File Data", (int)TreeViewIcon.Table, (int)TreeViewIcon.Table);
		}
	}

	class DataDescriptor : Section
	{
		UInt32 crc32;
		UInt32 compSize;
		UInt32 unCompSize;

		public override void Read(BinaryReader br)
		{
			crc32 = br.ReadUInt32();
			compSize = br.ReadUInt32();
			unCompSize = br.ReadUInt32();
		}

		public override TreeNode GetTreeNode()
		{
			return new TreeNode("Data Descriptor", (int)TreeViewIcon.Table, (int)TreeViewIcon.Table);
		}
	}


	class CompFile : Section
	{
		public LocalFileHeader m_header;
		public FileData m_fileData;
		public DataDescriptor m_dataDescriptor;

		public override void Read(BinaryReader br)
		{
			// Read Local File Header
			m_header = new LocalFileHeader();
			m_header.Read(br);

			// Read File Data
			m_fileData = new FileData();
			m_fileData.Read(br, m_header.GetFileDataLength());

			// Read Data Descriptor
			if(m_header.HasDataDescriptor())
			{
				m_dataDescriptor = new DataDescriptor();
				m_dataDescriptor.Read(br);
			}
		}

		public override TreeNode GetTreeNode()
		{
			TreeNode tnFileComp = new TreeNode(m_header.GetFileName(), (int)TreeViewIcon.Document, (int)TreeViewIcon.Document);
			tnFileComp.Nodes.Add(m_header.GetTreeNode());
			tnFileComp.Nodes.Add(m_fileData.GetTreeNode());
			return tnFileComp;
		}

	}

	/// <summary>
	/// Summary description for ZIP.
	/// </summary>
	public class Zip : FileType
	{
		private UInt32 LOCAL_FILE_SIGN = 0x04034b50;
		private UInt32 CENTRAL_FILE_SIGN = 0x02014b50;
		
		ArrayList files;

		public Zip()
		{
			ShowTechnical = true;
			ShowGraphic = false;
			ShowFileCheck = false;
			UseTreeViewIcons = true;
			files = new ArrayList();
		}

		public override bool ProcessFile()
		{
			BinaryReader br = new BinaryReader(FileInStream);

			TreeNode tnFiles = new TreeNode("Files", (int)TreeViewIcon.FolderClosed, (int)TreeViewIcon.FolderOpen);

			while(br.PeekChar() == 0x50)
			{
				UInt32 sign = br.ReadUInt32();
				// Rewind
				m_FileStream.Position -= 4;
				if(sign == LOCAL_FILE_SIGN)
				{
					CompFile myCompFile = new CompFile();
					myCompFile.Read(br);
					tnFiles.Nodes.Add(myCompFile.GetTreeNode());
					files.Add(myCompFile);
				}
				else if(sign == CENTRAL_FILE_SIGN)
				{
					TreeNodes.Add(tnFiles);
					return false;
				}

			}

			TreeNodes.Add(tnFiles);

			return true;
		}

		public override TableData GetData(System.Windows.Forms.TreeNode tn)
		{
			TableData td = null;

			if(tn.FullPath.StartsWith("Files\\"))
			{
				CompFile thisFile;
			
				if(tn.FullPath.EndsWith("Local File Header"))
				{
					thisFile = (CompFile)files[tn.Parent.Index];
					td = thisFile.m_header.GetTableData();
				}
				else if(tn.FullPath.EndsWith("File Data"))
				{
					thisFile = (CompFile)files[tn.Parent.Index];
					td = thisFile.m_fileData.GetTableData();
				}
				else
				{
					thisFile = (CompFile)files[tn.Index];
					td = thisFile.GetTableData();
				}
			}
			
			return td;
		}
	}
}
