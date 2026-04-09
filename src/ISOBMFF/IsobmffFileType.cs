using System;
using System.Linq;

using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF;

/// <summary>
/// FileType handler for ISO Base Media File Format (ISOBMFF) files.
/// ISOBMFF files require an ftyp box and follow stricter structural rules
/// than QTFF. This covers MP4, M4A, 3GP, HEIF, and other ISOBMFF derivatives.
/// </summary>
public class IsoBmffFileType : BaseIsobmffFileType
{
	public IsoBmffFileType()
	{
	}

	protected override void ValidateFile(BoxStreamReader reader)
	{
		// ISOBMFF requires ftyp
		var ftyp = FindBox<FtypBox>(reader.Boxes);
		if (ftyp == null)
		{
			ValidationReport.Warning("No 'ftyp' box found. ISOBMFF files should begin with a File Type box.");
		}
		else
		{
			// ftyp should be the first significant box
			var firstBox = reader.Boxes.FirstOrDefault();
			if (firstBox != null && firstBox.TypeString != "ftyp")
			{
				ValidationReport.Warning($"First box is '{firstBox.TypeString}', not 'ftyp'. ISOBMFF files should have 'ftyp' as the first box.");
			}
		}

		// Check for moov
		bool hasMoov = reader.Boxes.Any(b => b.TypeString == "moov");
		if (!hasMoov)
		{
			ValidationReport.Warning("No 'moov' box found. The file may be incomplete or a standalone segment.");
		}

		// Check for QTFF-specific atoms that are invalid in ISOBMFF
		var qtffOnlyTypes = new[] { "wide", "clip", "matt", "load", "imap", "kmat", "ctab", "prof" };
		CheckForQtffAtoms(reader.Boxes, qtffOnlyTypes);
	}

	private void CheckForQtffAtoms(System.Collections.Generic.List<Box> boxes, string[] qtffTypes)
	{
		foreach (var box in boxes)
		{
			string typeStr = box.TypeString.Trim();
			if (qtffTypes.Contains(typeStr))
			{
				ValidationReport.Info($"Found QTFF-specific atom '{typeStr}' at offset {box.Offset}. This atom is not part of the ISOBMFF specification.");
			}
			CheckForQtffAtoms(box.Children, qtffTypes);
		}
	}
}
