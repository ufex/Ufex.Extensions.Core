using System.IO;
using System.Text;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// DQT - Define Quantization Table marker segment node.
/// Displays quantization table metadata for each table in the segment.
/// </summary>
internal class DqtSegmentNode : SegmentNode
{
	/// <summary>
	/// JPEG zigzag order mapping: index in the 1D array -> position in the 8x8 block.
	/// Maps each zigzag index to (row * 8 + col).
	/// </summary>
	private static readonly int[] ZigzagOrder =
	[
		 0,  1,  8, 16,  9,  2,  3, 10,
		17, 24, 32, 25, 18, 11,  4,  5,
		12, 19, 26, 33, 40, 48, 41, 34,
		27, 20, 13,  6,  7, 14, 21, 28,
		35, 42, 49, 56, 57, 50, 43, 36,
		29, 22, 15, 23, 30, 37, 44, 51,
		58, 59, 52, 45, 38, 31, 39, 46,
		53, 60, 61, 54, 47, 55, 62, 63
	];

	public DqtSegmentNode(DqtSegment segment)
		: base(segment, "DQT", "Define Quantization Table", TreeViewIcon.Table)
	{
	}

	public override Visual[] Visuals
	{
		get
		{
			var d = (DqtSegment)Segment;
			var visuals = new List<Visual>();

			visuals.Add(new DataGridVisual(TableData(), "Data"));

			for (int i = 0; i < d.TableCount; i++)
			{
				int precision = d.GetPrecision(i);
				int tableId = d.GetTableId(i);
				int[,] block = DeZigZag(d.TableData[i], precision);
				string label = $"Quantization Table {tableId}";
				string svg = RenderHeatmapSvg(block, label);

				var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
				var image = new VectorImage(stream, label);
				visuals.Add(image);
			}

			return visuals.ToArray();
		}
	}

	protected override object[][] GetRows()
	{
		var d = (DqtSegment)Segment;
		var rows = new List<object[]>();

		for (int i = 0; i < d.TableCount; i++)
		{
			int precision = d.GetPrecision(i);
			int tableId = d.GetTableId(i);
			string precisionDesc = precision == 0 ? "8-bit" : "16-bit";

			rows.Add([$"Table {i} Info", d.TableInfos[i], $"Precision: {precisionDesc}, ID: {tableId}"]);
			rows.Add([$"Table {i} Data", d.TableData[i], $"{d.TableData[i].Length} bytes ({precisionDesc} values)"]);
		}

		return rows.ToArray();
	}

	/// <summary>
	/// De-zigzags a 1D quantization table array into an 8x8 block.
	/// </summary>
	private static int[,] DeZigZag(byte[] data, int precision)
	{
		int[,] block = new int[8, 8];

		for (int i = 0; i < 64; i++)
		{
			int value;
			if (precision == 0)
			{
				value = data[i];
			}
			else
			{
				value = (data[i * 2] << 8) | data[i * 2 + 1];
			}

			int pos = ZigzagOrder[i];
			int row = pos / 8;
			int col = pos % 8;
			block[row, col] = value;
		}

		return block;
	}

	/// <summary>
	/// Renders an 8x8 quantization table as an SVG heatmap.
	/// </summary>
	private static string RenderHeatmapSvg(int[,] block, string title)
	{
		const int cellSize = 50;
		const int labelWidth = 40;
		const int titleHeight = 30;
		const int axisLabelHeight = 20;
		const int legendWidth = 60;
		int gridSize = 8 * cellSize;
		int svgWidth = labelWidth + gridSize + legendWidth + 10;
		int svgHeight = titleHeight + axisLabelHeight + gridSize + axisLabelHeight + 10;

		// Find min/max values
		int minVal = int.MaxValue;
		int maxVal = int.MinValue;
		for (int r = 0; r < 8; r++)
		{
			for (int c = 0; c < 8; c++)
			{
				int v = block[r, c];
				if (v < minVal) minVal = v;
				if (v > maxVal) maxVal = v;
			}
		}

		var sb = new StringBuilder();
		sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{svgWidth}\" height=\"{svgHeight}\" font-family=\"sans-serif\">");
		sb.AppendLine($"<rect width=\"{svgWidth}\" height=\"{svgHeight}\" fill=\"white\"/>");

		// Title
		sb.AppendLine($"<text x=\"{labelWidth + gridSize / 2}\" y=\"{titleHeight - 8}\" text-anchor=\"middle\" font-size=\"14\" font-weight=\"bold\">{title}</text>");

		int gridX = labelWidth;
		int gridY = titleHeight + axisLabelHeight;

		// Column headers
		for (int c = 0; c < 8; c++)
		{
			int cx = gridX + c * cellSize + cellSize / 2;
			sb.AppendLine($"<text x=\"{cx}\" y=\"{gridY - 5}\" text-anchor=\"middle\" font-size=\"11\" fill=\"#555\">{c}</text>");
		}

		// Axis label - Horizontal
		sb.AppendLine($"<text x=\"{gridX + gridSize / 2}\" y=\"{gridY + gridSize + 18}\" text-anchor=\"middle\" font-size=\"11\" fill=\"#555\">Horizontal Spatial Frequency</text>");

		// Axis label - Vertical (rotated)
		sb.AppendLine($"<text x=\"12\" y=\"{gridY + gridSize / 2}\" text-anchor=\"middle\" font-size=\"11\" fill=\"#555\" transform=\"rotate(-90, 12, {gridY + gridSize / 2})\">Vertical Spatial Frequency</text>");

		// Draw cells
		for (int r = 0; r < 8; r++)
		{
			// Row label
			int ry = gridY + r * cellSize + cellSize / 2 + 4;
			sb.AppendLine($"<text x=\"{gridX - 5}\" y=\"{ry}\" text-anchor=\"end\" font-size=\"11\" fill=\"#555\">{r}</text>");

			for (int c = 0; c < 8; c++)
			{
				int value = block[r, c];
				string color = HeatmapColor(value, minVal, maxVal);
				int x = gridX + c * cellSize;
				int y = gridY + r * cellSize;

				sb.AppendLine($"<rect x=\"{x}\" y=\"{y}\" width=\"{cellSize}\" height=\"{cellSize}\" fill=\"{color}\" stroke=\"#fff\" stroke-width=\"1\"/>");

				// Text color: white on dark cells, black on light
				double t = maxVal > minVal ? (double)(value - minVal) / (maxVal - minVal) : 0;
				string textColor = t > 0.5 ? "#fff" : "#000";
				int tx = x + cellSize / 2;
				int ty = y + cellSize / 2 + 5;
				sb.AppendLine($"<text x=\"{tx}\" y=\"{ty}\" text-anchor=\"middle\" font-size=\"12\" fill=\"{textColor}\">{value}</text>");
			}
		}

		// Legend
		int legendX = gridX + gridSize + 15;
		int legendH = gridSize;
		int legendW = 15;
		int steps = 20;
		for (int i = 0; i < steps; i++)
		{
			double t = (double)i / (steps - 1);
			int val = minVal + (int)(t * (maxVal - minVal));
			string color = HeatmapColor(val, minVal, maxVal);
			int ly = gridY + (int)(t * (legendH - legendH / steps));
			int lh = legendH / steps + 1;
			sb.AppendLine($"<rect x=\"{legendX}\" y=\"{ly}\" width=\"{legendW}\" height=\"{lh}\" fill=\"{color}\"/>");
		}

		// Legend labels
		sb.AppendLine($"<text x=\"{legendX + legendW + 4}\" y=\"{gridY + 10}\" font-size=\"10\" fill=\"#555\">{minVal}</text>");
		sb.AppendLine($"<text x=\"{legendX + legendW + 4}\" y=\"{gridY + legendH}\" font-size=\"10\" fill=\"#555\">{maxVal}</text>");

		sb.AppendLine("</svg>");
		return sb.ToString();
	}

	/// <summary>
	/// Maps a value to a heatmap color (yellow to dark red).
	/// </summary>
	private static string HeatmapColor(int value, int min, int max)
	{
		double t = max > min ? (double)(value - min) / (max - min) : 0;

		// Yellow (#FFFFCC) -> Orange (#FD8D3C) -> Dark Red (#800026)
		int r, g, b;
		if (t < 0.5)
		{
			double s = t * 2;
			r = 255;
			g = (int)(255 - s * (255 - 141));
			b = (int)(204 - s * (204 - 60));
		}
		else
		{
			double s = (t - 0.5) * 2;
			r = (int)(253 - s * (253 - 128));
			g = (int)(141 - s * 141);
			b = (int)(60 - s * (60 - 38));
		}

		return $"rgb({r},{g},{b})";
	}
}
