using System;

using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF;

/// <summary>
/// FileType handler for QuickTime File Format (QTFF) .mov files.
/// QTFF files may not have an ftyp box and can contain QTFF-specific atoms
/// such as wide, clip, matt, load, imap, ctab, gmhd, and alis.
/// </summary>
public class QtffFileType : BaseIsobmffFileType
{
	public QtffFileType()
	{
	}

	protected override void ValidateFile(BoxStreamReader reader)
	{
		bool hasMoov = false;

		foreach (var box in reader.Boxes)
		{
			if (box.TypeString == "moov")
				hasMoov = true;
		}

		if (!hasMoov)
			ValidationReport.Warning("No 'moov' atom found. The file may be incomplete.");

		// Check for ftyp with QTFF brand
		var ftyp = FindBox<FtypBox>(reader.Boxes);
		if (ftyp != null && ftyp.MajorBrandString != "qt  ")
		{
			ValidationReport.Info($"ftyp major brand is '{ftyp.MajorBrandString}', not 'qt  '. This file may be ISOBMFF rather than QTFF.");
		}
	}
}
