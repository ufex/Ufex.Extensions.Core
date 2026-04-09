using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// hdlr — Handler Reference Box node. Displays the media handler type.
/// </summary>
internal class HdlrBoxNode : BoxNode
{
	public HdlrBoxNode(HdlrBox box)
		: base(box, "hdlr", "Handler Reference", TreeViewIcon.Gear)
	{
	}

	public override object[][] GetRows()
	{
		var hdlr = (HdlrBox)_box;
		string handlerDesc = BoxTypes.HandlerTypes.GetValueOrDefault(hdlr.HandlerTypeString, "");

		return [
			[ "Pre-defined", hdlr.PreDefined, hdlr.PreDefined != 0 ? $"QTFF component type: {hdlr.PreDefinedString}" : "" ],
			[ "Handler Type", hdlr.HandlerType, handlerDesc ],
			[ "Reserved", hdlr.Reserved, "" ],
			[ "Name", hdlr.Name, hdlr.NameString ],
		];
	}
}
