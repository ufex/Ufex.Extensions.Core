using System;
using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// colr — Colour Information node. Displays nclx parameters or ICC profile info.
/// </summary>
internal class ColrBoxNode : BoxNode
{
	public ColrBoxNode(ColrBox box)
		: base(box, "colr", "Colour Information", TreeViewIcon.Palette)
	{
	}

	public override object[][] GetRows()
	{
		var box = (ColrBox)_box;
		var rows = new List<object[]>();

		rows.Add([ "Colour Type", box.ColourType, box.ColourTypeString ]);

		string colourTypeStr = box.ColourTypeString;

		if (colourTypeStr == "nclx")
		{
			string primariesDesc = ColrBox.ColourPrimariesNames.GetValueOrDefault(box.ColourPrimaries, "");
			string transferDesc = ColrBox.TransferNames.GetValueOrDefault(box.TransferCharacteristics, "");
			string matrixDesc = ColrBox.MatrixNames.GetValueOrDefault(box.MatrixCoefficients, "");

			rows.Add([ "Colour Primaries", box.ColourPrimaries, primariesDesc ]);
			rows.Add([ "Transfer Characteristics", box.TransferCharacteristics, transferDesc ]);
			rows.Add([ "Matrix Coefficients", box.MatrixCoefficients, matrixDesc ]);
			rows.Add([ "Full Range", box.FullRangeFlag ? (Byte)1 : (Byte)0, box.FullRangeFlag ? "Full range (0–255)" : "Limited range (16–235)" ]);
		}
		else if (colourTypeStr == "rICC" || colourTypeStr == "prof")
		{
			rows.Add([ "ICC Profile Size", (UInt32)box.IccProfileLength, ByteCountFormatter.Format((UInt64)box.IccProfileLength) ]);
		}

		return rows.ToArray();
	}
}
