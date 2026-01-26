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

	public LocalFileHeaderNode(ZipFileReader.LocalFileHeader localFileHeader)
	{
		LocalFileHeader = localFileHeader;
	}

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
		object[][] rows = [
			["Local Header Signature", d.LocFileHeadSign],
			["Version Needed To Extract", d.VersionToExtract],
			["General Purpose Bit Flag", d.GeneralPurposeBitFlag],
			["Compression Method", d.CompressionMethod, Constants.COMPRESSION_METHODS.ContainsKey(d.CompressionMethod) ? Constants.COMPRESSION_METHODS[d.CompressionMethod] : "Unknown"],
			["Last Mod File Time", d.LastModFileTime, d.LastModFileTimeText],
			["Last Mod File Date", d.LastModFileDate, d.LastModFileDateText],
			["CRC-32", d.Crc32],
			["Compressed Size", d.CompSize, ByteCountFormatter.Format(d.CompSize)],
			["Uncompressed Size", d.UnCompSize, ByteCountFormatter.Format(d.UnCompSize)],
			["File Name Length", d.FileNameLength, ByteCountFormatter.Format(d.FileNameLength)],
			["Extra Field Length", d.ExtraFieldLength, ByteCountFormatter.Format(d.ExtraFieldLen)],
			["File Name", d.FileName, d.FileNameText],
			["Extra Field", d.ExtraField]
		];
		td.AddRows(rows);
	}
}


class FileDataNode : SectionNode
{
	public ZipFileReader.FileData FileData;

	public FileDataNode(ZipFileReader.FileData fileData)
	{
		FileData = fileData;
	}

	public override string TreeNodeLabel { get { return "File Data"; } }

	public override int TreeNodeIcon { get { return (int)TreeViewIcon.Table; } }
}

class DataDescriptorNode : SectionNode
{
	public ZipFileReader.DataDescriptor DataDescriptor;

	public DataDescriptorNode(ZipFileReader.DataDescriptor dataDescriptor)
	{
		DataDescriptor = dataDescriptor;
	}

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
			tnFileComp.Nodes.Add(new LocalFileHeaderNode(CompressedFile.Header).Node);
			tnFileComp.Nodes.Add(new FileDataNode(CompressedFile.FileData).Node);
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

	public CompressedFileNode(ZipFileReader.CompressedFile compressedFile)
	{
		CompressedFile = compressedFile;
	}
}

class CentralDirectoryHeaderNode : SectionNode
{
	public ZipFileReader.CentralDirectoryHeader CentralDirectoryHeader;

	public override string TreeNodeLabel { get { return "Central Directory Header"; } }
	public override int TreeNodeIcon { get { return (int)TreeViewIcon.Header; } }

	public CentralDirectoryHeaderNode(ZipFileReader.CentralDirectoryHeader centralDirectoryHeader)
	{
		CentralDirectoryHeader = centralDirectoryHeader;
	}

	public override void PopulateTableData(DynamicTableData td)
	{
		var d = CentralDirectoryHeader;
		object[][] rows = [
			["Central File Header Signature", d.CentralFileHeaderSignature],
			["Version Made By", d.VersionMadeBy],
			["Version Needed To Extract", d.VersionNeededToExtract],
			["General Purpose Bit Flag", d.GeneralPurposeBitFlag],
			["Compression Method", d.CompressionMethod, Constants.CompressionMethodDescription(d.CompressionMethod)],
			["Last Mod File Time", d.LastModFileTime, d.LastModFileTimeText],
			["Last Mod File Date", d.LastModFileDate, d.LastModFileDateText],
			["CRC-32", d.Crc32],
			["Compressed Size", d.CompressedSize, ByteCountFormatter.Format(d.CompressedSize)],
			["Uncompressed Size", d.UncompressedSize, ByteCountFormatter.Format(d.UncompressedSize)],
			["File Name Length", d.FileNameLengthgth, ByteCountFormatter.Format(d.FileNameLengthgth)],
			["Extra Field Length", d.ExtraFieldLength, ByteCountFormatter.Format(d.ExtraFieldLength)],
			["File Comment Length", d.FileCommentLength, ByteCountFormatter.Format(d.FileCommentLength)],
			["Disk Number Start", d.DiskNumberStart],
			["Internal File Attributes", d.InternalFileAttributes],
			["External File Attributes", d.ExternalFileAttributes],
			["Relative Offset of Local Header", d.RelativeOffsetOfLocalHeader],
			["File Name", d.FileName, d.FileNameText],
			["Extra Field", d.ExtraField],
			["File Comment", d.FileComment, d.FileCommentText]
		];
		td.AddRows(rows);
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

	public static string CompressionMethodDescription(int method)
	{
		if (COMPRESSION_METHODS.ContainsKey(method))
			return COMPRESSION_METHODS[method];
		else
			return "Unknown";
	}
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
		Parts = new List<ZipFileReader.Section>();
	}

	public override bool ProcessFile()
	{	
		TreeNode tnFiles = new TreeNode("Files", (int)TreeViewIcon.FolderClosed, (int)TreeViewIcon.FolderOpen);
		TreeNode tnOther = new TreeNode("Other Data", (int)TreeViewIcon.FolderClosed, (int)TreeViewIcon.FolderOpen);
		ZipFileReader zipReader = new ZipFileReader(FileInStream, Log);
		bool result = zipReader.Read();

		foreach(ZipFileReader.Section segment in zipReader.Parts)
		{
			Parts.Add(segment);
			var index = Parts.Count - 1;
			switch(segment)
			{
				case ZipFileReader.CompressedFile compFile:
					tnFiles.Nodes.Add(new CompressedFileNode(compFile).Node);
					break;
				case ZipFileReader.CentralDirectoryHeader centralDir:
					tnOther.Nodes.Add(new CentralDirectoryHeaderNode(centralDir).Node);
					break;
				default:
					break;
			}
		}
		TreeNodes.Add(tnFiles);
		TreeNodes.Add(tnOther);
		return result;
	}

	public override TableData? GetData(TreeNode tn)
	{
		if (tn.Tag is not SectionNode node)
			return null;
		return node.TableData();
	}
}
