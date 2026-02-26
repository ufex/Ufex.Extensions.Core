using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Visual;

using Ufex.Extensions.Core.INI.Data;
using Ufex.Extensions.Core.INI.Structure;

namespace Ufex.Extensions.Core.INI;

/// <summary>
/// INI FileType module for Ufex.
/// Parses INI configuration files according to the Core INI specification.
/// </summary>
public class IniFileType : FileType
{
	public IniFileType()
	{
		ShowGraphic = true;
		ShowTechnical = true;
		ShowFileCheck = true;
	}

	public override bool ProcessFile()
	{
		var reader = new IniStreamReader(FileInStream, Log, ValidationReport);
		bool result = reader.Read();

		if (!result)
		{
			ValidationReport.Error("Failed to parse INI file");
			return false;
		}

		BuildQuickInfo(reader);
		BuildVisuals(reader);
		BuildStructure(reader);

		return true;
	}

	/// <summary>
	/// Builds the Info tab quick information display
	/// </summary>
	private void BuildQuickInfo(IniStreamReader reader)
	{
		var quickInfo = QuickInfoTable;

		quickInfo.AddRow("Sections", reader.Sections.Count.ToString());
		quickInfo.AddRow("Properties", reader.PropertyCount.ToString());
		quickInfo.AddRow("Comments", reader.CommentCount.ToString());
		quickInfo.AddRow("Total Lines", reader.Lines.Count.ToString());

		if (reader.InvalidLines.Count > 0)
		{
			quickInfo.AddRow("Invalid Lines", reader.InvalidLines.Count.ToString());
		}

		quickInfo.AddRow("Encoding", reader.DetectedEncoding?.EncodingName ?? "Unknown");
		quickInfo.AddRow("Has BOM", reader.HasBom ? "Yes" : "No");

		// Show first few section names
		var sectionNames = new List<string>();
		foreach (var section in reader.Sections)
		{
			if (sectionNames.Count >= 5) break;
			sectionNames.Add(section.IsGlobal ? "(Global)" : section.Name);
		}
		if (sectionNames.Count > 0)
		{
			string sectionList = string.Join(", ", sectionNames);
			if (reader.Sections.Count > 5)
				sectionList += $", ... (+{reader.Sections.Count - 5} more)";
			quickInfo.AddRow("Sections", sectionList);
		}
	}

	/// <summary>
	/// Builds the Visuals tab with file map
	/// </summary>
	private void BuildVisuals(IniStreamReader reader)
	{
		var spans = new List<FileSpan>();

		foreach (var section in reader.Sections)
		{
			if (section.Lines.Count == 0)
				continue;

			string name = section.IsGlobal ? "(Global)" : $"[{section.Name}]";
			spans.Add(new FileSpan
			{
				StartPosition = section.Offset,
				EndPosition = section.Offset + section.Length,
				Name = name
			});
		}

		if (spans.Count > 0)
		{
			var map = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
			VisualsList.Add(map);
		}

		// Add a summary table visual
		var summaryTable = BuildSummaryTable(reader);
		VisualsList.Add(new DataGridVisual(summaryTable, "Section Summary"));
	}

	/// <summary>
	/// Builds a summary table of all sections and their properties
	/// </summary>
	private DynamicTableData BuildSummaryTable(IniStreamReader reader)
	{
		var td = new DynamicTableData(3, "INI.SectionSummary");
		td.SetColumn(0, "Section");
		td.SetColumn(1, "Properties");
		td.SetColumn(2, "Lines");

		foreach (var section in reader.Sections)
		{
			string name = section.IsGlobal ? "(Global)" : section.Name;
			td.AddRow(name, section.Properties.Count, section.Lines.Count);
		}

		return td;
	}

	/// <summary>
	/// Builds the Structure tab tree view
	/// </summary>
	private void BuildStructure(IniStreamReader reader)
	{
		foreach (var section in reader.Sections)
		{
			Log.LogInformation($"Adding section node: {(section.IsGlobal ? "(Global)" : section.Name)}");
			var sectionNode = new SectionNode(section);
			TreeNodes.Add(sectionNode);
		}

		// If there are invalid lines outside of sections, add them separately
		foreach (var invalidLine in reader.InvalidLines)
		{
			// Check if this line is already in a section
			bool inSection = false;
			foreach (var section in reader.Sections)
			{
				if (section.Lines.Contains(invalidLine))
				{
					inSection = true;
					break;
				}
			}

			if (!inSection)
			{
				TreeNodes.Add(new InvalidLineNode(invalidLine));
			}
		}
	}
}