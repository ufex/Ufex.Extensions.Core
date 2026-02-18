using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.FileTypes.GIF.Data;
using Ufex.FileTypes.GIF.Structure;

namespace Ufex.FileTypes.GIF;

/// <summary>
/// GIF FileType module for Ufex
/// Supports both GIF87a and GIF89a versions
/// </summary>
public class GifFileType : FileType
{
	private GifStreamReader? _reader;

	public GifFileType()
	{
		ShowGraphic = true;
		ShowTechnical = true;
		ShowFileCheck = true;
		Description = "GIF Image";
	}

	public override bool ProcessFile()
	{
		_reader = new GifStreamReader(FileInStream, Log, ValidationReport);
		bool result = _reader.Read();

		if (!result)
			return false;

		// Build Quick Info
		BuildQuickInfo();

		// Build Visuals
		BuildVisuals();

		// Build Structure Tree
		BuildStructure();

		return true;
	}

	private void BuildQuickInfo()
	{
		if (_reader == null) return;

		QuickInfoTable.AddRow("Format", $"GIF{_reader.Version}");
		QuickInfoTable.AddRow("Width", $"{_reader.LogicalScreenDescriptor?.Width} pixels");
		QuickInfoTable.AddRow("Height", $"{_reader.LogicalScreenDescriptor?.Height} pixels");

		if (_reader.LogicalScreenDescriptor?.GlobalColorTableFlag == true)
		{
			QuickInfoTable.AddRow("Colors", $"{_reader.GlobalColorTable?.ColorCount}");
		}

		QuickInfoTable.AddRow("Frames", $"{_reader.Frames.Count}");

		if (_reader.IsAnimated)
		{
			QuickInfoTable.AddRow("Animated", "Yes");

			// Check for loop count from NETSCAPE extension
			var netscape = _reader.ApplicationExtensions.FirstOrDefault(e => e.IsNetscapeExtension);
			if (netscape?.LoopCount != null)
			{
				var loopText = netscape.LoopCount == 0 ? "Infinite" : netscape.LoopCount.ToString();
				QuickInfoTable.AddRow("Loop Count", loopText);
			}

			// Calculate total animation duration
			int totalDelayMs = _reader.Frames.Sum(f => f.DelayTimeMs);
			if (totalDelayMs > 0)
			{
				QuickInfoTable.AddRow("Duration", $"{totalDelayMs / 1000.0:F2} seconds");
			}
		}

		// Add comments if present
		if (_reader.CommentExtensions.Count > 0)
		{
			var comment = _reader.CommentExtensions[0].CommentText;
			if (comment.Length > 50)
				comment = comment.Substring(0, 47) + "...";
			QuickInfoTable.AddRow("Comment", comment);
		}
	}

	private void BuildVisuals()
	{
		if (_reader == null) return;

		// Build file spans for file map
		var spans = new List<FileSpan>();

		// Header (6 bytes)
		spans.Add(new FileSpan { Name = "Header", StartPosition = 0, EndPosition = 6 });

		// Logical Screen Descriptor (7 bytes)
		spans.Add(new FileSpan { Name = "Logical Screen Descriptor", StartPosition = 6, EndPosition = 13 });

		// Global Color Table
		if (_reader.GlobalColorTable != null)
		{
			spans.Add(new FileSpan
			{
				Name = "Global Color Table",
				StartPosition = _reader.GlobalColorTable.Offset,
				EndPosition = _reader.GlobalColorTable.Offset + _reader.GlobalColorTable.Size
			});
		}

		// Frames
		foreach (var frame in _reader.Frames)
		{
			long frameStart = frame.GraphicControlExtension?.Offset ?? frame.ImageDescriptor.Offset;
			long frameEnd = frame.ImageData.Offset + frame.ImageData.TotalDataSize + frame.ImageData.BlockCount + 2;
			spans.Add(new FileSpan
			{
				Name = $"Frame {frame.FrameIndex}",
				StartPosition = frameStart,
				EndPosition = frameEnd
			});
		}

		var map = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
		VisualsList.Add(map);
	}

	private void BuildStructure()
	{
		if (_reader == null) return;

		// Add Header node
		if (_reader.Header != null)
		{
			TreeNodes.Add(new HeaderNode(_reader.Header));
		}

		// Add Logical Screen Descriptor node
		if (_reader.LogicalScreenDescriptor != null)
		{
			TreeNodes.Add(new LogicalScreenDescriptorNode(_reader.LogicalScreenDescriptor));
		}

		// Add Global Color Table node
		if (_reader.GlobalColorTable != null)
		{
			TreeNodes.Add(new ColorTableNode(_reader.GlobalColorTable, isGlobal: true));
		}

		// Add Application Extensions
		foreach (var appExt in _reader.ApplicationExtensions)
		{
			TreeNodes.Add(new ApplicationExtensionNode(appExt));
		}

		// Add Comment Extensions
		foreach (var commentExt in _reader.CommentExtensions)
		{
			TreeNodes.Add(new CommentExtensionNode(commentExt));
		}

		// Add Frame nodes
		foreach (var frame in _reader.Frames)
		{
			TreeNodes.Add(new FrameNode(frame));
		}
	}
}
