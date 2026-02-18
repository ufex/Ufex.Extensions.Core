using Ufex.API;
using Ufex.API.Validation;
using Ufex.Extensions.Core.GIF.Data;
using Microsoft.Extensions.Logging;

namespace Ufex.Extensions.Core.GIF;

/// <summary>
/// Reads and parses GIF (Graphics Interchange Format) files
/// Supports both GIF87a and GIF89a versions
/// </summary>
public class GifStreamReader
{
	private readonly Stream _fileStream;

	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }

	/// <summary>
	/// The GIF header (signature and version)
	/// </summary>
	internal Header? Header { get; private set; }

	/// <summary>
	/// The Logical Screen Descriptor
	/// </summary>
	internal LogicalScreenDescriptor? LogicalScreenDescriptor { get; private set; }

	/// <summary>
	/// The Global Color Table (if present)
	/// </summary>
	internal ColorTable? GlobalColorTable { get; private set; }

	/// <summary>
	/// List of frames (images) in the GIF
	/// </summary>
	internal List<GifFrame> Frames { get; private set; } = new();

	/// <summary>
	/// List of application extensions
	/// </summary>
	internal List<ApplicationExtension> ApplicationExtensions { get; private set; } = new();

	/// <summary>
	/// List of comment extensions
	/// </summary>
	internal List<CommentExtension> CommentExtensions { get; private set; } = new();

	/// <summary>
	/// Whether the file is a valid GIF
	/// </summary>
	public bool IsValid { get; private set; }

	/// <summary>
	/// Whether this is an animated GIF (multiple frames)
	/// </summary>
	public bool IsAnimated => Frames.Count > 1;

	/// <summary>
	/// The GIF version string ("87a" or "89a")
	/// </summary>
	public string Version => Header?.Version ?? "";

	/// <summary>
	/// Whether this is GIF89a format
	/// </summary>
	public bool IsGif89a => Header?.IsGif89a ?? false;

	public GifStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		_fileStream = fileStream;
		Log = log;
		ValidationReport = validationReport;
	}

	public bool Read()
	{
		FileReader fr = new FileReader(_fileStream, Endian.Little);

		// Read Header
		Header = new Header(fr);
		Log.Info($"Read GIF header: {Header.Signature}{Header.Version}");

		// Validate signature
		if (Header.Signature != "GIF")
		{
			ValidationReport.Error("Invalid GIF signature");
			return false;
		}

		// Validate version
		if (!Header.IsGif87a && !Header.IsGif89a)
		{
			ValidationReport.Warning($"Unknown GIF version: {Header.Version}");
		}

		// Read Logical Screen Descriptor
		LogicalScreenDescriptor = new LogicalScreenDescriptor(fr);
		Log.Info($"Logical Screen: {LogicalScreenDescriptor.Width}x{LogicalScreenDescriptor.Height}");

		// Read Global Color Table if present
		if (LogicalScreenDescriptor.GlobalColorTableFlag)
		{
			GlobalColorTable = new ColorTable(fr, LogicalScreenDescriptor.SizeOfGlobalColorTable);
			Log.Info($"Global Color Table: {GlobalColorTable.ColorCount} colors");
		}

		// Read blocks until trailer
		GraphicControlExtension? pendingGraphicControl = null;

		while (_fileStream.Position < _fileStream.Length)
		{
			byte blockType = fr.ReadByte();

			if (blockType == Constants.BLOCK_TRAILER)
			{
				// End of GIF
				Log.Info("Read GIF trailer");
				break;
			}
			else if (blockType == Constants.BLOCK_EXTENSION)
			{
				// Extension block
				byte extensionLabel = fr.ReadByte();
				// Rewind to let the extension read from the start
				_fileStream.Seek(-2, SeekOrigin.Current);

				switch (extensionLabel)
				{
					case Constants.EXT_GRAPHIC_CONTROL:
						pendingGraphicControl = new GraphicControlExtension(fr);
						Log.LogInformation($"Graphic Control Extension: delay={pendingGraphicControl.DelayTime}");
						break;

					case Constants.EXT_APPLICATION:
						var appExt = new ApplicationExtension(fr);
						ApplicationExtensions.Add(appExt);
						Log.LogInformation($"Application Extension: {appExt.ApplicationIdentifierString}");
						break;

					case Constants.EXT_COMMENT:
						var commentExt = new CommentExtension(fr);
						CommentExtensions.Add(commentExt);
						Log.LogInformation($"Comment Extension: {commentExt.CommentText.Length} chars");
						break;

					case Constants.EXT_PLAIN_TEXT:
						var plainTextExt = new PlainTextExtension(fr);
						Log.LogInformation($"Plain Text Extension: {plainTextExt.TextContent.Length} chars");
						break;

					default:
						// Unknown extension - skip it
						ValidationReport.Warning($"Unknown extension label: 0x{extensionLabel:X2}");
						SkipUnknownExtension(fr);
						break;
				}
			}
			else if (blockType == Constants.BLOCK_IMAGE_DESCRIPTOR)
			{
				// Image Descriptor - rewind to include separator
				_fileStream.Seek(-1, SeekOrigin.Current);

				var frame = ReadFrame(fr, pendingGraphicControl);
				Frames.Add(frame);
				Log.Info($"Frame {frame.FrameIndex}: {frame.ImageDescriptor.Width}x{frame.ImageDescriptor.Height}");

				pendingGraphicControl = null;
			}
			else
			{
				// Unknown block type
				ValidationReport.Warning($"Unknown block type: 0x{blockType:X2} at position {_fileStream.Position - 1}");
				break;
			}
		}

		IsValid = true;
		Log.Info($"GIF parsed: {Frames.Count} frames");
		return true;
	}

	private GifFrame ReadFrame(FileReader fr, GraphicControlExtension? graphicControl)
	{
		var frame = new GifFrame
		{
			FrameIndex = Frames.Count,
			GraphicControlExtension = graphicControl
		};

		// Read Image Descriptor
		frame.ImageDescriptor = new ImageDescriptor(fr);

		// Read Local Color Table if present
		if (frame.ImageDescriptor.LocalColorTableFlag)
		{
			frame.LocalColorTable = new ColorTable(fr, frame.ImageDescriptor.SizeOfLocalColorTable);
		}

		// Read Table Based Image Data
		frame.ImageData = new TableBasedImageData(fr);

		return frame;
	}

	private void SkipUnknownExtension(FileReader fr)
	{
		// Already read the extension introducer and label
		fr.ReadByte(); // Extension introducer (0x21)
		fr.ReadByte(); // Extension label

		// Skip sub-blocks
		while (true)
		{
			byte blockSize = fr.ReadByte();
			if (blockSize == 0)
				break;
			fr.ReadBytes(blockSize);
		}
	}
}
