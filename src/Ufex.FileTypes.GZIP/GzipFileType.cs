using System;
using System.Collections.Generic;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Visual;

namespace Ufex.FileTypes.GZIP;

/// <summary>
/// GZIP FileType module for Ufex.
/// Parses GZIP files according to the GZIP specification.
/// </summary>
public class GzipFileType : FileType
{
	public GzipFileType()
	{
		ShowGraphic = true;
		ShowTechnical = true;
		ShowFileCheck = true;
		Log.SetLogName("GZIP.log");
	}

	public override bool ProcessFile()
	{
		var reader = new GzipStreamReader(FileInStream, Log, ValidationReport);
		bool result = reader.Read();

		if (!result)
		{
			ValidationReport.Error("Failed to parse GZIP file");
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
	private void BuildQuickInfo(GzipStreamReader reader)
	{
		var quickInfo = QuickInfoTable;
		quickInfo.AddRow("Number of Files", reader.Members.Count.ToString());
	}

	/// <summary>
	/// Builds the Visuals tab with file map
	/// </summary>
	private void BuildVisuals(GzipStreamReader reader)
	{

	}

	/// <summary>
	/// Builds the Structure tab tree view
	/// </summary>
	private void BuildStructure(GzipStreamReader reader)
	{
		foreach(var member in reader.Members)
		{
			var memberNode = new MemberNode(member);
			TreeNodes.Add(memberNode);
		}
	}
}