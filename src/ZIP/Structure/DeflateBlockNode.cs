using System.Collections.Generic;
using Ufex.API.Tree;
using Ufex.API.Visual;

namespace Ufex.Extensions.Core.ZIP.Structure;

public class DeflateBlockNode : TreeNode
{
	public DeflateStreamReader.Block Block;

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get 
		{ 
			if(Block.Type != DeflateStreamReader.BlockType.DynamicHuffman || Block.LiteralCodeMap == null || Block.DistanceCodeMap == null)
			{
				return [];
			}
			return [ new TreeDiagramVisual(BuildVisualTree(Block.LiteralCodeMap), "Deflate Block Structure") ];
		}
	}

	public DeflateBlockNode(DeflateStreamReader.Block block)
		: base("Deflate Block", TreeViewIcon.Table, TreeViewIcon.Table)
	{
		Block = block;
		if(block.Type == DeflateStreamReader.BlockType.Stored)
		{
			Text = "Stored Block";
		}
		else if(block.Type == DeflateStreamReader.BlockType.DynamicHuffman)
		{
			Text = "Dynamic Huffman Block";
		}
		else if(block.Type == DeflateStreamReader.BlockType.StaticHuffman)
		{
			Text = "Static Huffman Block";
		}
		else
		{
			Text = "Reserved Block";
		}
	}

	private static TreeDiagramVisual.Node BuildVisualTree(Dictionary<int, (int code, int len)> codeMap)
	{
		TreeDiagramVisual.Node root = new TreeDiagramVisual.Node("Root");
		foreach (var entry in codeMap)
		{
			if (entry.Value.len == 0) continue;
			TreeDiagramVisual.Node current = root;
			for (int i = entry.Value.len - 1; i >= 0; i--)
			{
				int bit = (entry.Value.code >> i) & 1;
				while (current.Children.Count < 2) current.Children.Add(new TreeDiagramVisual.Node(current.Children.Count.ToString()));
				current = current.Children[bit];
			}
			string hex = $"0x{entry.Key:X2}";
			string ascii = (entry.Key >= 32 && entry.Key <= 126) ? $" '{(char)entry.Key}'" : "";
			current.Label = $"{hex}{ascii}";
			current.Children = null;
		}
		return root;
	}
}
