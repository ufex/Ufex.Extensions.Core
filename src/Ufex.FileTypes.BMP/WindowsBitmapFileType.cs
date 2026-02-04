using System;
using System.Collections.Generic;
using Ufex.API;
using Ufex.API.Visual;
using Ufex.FileTypes.BMP.Data;
using Ufex.FileTypes.BMP.Structure;

namespace Ufex.FileTypes.BMP;

/// <summary>
/// Ufex FileType module for Windows Bitmap (BMP) files
/// </summary>
public class WindowsBitmapFileType : FileType
{
	public WindowsBitmapFileType()
	{
		Description = "Windows Bitmap (BMP)";
		Log.SetLogName("BMP.log");

		ShowTechnical = true;
		ShowGraphic = true;
		ShowFileCheck = true;
	}

	public override bool ProcessFile()
	{
		// Parse the file
		var reader = new BmpStreamReader(FileInStream, Log, ValidationReport);
		bool success = reader.Read();

		// Build UI components
		BuildQuickInfo(reader);
		BuildVisuals(reader);
		BuildStructure(reader);

		return success;
	}

	private void BuildQuickInfo(BmpStreamReader reader)
	{
		if (reader.InfoHeader == null) return;

		var header = reader.InfoHeader;
		QuickInfoTable.AddRow("Width", $"{header.Width} pixels");
		QuickInfoTable.AddRow("Height", $"{Math.Abs(header.Height)} pixels");
		QuickInfoTable.AddRow("Bit Depth", $"{header.BitsPerPixel}-bit");
		QuickInfoTable.AddRow("Compression", Constants.GetCompressionDescription(header.Compression));

		if (reader.ColorTable != null && reader.ColorTable.Count > 0)
		{
			QuickInfoTable.AddRow("Colors", reader.ColorTable.Count.ToString());
		}

		// Add header version info
		string headerVersion = header switch
		{
			BitmapV5Header => "BITMAPV5HEADER",
			BitmapV4Header => "BITMAPV4HEADER",
			_ => "BITMAPINFOHEADER"
		};
		QuickInfoTable.AddRow("Header Version", headerVersion);
	}

	private void BuildVisuals(BmpStreamReader reader)
	{
		var spans = new List<FileSpan>();

		// File header span
		if (reader.FileHeader != null)
		{
			spans.Add(new FileSpan
			{
				StartPosition = reader.FileHeader.Offset,
				EndPosition = reader.FileHeader.Offset + 14,
				Name = "File Header"
			});
		}

		// Info header span
		if (reader.InfoHeader != null)
		{
			spans.Add(new FileSpan
			{
				StartPosition = reader.InfoHeader.Offset,
				EndPosition = reader.InfoHeader.Offset + reader.InfoHeader.Size,
				Name = "Info Header"
			});
		}

		// Color table span
		if (reader.ColorTable != null && reader.ColorTable.Count > 0)
		{
			spans.Add(new FileSpan
			{
				StartPosition = reader.ColorTable.Offset,
				EndPosition = reader.ColorTable.Offset + (reader.ColorTable.Count * 4),
				Name = "Color Table"
			});
		}

		// Pixel data span
		if (reader.PixelDataSize > 0)
		{
			spans.Add(new FileSpan
			{
				StartPosition = reader.PixelDataOffset,
				EndPosition = reader.PixelDataOffset + reader.PixelDataSize,
				Name = "Pixel Data"
			});
		}

		var fileMap = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
		VisualsList.Add(fileMap);
	}

	private void BuildStructure(BmpStreamReader reader)
	{
		// File Header node
		if (reader.FileHeader != null)
		{
			TreeNodes.Add(new BitmapFileHeaderNode(reader.FileHeader));
		}

		// Info Header node
		if (reader.InfoHeader != null)
		{
			TreeNodes.Add(new BitmapInfoHeaderNode(reader.InfoHeader));
		}

		// Color Table node
		if (reader.ColorTable != null && reader.ColorTable.Count > 0)
		{
			TreeNodes.Add(new ColorTableNode(reader.ColorTable));
		}

		// Pixel Data node
		if (reader.PixelDataSize > 0)
		{
			TreeNodes.Add(new PixelDataNode(reader.PixelDataOffset, reader.PixelDataSize));
		}
	}
}
