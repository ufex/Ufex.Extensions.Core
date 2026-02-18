using Ufex.API.Tree;
using Ufex.Extensions.Core.GIF.Data;

namespace Ufex.Extensions.Core.GIF.Structure;

/// <summary>
/// Application Extension node
/// </summary>
internal class ApplicationExtensionNode : BlockNode
{
	private readonly ApplicationExtension _extension;

	public ApplicationExtensionNode(ApplicationExtension extension)
		: base("Application Extension", TreeViewIcon.Gear, extension.Offset)
	{
		_extension = extension;
		Description = $"Application Extension - {extension.ApplicationIdentifierString}";
	}

	protected override List<object[]> GetRows()
	{
		var rows = new List<object[]>
		{
			new object[] { "Extension Introducer", $"0x{_extension.ExtensionIntroducer:X2}", "Always 0x21" },
			new object[] { "Extension Label", $"0x{_extension.ExtensionLabel:X2}", "Always 0xFF" },
			new object[] { "Block Size", _extension.BlockSize, "Always 11" },
			new object[] { "Application Identifier", _extension.ApplicationIdentifierString, "8-character identifier" },
			new object[] { "Authentication Code", _extension.AuthenticationCodeString, "3-byte code" },
			new object[] { "Data Blocks", _extension.DataBlocks.Count, $"{_extension.DataBlocks.Count} sub-blocks" }
		};

		// Add NETSCAPE-specific info
		if (_extension.IsNetscapeExtension && _extension.LoopCount.HasValue)
		{
			var loopDesc = _extension.LoopCount == 0 ? "Infinite loop" : $"{_extension.LoopCount} loops";
			rows.Add(new object[] { "Loop Count", _extension.LoopCount.Value, loopDesc });
		}

		return rows;
	}
}
