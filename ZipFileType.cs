using System;
using System.IO;
using System.Collections;
using System.Text;
using System.IO.Compression;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Format;
using Ufex.API.Visual;
using System.Reflection.Emit;

namespace Ufex.FileTypes.ZIP;

internal class SectionNode : TreeNode
{
	public ZipFileReader.Section Section;

	public virtual string Description
	{
		get { throw new Exception("Description must be implemented"); }
	}

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get { return [ new DataGridVisual(TableData(), "Data") ]; }
	}

	public SectionNode(ZipFileReader.Section section, string text, TreeViewIcon imageIndex) 
		: base(text, imageIndex, imageIndex)
	{
		Section = section;
	}

	public static SectionNode FromSection(ZipFileReader.Section section)
	{
		switch(section)
		{
			case ZipFileReader.CompressedFile compFile:
				return new CompressedFileNode(compFile);
			case ZipFileReader.CentralDirectoryHeader centralDir:
				return new CentralDirectoryHeaderNode(centralDir);
			case ZipFileReader.EndOfCentralDirectoryRecord endRecord:
				return new EndOfCentralDirectoryRecordNode(endRecord);
			case ZipFileReader.LocalFileHeader localFileHeader:
				return new LocalFileHeaderNode(localFileHeader);
			case ZipFileReader.FileData fileData:
				return new FileDataNode(fileData);
			case ZipFileReader.DataDescriptor dataDescriptor:
				return new DataDescriptorNode(dataDescriptor);
			default:
				throw new Exception("Unknown section type");
		}
	}

	public virtual DynamicTableData TableData()
	{ 
		DynamicTableData td = new DynamicTableData(4, "Zip.PropertyValueDescription");
		td.SetColumn(0, "Offset");
		td.SetColumn(1, "Property");
		td.SetColumn(2, "Value");
		td.SetColumn(3, "Description");
		var rows = GetRows();
		long offset = Section.StartPosition;
		for(int i = 0; i < rows.Length; i++)
		{
			if(offset < 0x0100000000) {
				td.AddRow((uint)offset, rows[i][0], rows[i][1], rows[i].Length > 2 ? rows[i][2] : "");
			}
			else {
				td.AddRow((ulong)offset, rows[i][0], rows[i][1], rows[i].Length > 2 ? rows[i][2] : "");
			}
			
			offset += GetValueSize(rows[i][1]);
		}
		return td;
	}

	private static long GetValueSize(object value)
	{
		return value switch
		{
			byte => sizeof(byte),
			sbyte => sizeof(sbyte),
			short => sizeof(short),
			ushort => sizeof(ushort),
			int => sizeof(int),
			uint => sizeof(uint),
			long => sizeof(long),
			ulong => sizeof(ulong),
			float => sizeof(float),
			double => sizeof(double),
			byte[] arr => arr.Length,
			_ => 0
		};
	}

	public virtual object[][] GetRows()
	{
		return [];
	}
}

class LocalFileHeaderNode : SectionNode
{

	public LocalFileHeaderNode(ZipFileReader.LocalFileHeader localFileHeader) 
		: base(localFileHeader, "Local File Header", TreeViewIcon.Header)
	{
	}

	public override string Description
	{
		get { return "Local File Header (" + ((ZipFileReader.LocalFileHeader)Section).FileNameText + ")"; }
	}

	public override object[][] GetRows()
	{
		var d = (ZipFileReader.LocalFileHeader)Section;
		object[][] rows = [
			["Local Header Signature", d.LocFileHeadSign],
			["Version Needed To Extract", d.VersionToExtract],
			["General Purpose Bit Flag", d.GeneralPurposeBitFlag, Constants.GeneralPurposeBitFlagDescription(d.GeneralPurposeBitFlag, d.CompressionMethod)],
			["Compression Method", d.CompressionMethod, Constants.COMPRESSION_METHODS.ContainsKey(d.CompressionMethod) ? Constants.COMPRESSION_METHODS[d.CompressionMethod] : "Unknown"],
			["Last Mod File Time", d.LastModFileTime, d.LastModFileTimeText],
			["Last Mod File Date", d.LastModFileDate, d.LastModFileDateText],
			["CRC-32", d.Crc32],
			["Compressed Size", d.CompSize, ByteCountFormatter.Format(d.CompSize)],
			["Uncompressed Size", d.UnCompSize, ByteCountFormatter.Format(d.UnCompSize)],
			["File Name Length", d.FileNameLength, ByteCountFormatter.Format(d.FileNameLength)],
			["Extra Field Length", d.ExtraFieldLength, ByteCountFormatter.Format(d.ExtraFieldLength)],
			["File Name", d.FileName, d.FileNameText],
			["Extra Field", d.ExtraField]
		];
		return rows;
	}
}

class DeflateBlockNode : TreeNode
{
	public DeflateStreamReader.Block Block;

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get 
		{ 
			if(Block.Type != DeflateStreamReader.BlockType.DynamicHuffman || Block.LiteralCodeMap == null || Block.DistanceCodeMap == null)
			{
				return [];
			}
			return [ new TreeDiagramVisual(BuildVisualTree(Block.LiteralCodeMap), "Deflate Block Structure") ];
		}
	}

	public DeflateBlockNode(DeflateStreamReader.Block block)
		: base("Deflate Block", TreeViewIcon.Table, TreeViewIcon.Table)
	{
		Block = block;
		if(block.Type == DeflateStreamReader.BlockType.Stored)
		{
			Text = "Stored Block";
		}
		else if(block.Type == DeflateStreamReader.BlockType.DynamicHuffman)
		{
			Text = "Dynamic Huffman Block";
		}
		else if(block.Type == DeflateStreamReader.BlockType.StaticHuffman)
		{
			Text = "Static Huffman Block";
		}
		else
		{
			Text = "Reserved Block";
		}
	}

	private static TreeDiagramVisual.Node BuildVisualTree(Dictionary<int, (int code, int len)> codeMap)
	{
		TreeDiagramVisual.Node root = new TreeDiagramVisual.Node("Root");
		foreach (var entry in codeMap)
		{
			if (entry.Value.len == 0) continue;
			TreeDiagramVisual.Node current = root;
			for (int i = entry.Value.len - 1; i >= 0; i--)
			{
				int bit = (entry.Value.code >> i) & 1;
				while (current.Children.Count < 2) current.Children.Add(new TreeDiagramVisual.Node(current.Children.Count.ToString()));
				current = current.Children[bit];
			}
			string hex = $"0x{entry.Key:X2}";
			string ascii = (entry.Key >= 32 && entry.Key <= 126) ? $" '{(char)entry.Key}'" : "";
			current.Label = $"{hex}{ascii}";
			current.Children = null;
		}
		return root;
	}

}

class FileDataNode : SectionNode
{
	public FileDataNode(ZipFileReader.FileData fileData)
		: base(fileData, "File Data", TreeViewIcon.Table)
	{
		foreach(var block in fileData.Blocks)
		{
			Nodes.Add(new DeflateBlockNode(block));
		}
	}

	public override string Description
	{
		get { return "File Data"; }
	}

}

class DataDescriptorNode : SectionNode
{
	public DataDescriptorNode(ZipFileReader.DataDescriptor dataDescriptor)
		: base(dataDescriptor, "Data Descriptor", TreeViewIcon.Table)
	{
	}

	public override string Description
	{
		get { return "Data Descriptor"; }
	}

	public override object[][] GetRows()
	{
		var d = (ZipFileReader.DataDescriptor)Section;
		object[][] rows = [
			["CRC-32", d.Crc32],
			["Compressed Size", d.CompressedSize, ByteCountFormatter.Format(d.CompressedSize)],
			["Uncompressed Size", d.UncompressedSize, ByteCountFormatter.Format(d.UncompressedSize)],
		];
		return rows;
	}
}


class CompressedFileNode : SectionNode
{
	public CompressedFileNode(ZipFileReader.CompressedFile compressedFile)
		: base(compressedFile, compressedFile.Header.FileNameText, TreeViewIcon.Document)
	{
		Nodes.Add(new LocalFileHeaderNode(compressedFile.Header));
		Nodes.Add(new FileDataNode(compressedFile.FileData));
	}

	public override string Description
	{
		get { return "Compressed File (" + ((ZipFileReader.CompressedFile)Section).Header.FileNameText + ")"; }
	}
}

class CentralDirectoryHeaderNode : SectionNode
{
	public CentralDirectoryHeaderNode(ZipFileReader.CentralDirectoryHeader centralDirectoryHeader)
		: base(centralDirectoryHeader, "Central Directory Header", TreeViewIcon.Header)
	{
	}

	public override string Description
	{
		get { return "Central Directory Header (" + ((ZipFileReader.CentralDirectoryHeader)Section).FileNameText + ")"; }
	}

	public override object[][] GetRows()
	{
		var d = (ZipFileReader.CentralDirectoryHeader)Section;
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
			["File Name Length", d.FileNameLength, ByteCountFormatter.Format(d.FileNameLength)],
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
		return rows;
	}
}
class EndOfCentralDirectoryRecordNode : SectionNode
{
	public EndOfCentralDirectoryRecordNode(ZipFileReader.EndOfCentralDirectoryRecord record)
		: base(record, "End of Central Directory Record", TreeViewIcon.Header)
	{
	}

	public override string Description
	{
		get { return "End of Central Directory Record"; }
	}

	public override object[][] GetRows()
	{
		var d = (ZipFileReader.EndOfCentralDirectoryRecord)Section;
		object[][] rows = [
			["End of Central Directory Signature", d.EndOfCentralDirSignature],
			["Number of This Disk", d.NumberOfThisDisk],
			["Disk Where Central Directory Starts", d.DiskWhereCentralDirectoryStarts],
			["Number of Central Directory Records on This Disk", d.NumberOfCentralDirectoryRecordsOnThisDisk],
			["Total Number of Central Directory Records", d.TotalNumberOfCentralDirectoryRecords],
			["Size of Central Directory", d.SizeOfCentralDirectory, ByteCountFormatter.Format(d.SizeOfCentralDirectory)],
			["Offset of Start of Central Directory", d.OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber],
			["ZIP File Comment Length", d.ZIPFileCommentLength, ByteCountFormatter.Format(d.ZIPFileCommentLength)],
			["ZIP File Comment", d.ZIPFileComment, d.ZIPFileCommentText]
		];
		return rows;
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

	public static string GeneralPurposeBitFlagDescription(UInt16 flag, UInt16 compressionMethod)
	{
		List<string> descriptions = new List<string>();
		if (ByteUtil.GetBit(flag, 0)) descriptions.Add("Encrypted");
		if (compressionMethod == 6) {
			descriptions.Add(ByteUtil.GetBit(flag, 1) ? "8K sliding dictionary" : "4K sliding dictionary");
			descriptions.Add(ByteUtil.GetBit(flag, 2) ? "3 Shannon-Fano trees" : "2 Shannon-Fano trees");
		}
		else if (compressionMethod == 8 || compressionMethod == 9) {
			var bit1 = ByteUtil.GetBit(flag, 1);
			var bit2 = ByteUtil.GetBit(flag, 2);
			if(bit1 && bit2)
				descriptions.Add("Super Fast compression");
			else if(!bit1 && bit2)
				descriptions.Add("Fast compression");
			else if(bit1 && !bit2)
				descriptions.Add("Maximum compression");
			else
				descriptions.Add("Normal compression");
		}
		else if(compressionMethod == 14) {
			descriptions.Add(ByteUtil.GetBit(flag, 1) ? "end-of-stream marker" : "no end-of-stream marker");
		}
		if (ByteUtil.GetBit(flag, 3)) descriptions.Add("Data descriptor present");
		if (ByteUtil.GetBit(flag, 4) && compressionMethod == 8) descriptions.Add("Enhanced deflation");
		if (ByteUtil.GetBit(flag, 5)) descriptions.Add("Compressed patched data");
		if (ByteUtil.GetBit(flag, 6)) descriptions.Add("Strong encryption");
		if (ByteUtil.GetBit(flag, 11)) descriptions.Add("Language encoding flag (UTF-8 filenames and comments)");
		if (ByteUtil.GetBit(flag, 12)) descriptions.Add("Reserved by PKWARE for enhanced compression");
		return string.Join(", ", descriptions);
	}
}

/// <summary>
/// FileType implementation for ZIP files.
/// </summary>
public class ZipFileType : FileType
{
	protected List<ZipFileReader.Section> Parts { get; set; }

	protected FileMap? Map { get; set; }

	public ZipFileType()
	{
		ShowTechnical = true;
		ShowGraphic = true;
		ShowFileCheck = true;
		Parts = new List<ZipFileReader.Section>();
	}

	public override bool ProcessFile()
	{	
		ZipFileReader zipReader = new ZipFileReader(FileInStream, Log, ValidationReport);
		bool result = zipReader.Read();

		BuildQuickInfo(zipReader);
		BuildVisuals(zipReader);
		BuildStructure(zipReader);
		return result;
	}

	protected void BuildQuickInfo(ZipFileReader zipReader)
	{
		var parts = zipReader.Parts;
		var quickInfo = QuickInfoTable;
		quickInfo.AddRow("Number of Files", parts.FindAll(p => p is ZipFileReader.CompressedFile).Count.ToString());
		quickInfo.AddRow("Compression Methods", string.Join(", ", parts.OfType<ZipFileReader.CompressedFile>().Select(f => Constants.CompressionMethodDescription(f.Header.CompressionMethod)).Distinct()));
		foreach(var part in parts)
		{
			if(part is ZipFileReader.EndOfCentralDirectoryRecord eocdRecord)
			{
				quickInfo.AddRow("ZIP Comment", eocdRecord.ZIPFileCommentText);
			}
		}
	}

	protected void BuildVisuals(ZipFileReader zipReader)
	{
		var spans = new List<FileSpan>();
		foreach(ZipFileReader.Section segment in zipReader.Parts)
		{
			SectionNode node = SectionNode.FromSection(segment);
			spans.Add(new FileSpan
			{
				StartPosition = segment.StartPosition,
				EndPosition = segment.EndPosition,
				Name = node.Description
			});
		}
		
		// Create the FileMap from collected spans
		Map = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
		VisualsList.Add(Map);
	}

	protected void BuildStructure(ZipFileReader zipReader) 
	{
		TreeNode tnFiles = new TreeNode("Files", TreeViewIcon.FolderClosed, TreeViewIcon.FolderOpen);
		TreeNode tnOther = new TreeNode("Other Data", TreeViewIcon.FolderClosed, TreeViewIcon.FolderOpen);
	
		foreach(ZipFileReader.Section segment in zipReader.Parts)
		{
			Log.Info($"Processing segment at position {segment.StartPosition}, type {segment.GetType().Name}");
			SectionNode node = SectionNode.FromSection(segment);
			switch(segment)
			{
				case ZipFileReader.CompressedFile compFile:
					tnFiles.Nodes.Add(node);
					break;
				default:
					tnOther.Nodes.Add(node);
					break;
			}
		}
		
		TreeNodes.Add(tnFiles);
		TreeNodes.Add(tnOther);
	}

/* 	public override TableData? GetData(TreeNode tn)
	{
		if (tn.Tag is not SectionNode node)
			return null;
		return node.TableData();
	} */
}
