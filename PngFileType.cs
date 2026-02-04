using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.FileTypes.PNG.Data;
using Ufex.FileTypes.PNG.Structure;

namespace Ufex.FileTypes.PNG;

/// <summary>
/// PNG FileType module for Ufex
/// </summary>
public class PngFileType : FileType
{

	// PNG color type to pixel format description mapping
	static readonly Dictionary<int, Dictionary<int, string>> COLOR_TYPE_FORMATS = new Dictionary<int, Dictionary<int, string>>()
	{
		{ 0, new Dictionary<int, string> // Grayscale
			{
				{ 1, "1-bpp Grayscale" },
				{ 2, "2-bpp Grayscale" },
				{ 4, "4-bpp Grayscale" },
				{ 8, "8-bpp Grayscale" },
				{ 16, "16-bpp Grayscale" }
			}
		},
		{ 2, new Dictionary<int, string> // RGB
			{
				{ 8, "24-bpp RGB" },
				{ 16, "48-bpp RGB" }
			}
		},
		{ 3, new Dictionary<int, string> // Indexed
			{
				{ 1, "1-bpp Indexed" },
				{ 2, "2-bpp Indexed" },
				{ 4, "4-bpp Indexed" },
				{ 8, "8-bpp Indexed" }
			}
		},
		{ 4, new Dictionary<int, string> // Grayscale + Alpha
			{
				{ 8, "16-bpp Grayscale+Alpha" },
				{ 16, "32-bpp Grayscale+Alpha" }
			}
		},
		{ 6, new Dictionary<int, string> // RGBA
			{
				{ 8, "32-bpp RGBA" },
				{ 16, "64-bpp RGBA" }
			}
		}
	};

	protected Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
	protected FileMap? Map { get; set; }
	protected int Width { get; set; }	
	protected int Height { get; set; }

	public PngFileType()
	{
		ShowGraphic = true;
		ShowTechnical = true;
		ShowFileCheck = false;
		Log.SetLogName("PNG.log");
	}

	public override bool ProcessFile()
	{
		PngStreamReader pngReader = new PngStreamReader(FileInStream, Log, ValidationReport);
		bool result = pngReader.Read();

		// Extract metadata from chunks
		int colorType = pngReader.ColorType;
		int bitDepth = -1;
		foreach(Chunk chunk in pngReader.Chunks)
		{
			ExtractChunkMetadata(chunk.ChunkTypeString, chunk, ref colorType, ref bitDepth);
		}

		// Determine pixel format from color type and bit depth
		if (colorType >= 0 && bitDepth > 0)
		{
			if (COLOR_TYPE_FORMATS.TryGetValue(colorType, out var bitDepthFormats) &&
				bitDepthFormats.TryGetValue(bitDepth, out var pixelFormat))
			{
				Metadata["Pixel Format"] = pixelFormat;
			}
			else
			{
				Metadata["Pixel Format"] = $"Color Type {colorType}, {bitDepth}-bit";
			}
		}

		BuildQuickInfo(pngReader);
		BuildVisuals(pngReader);
		BuildStructure(pngReader);

		return true;
	}

	/// <summary>
	/// Extracts metadata from parsed chunks and stores in Width/Height/Metadata properties.
	/// </summary>
	private void ExtractChunkMetadata(string chunkType, Chunk chunk, ref int colorType, ref int bitDepth)
	{
		switch (chunkType)
		{
			case "IHDR":
				var ihdr = (IhdrChunk)chunk;
				Width = (int)ihdr.Width;
				Height = (int)ihdr.Height;
				colorType = ihdr.ColorType;
				bitDepth = ihdr.BitDepth;
				break;

			case "gAMA":
				var gama = (GamaChunk)chunk;
				// PNG gamma is stored as gamma * 100000
				double gamma = gama.Gamma / 100000.0;
				Metadata["Gamma"] = gamma.ToString("F5");
				break;

			case "pHYs":
				var phys = (PhysChunk)chunk;
				string unitName = phys.Unit == 1 ? "Metre" : "Unknown";
				Metadata["Pixel Unit"] = unitName;
				Metadata["Pixels per unit X"] = phys.PixelsPerUnitX.ToString();
				Metadata["Pixels per unit Y"] = phys.PixelsPerUnitY.ToString();
				break;

			case "tEXt":
				var text = (TextChunk)chunk;
				string keyword = text.KeywordString;
				string value = text.TextStringString;
				
				// Map known PNG text keywords to metadata
				if (keyword.Equals("Software", StringComparison.OrdinalIgnoreCase))
				{
					Metadata["Software"] = value;
				}
				else if (keyword.Equals("Creation Time", StringComparison.OrdinalIgnoreCase) ||
							keyword.Equals("Date", StringComparison.OrdinalIgnoreCase))
				{
					Metadata["Date/Time"] = value;
				}
				else
				{
					// Store other text metadata with the keyword as the key
					Metadata[keyword] = value;
				}
				break;

			case "tIME":
				var time = (TimeChunk)chunk;
				string dateTime = $"{time.Year:D4}-{time.Month:D2}-{time.Day:D2} {time.Hour:D2}:{time.Minute:D2}:{time.Second:D2}";
				Metadata["Last Modified"] = dateTime;
				break;
		}
	}

	public void BuildQuickInfo(PngStreamReader pngReader)
	{
		var quickInfo = QuickInfoTable;

		// Add width/height from inherited properties
		quickInfo.AddRow("Width", Width.ToString());
		quickInfo.AddRow("Height", Height.ToString());

		// Define the metadata keys to include in the quick info (in display order)
		string[] metadataKeys =
		[
			"Pixel Format",
			"Gamma",
			"Pixel Unit",
			"Software",
			"Date/Time",
			"Last Modified"
		];

		// Add metadata rows if available
		for (int i = 0; i < metadataKeys.Length; i++)
		{
			if (Metadata.TryGetValue(metadataKeys[i], out string value))
				quickInfo.AddRow(metadataKeys[i], value);
		}
	}

	protected void BuildVisuals(PngStreamReader pngReader)
	{
		var spans = new List<FileSpan>();
		foreach(Chunk chunk in pngReader.Chunks)
		{
			ChunkNode node = ChunkNode.FromChunk(chunk);
			spans.Add(new FileSpan
			{
				StartPosition = chunk.Offset,
				EndPosition = chunk.Offset + chunk.Length + 12, // Include length, type, CRC
				Name = node.Description
			});
		}
		
		// Create the FileMap from collected spans
		Map = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
		VisualsList.Add(Map);
	}

	protected void BuildStructure(PngStreamReader pngReader) 
	{
		foreach(Chunk chunk in pngReader.Chunks)
		{
			Log.Info($"Processing chunk at position {chunk.Offset}, type {chunk.GetType().Name}");
			ChunkNode node = ChunkNode.FromChunk(chunk);
			TreeNodes.Add(node);
		}
	}
}