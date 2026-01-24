using System;
using System.IO;
using System.Collections;
using System.Text;
using System.IO.Compression;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Format;

namespace Ufex.FileTypes.ZIP;

internal class SectionNode
{
	public virtual TreeNode Node 
	{
		get
		{
			TreeNode tn = new TreeNode(TreeNodeLabel, TreeNodeIcon, TreeNodeIcon);
			tn.Tag = this;
			return tn;
		}
	}

	public virtual string TreeNodeLabel
	{
		get { throw new Exception("TreeNodeLabel must be implemented"); }
	}

	public virtual int TreeNodeIcon
	{
		get { return (int)TreeViewIcon.NullIcon; }
	}

	public virtual DynamicTableData TableData()
	{ 
		DynamicTableData td = new DynamicTableData(3);
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");

		PopulateTableData(td);
		return td;
	}

	public virtual void PopulateTableData(DynamicTableData td)
	{
	
	}
}

class LocalFileHeaderNode : SectionNode
{
	public ZipFileReader.LocalFileHeader LocalFileHeader;

	public override string TreeNodeLabel
	{
		get { return "Local File Header"; }
	}

	public override int TreeNodeIcon
	{
		get { return (int)TreeViewIcon.Header; }
	}

	public override void PopulateTableData(DynamicTableData td)
	{
		var d = LocalFileHeader;
		td.AddRow("Local Header Signature", d.LocFileHeadSign);
		td.AddRow("Version Needed To Extract", d.VersionToExtract);
		td.AddRow("General Purpose Bit Flag", d.GeneralBitFlag);
		td.AddRow(
			"Compression Method", 
			d.CompMethod, 
			Constants.COMPRESSION_METHODS.ContainsKey(d.CompMethod) ? Constants.COMPRESSION_METHODS[d.CompMethod] : "Unknown"
		);
		td.AddRow("Last Mod File Time", d.LastModFileTime);
		td.AddRow("Last Mod File Date", d.LastModFileDate);
		td.AddRow("CRC-32", d.Crc32);
		td.AddRow("Compressed Size", d.CompSize, ByteCountFormatter.Format(d.CompSize));
		td.AddRow("Uncompressed Size", d.UnCompSize, ByteCountFormatter.Format(d.UnCompSize));
		td.AddRow("File Name Length", d.FileNameLen, ByteCountFormatter.Format(d.FileNameLen));
		td.AddRow("Extra Field Length", d.ExtraFieldLen, ByteCountFormatter.Format(d.ExtraFieldLen));
		td.AddRow("File Name", d.FileName, d.FileNameText);
		td.AddRow("Extra Field", d.ExtraField);
	}
}


class FileDataNode : SectionNode
{
	public ZipFileReader.FileData FileData;

	public override string TreeNodeLabel { get { return "File Data"; } }

	public override int TreeNodeIcon { get { return (int)TreeViewIcon.Table; } }
}

class DataDescriptorNode : SectionNode
{
	public ZipFileReader.DataDescriptor DataDescriptor;
	public override string TreeNodeLabel { get { return "Data Descriptor"; } }
	public override int TreeNodeIcon { get { return (int)TreeViewIcon.Table; } }
}


class CompressedFileNode : SectionNode
{
	public ZipFileReader.CompressedFile CompressedFile;

	public override TreeNode Node 
	{
		get
		{
			TreeNode tnFileComp = new TreeNode(TreeNodeLabel, TreeNodeIcon, TreeNodeIcon);
			tnFileComp.Nodes.Add(new LocalFileHeaderNode() { LocalFileHeader = CompressedFile.Header }.Node);
			tnFileComp.Nodes.Add(new FileDataNode() { FileData = CompressedFile.FileData }.Node);
			return tnFileComp;
		}
	}

	public override string TreeNodeLabel
	{
		get { return CompressedFile.Header.FileNameText; }
	}

	public override int TreeNodeIcon
	{
		get { return (int)TreeViewIcon.Document; }
	}
}
class CentralDirectoryHeaderNode : SectionNode
{
	public ZipFileReader.CentralDirectoryHeader CentralDirectoryHeader;
	public override string TreeNodeLabel { get { return "Central Directory Header"; } }
	public override int TreeNodeIcon { get { return (int)TreeViewIcon.Header; } }

	public override void PopulateTableData(DynamicTableData td)
	{
		var d = CentralDirectoryHeader;
		td.AddRow("Central File Header Signature", d.CentralFileHeaderSignature);
		td.AddRow("Version Made By", d.VersionMadeBy);
		td.AddRow("Version Needed To Extract", d.VersionNeededToExtract);
		td.AddRow("General Purpose Bit Flag", d.GeneralPurposeBitFlag);
		td.AddRow("Compression Method", d.CompressionMethod, 
			Constants.COMPRESSION_METHODS.ContainsKey(d.CompressionMethod) ? 
			Constants.COMPRESSION_METHODS[d.CompressionMethod] : "Unknown"
		);
		td.AddRow("Last Mod File Time", d.LastModFileTime);
		td.AddRow("Last Mod File Date", d.LastModFileDate);
		td.AddRow("CRC-32", d.Crc32);
		td.AddRow("Compressed Size", d.CompressedSize, ByteCountFormatter.Format(d.CompressedSize));
		td.AddRow("Uncompressed Size", d.UncompressedSize, ByteCountFormatter.Format(d.UncompressedSize));
		td.AddRow("File Name Length", d.FileNameLength, ByteCountFormatter.Format(d.FileNameLength));
		td.AddRow("Extra Field Length", d.ExtraFieldLength, ByteCountFormatter.Format(d.ExtraFieldLength));
		td.AddRow("File Comment Length", d.FileCommentLength, ByteCountFormatter.Format(d.FileCommentLength));
		td.AddRow("Disk Number Start", d.DiskNumberStart);
		td.AddRow("Internal File Attributes", d.InternalFileAttributes);
		td.AddRow("External File Attributes", d.ExternalFileAttributes);
		td.AddRow("Relative Offset of Local Header", d.RelativeOffsetOfLocalHeader);
		td.AddRow("File Name", d.FileName);
		td.AddRow("Extra Field", d.ExtraField);
		td.AddRow("File Comment", d.FileComment);
	}
}

internal static class Constants
{

	public static readonly Dictionary<int, string> COMPRESSION_METHODS = new Dictionary<int, string>
	{
		{ 0, "Stored (no compression)" },
		{ 1, "Shrunk" },
		{ 2, "Reduced with compression factor 1" },
		{ 3, "Reduced with compression factor 2" },
		{ 4, "Reduced with compression factor 3" },
		{ 5, "Reduced with compression factor 4" },
		{ 6, "Imploded" },
		{ 7, "Reserved for Tokenizing compression algorithm" },
		{ 8, "Deflated" },
		{ 9, "Deflate64" },
		{ 10, "PKWARE Data Compression Library Imploding" },
		{ 11, "Reserved by PKWARE" },
		{ 12, "BZIP2" },
		{ 13, "Reserved by PKWARE" },
		{ 14, "LZMA" },
		{ 15, "Reserved by PKWARE" },
		{ 16, "Reserved by PKWARE" },
		{ 17, "Reserved by PKWARE" },
		{ 18, "IBM TERSE" },
		{ 19, "IBM LZ77 z Architecture (PFS)" },
		{ 97, "WavPack" },
		{ 98, "PPMd version I, Rev 1" }
	};
}

/// <summary>
/// FileType implementation for ZIP files.
/// </summary>
public class ZipFileType : FileType
{
	List<ZipFileReader.Section> Parts = new List<ZipFileReader.Section>();
	
	public ZipFileType()
	{
		ShowTechnical = true;
		ShowGraphic = false;
		ShowFileCheck = false;
		UseTreeViewIcons = true;
		Parts = new List<ZipFileReader.Section>();
	}

	public override bool ProcessFile()
	{	
		TreeNode tnFiles = new TreeNode("Files", (int)TreeViewIcon.FolderClosed, (int)TreeViewIcon.FolderOpen);
		TreeNode tnOther = new TreeNode("Other Data", (int)TreeViewIcon.FolderClosed, (int)TreeViewIcon.FolderOpen);
		ZipFileReader zipReader = new ZipFileReader(FileInStream);
		bool result = zipReader.Read();

		foreach(ZipFileReader.Section segment in zipReader.Parts)
		{
			Parts.Add(segment);
			var index = Parts.Count - 1;
			switch(segment)
			{
				case ZipFileReader.CompressedFile compFile:
					CompressedFileNode compFileNode = new CompressedFileNode() { CompressedFile = compFile };
					tnFiles.Nodes.Add(compFileNode.Node);
					break;
				case ZipFileReader.CentralDirectoryHeader centralDir:
					CentralDirectoryHeaderNode centralDirNode = new CentralDirectoryHeaderNode() { CentralDirectoryHeader = centralDir };
					tnOther.Nodes.Add(centralDirNode.Node);
					break;
				default:
					break;
			}
		}
		TreeNodes.Add(tnFiles);
		TreeNodes.Add(tnOther);
		return result;
	}

	public override TableData GetData(TreeNode tn)
	{
		Log.Info("Zip GetData: " + tn.FullPath);
		SectionNode node = (SectionNode)tn.Tag;
		return node.TableData();
	}
}
