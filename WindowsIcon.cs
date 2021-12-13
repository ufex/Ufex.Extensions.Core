using System;
using System.Windows.Forms;
using System.IO;

using Ufex.API;
using Ufex.API.Tables;

namespace Ufex.FileTypes.Image
{
	/// <summary>
	/// Ufex FileType module for Windows Icon files
	/// </summary>
	public class WindowsIcon : ImageFileType
	{
		struct IconDirEntry
		{
			public Byte bWidth;
			public Byte bHeight;
			public Byte bColorCount;
			public Byte bReserved;
			public UInt16 wPlanes;
			public UInt16 wBitCount;
			public UInt32 dwBytesInRes;
			public UInt32 dwImageOffset;
		}

		struct IconImage
		{
			public BitmapHeader3 icHeader;          // DIB header
			public RGBR[] icColors;                 // Color table
			public Byte[] icXOR;                    // DIB bits for XOR mask
			public Byte[] icAND;                    // DIB bits for AND mask
		}
		UInt16 idReserved;		// Should always be zero
		UInt16 idType;			// Should always be one
		UInt16 idCount;			// Number of icons in the file

		IconDirEntry[] idEntries;
		IconImage[] iconImages;

		public WindowsIcon()
		{
			Log.SetLogName("UFEModule_ICON.log");

			// Set which tabs to display
			ShowTechnical = true;
			ShowGraphic = true;
			ShowFileCheck = true;
		}

		public override bool ProcessFile()
		{
			// Set the FileStream Position to 0
			m_FileStream.Position = 0;
			BinaryReader br = new BinaryReader(m_FileStream);
			
			idReserved = br.ReadUInt16();
			idType = br.ReadUInt16();
			idCount = br.ReadUInt16();
			idEntries = new IconDirEntry[idCount];

			TreeNode tnIconHeader = new TreeNode("IconDir");
			TreeNode tnEntries = new TreeNode("idEntries");

			// Read in the Icon Directory Entries
			for(int i = 0; i < idCount; i++)
			{
				idEntries[i] = new IconDirEntry();
				idEntries[i].bWidth = br.ReadByte();
				idEntries[i].bHeight = br.ReadByte();
				idEntries[i].bColorCount = br.ReadByte();
				idEntries[i].bReserved = br.ReadByte();
				idEntries[i].wPlanes = br.ReadUInt16();
				idEntries[i].wBitCount = br.ReadUInt16();
				idEntries[i].dwBytesInRes = br.ReadUInt32();
				idEntries[i].dwImageOffset = br.ReadUInt32();
				tnEntries.Nodes.Add(String.Concat("idEntries[", i.ToString(), "]"));
			}
			tnIconHeader.Nodes.Add(tnEntries);
			TreeNodes.Add(tnIconHeader);
			
			// Read in the Icon Images
			TreeNode tnIconImages = new TreeNode("IconImages");
			iconImages = new IconImage[idCount];
			for(int i = 0; i < idCount; i++)
			{		
				TreeNode tnIconImagesI = new TreeNode(String.Concat("IconImages[", i.ToString(), "]"));
				tnIconImagesI.Nodes.Add(new TreeNode("icHeader"));
				tnIconImagesI.Nodes.Add(new TreeNode("icColors"));
				tnIconImagesI.Nodes.Add(new TreeNode("icXOR"));
				tnIconImagesI.Nodes.Add(new TreeNode("icAND"));

				m_FileStream.Position = idEntries[i].dwImageOffset;
				iconImages[i] = new IconImage();
				iconImages[i].icHeader = new BitmapHeader3();
				iconImages[i].icHeader.Size = br.ReadUInt32();
				iconImages[i].icHeader.Width = br.ReadInt32();
				iconImages[i].icHeader.Height = br.ReadInt32();
				iconImages[i].icHeader.Planes = br.ReadUInt16();
				iconImages[i].icHeader.BitsPerPixel = br.ReadUInt16();
				iconImages[i].icHeader.Compression = br.ReadUInt32();
				iconImages[i].icHeader.SizeOfBitmap = br.ReadUInt32();
				iconImages[i].icHeader.HorzResolution = br.ReadInt32();
				iconImages[i].icHeader.VertResolution = br.ReadInt32();
				iconImages[i].icHeader.ColorsUsed = br.ReadUInt32();
				iconImages[i].icHeader.ColorsImportant = br.ReadUInt32();
				
				uint numColors = 0;
		
				if(iconImages[i].icHeader.BitsPerPixel == 1)
				{
					if(iconImages[i].icHeader.ColorsUsed == 0)
						numColors = 2;
					else 
						numColors = iconImages[i].icHeader.ColorsUsed;
				}
				else if(iconImages[i].icHeader.BitsPerPixel == 4)
				{
					if(iconImages[i].icHeader.ColorsUsed == 0)
						numColors = 16;
					else 
						numColors = iconImages[i].icHeader.ColorsUsed;
				}
				else if(iconImages[i].icHeader.BitsPerPixel == 8)
				{
					if(iconImages[i].icHeader.ColorsUsed == 0)
						numColors = 256;
					else 
						numColors = iconImages[i].icHeader.ColorsUsed;
				}
				else
				{
					Log.Info(String.Concat("Invalid BitCount: ", iconImages[i].icHeader.BitsPerPixel.ToString()), "ICON", "ProcessFile()");
				}

				// Read in the colors
				iconImages[i].icColors = new RGBR[numColors];
				for(int c = 0; c < numColors; c++)
				{
					iconImages[i].icColors[c] = new RGBR();
					iconImages[i].icColors[c].Red = br.ReadByte();
					iconImages[i].icColors[c].Green = br.ReadByte();
					iconImages[i].icColors[c].Blue = br.ReadByte();
					iconImages[i].icColors[c].Reserved = br.ReadByte();
				}

				
				// Read the XOR image data
				//int icXORSz = ((iconImages[i].icHeader.biHeight / 2) * iconImages[i].icHeader.biWidth * iconImages[i].icHeader.biBitCount) / 8;
				iconImages[i].icXOR = br.ReadBytes((int)iconImages[i].icHeader.SizeOfBitmap);
				
				// Read the AND image data
				for(int p = 0; p < ((idEntries[i].bHeight / 2) * idEntries[i].bWidth); p++)
				{


				}


				tnIconImages.Nodes.Add(tnIconImagesI);
			}
			TreeNodes.Add(tnIconImages);
			return true;
		}

		public override TableData GetData(TreeNode tn)
		{	
			int tnIndex = tn.Index;
			String tnFullPath = tn.FullPath;
					
			if(tnFullPath.Equals("IconDir"))
			{
				TextTableData td = new TextTableData(2);
				td.SetColumn(0, "Property", 100);
				td.SetColumn(1, "Value", 100);

				td.AddRow("idReserved", NTS.UInt16(idReserved));
				td.AddRow("idType", NTS.UInt16(idType));
				td.AddRow("idCount", NTS.UInt16(idCount));

				return td;
			}
			else if(tnFullPath.Equals("IconDir\\idEntries"))
			{
				return null;
			}
			else if(tnFullPath.StartsWith("IconDir\\idEntries\\idEntries["))
			{
				TextTableData td = new TextTableData(2);
				td.SetColumn(0, "Property", 100);
				td.SetColumn(1, "Value", 90);
				
				td.AddRow("bWidth", NTS.UInt8(idEntries[tnIndex].bWidth));
				td.AddRow("bHeight", NTS.UInt8(idEntries[tnIndex].bHeight));
				td.AddRow("bColorCount", NTS.UInt8(idEntries[tnIndex].bColorCount));
				td.AddRow("bReserved", NTS.UInt8(idEntries[tnIndex].bReserved));
				td.AddRow("wPlanes", NTS.UInt16(idEntries[tnIndex].wPlanes));
				td.AddRow("wBitCount", NTS.UInt16(idEntries[tnIndex].wBitCount));
				td.AddRow("dwBytesInRes", NTS.UInt32(idEntries[tnIndex].dwBytesInRes));
				td.AddRow("dwImageOffset", NTS.UInt32(idEntries[tnIndex].dwImageOffset));
							
				return td;
			}
			else if(tnFullPath.Equals("IconImages"))
			{
				return null;
			}
			else if(tnFullPath.StartsWith("IconImages\\IconImages[") && tnFullPath.EndsWith("]\\icHeader"))
			{
				TextTableData td = new TextTableData(2);
				td.SetColumn(0, "Property", 125);
				td.SetColumn(1, "Value", 90);
				
				int parIndex = tn.Parent.Index;
				
				td.AddRow("biSize", NTS.UInt32(iconImages[parIndex].icHeader.Size));
				td.AddRow("biWidth", NTS.Int32(iconImages[parIndex].icHeader.Width));
				td.AddRow("biHeight", NTS.Int32(iconImages[parIndex].icHeader.Height));
				td.AddRow("biPlanes", NTS.UInt16(iconImages[parIndex].icHeader.Planes));
				td.AddRow("biBitCount", NTS.UInt16(iconImages[parIndex].icHeader.BitsPerPixel));
				td.AddRow("biCompression", NTS.UInt32(iconImages[parIndex].icHeader.Compression));
				td.AddRow("biSizeImage", NTS.UInt32(iconImages[parIndex].icHeader.SizeOfBitmap));
				td.AddRow("biXPelsPerMeter", NTS.Int32(iconImages[parIndex].icHeader.HorzResolution));
				td.AddRow("biYPelsPerMeter", NTS.Int32(iconImages[parIndex].icHeader.VertResolution));
				td.AddRow("biClrUsed", NTS.UInt32(iconImages[parIndex].icHeader.ColorsUsed));
				td.AddRow("biClrImportant", NTS.UInt32(iconImages[parIndex].icHeader.ColorsImportant));

				return td;
			}
			else if(tnFullPath.StartsWith("IconImages\\IconImages[") && tnFullPath.EndsWith("]\\icColors"))
			{			
				TextTableData td = new TextTableData(5);
				td.SetColumn(0, "i", 50, ColumnAlignment.Center);
				td.SetColumn(1, "rgbRed", 75, ColumnAlignment.Center);
				td.SetColumn(2, "rgbGreen", 75, ColumnAlignment.Center);
				td.SetColumn(3, "rgbBlue", 75, ColumnAlignment.Center);
				td.SetColumn(4, "rgbReserved", 75, ColumnAlignment.Center);
				
				int parIndex = tn.Parent.Index;
				td.SetCapacity(iconImages[parIndex].icColors.Length);
				for(UInt16 i = 0; i < iconImages[parIndex].icColors.Length; i++)
				{
					td.AddRow(NTS.UInt16(i), 
						NTS.UInt8(iconImages[parIndex].icColors[i].Red),
						NTS.UInt8(iconImages[parIndex].icColors[i].Green), 
						NTS.UInt8(iconImages[parIndex].icColors[i].Blue), 
						NTS.UInt8(iconImages[parIndex].icColors[i].Reserved));
				}


				return td;
			}
			else if(tnFullPath.StartsWith("IconImages\\IconImages[") && tnFullPath.EndsWith("]\\icXOR"))
			{			
				//myData = new DataTable();
				//DataColumnCollection* columns = myData.Columns;
				//DataRowCollection* rows = myData.Rows;

				//m_ColWidth = 50;
				//int parIndex = tn.get_Parent().get_Index();
				//for(int i = 0; i < iconImages[parIndex].icHeader.biWidth; i++)
				//{
				//	columns.Add(NTS.GetStrInt32(i));
				//}
				
				//DataRow* myRow;
				

			/*	for(UInt16 i = 0; i < iconImages[parIndex].icColors.Count; i++)
				{
					myRow = myData.NewRow();
					myRow.set_Item(columns.get_Item(0), NTS.UInt16(i));

					rows.Add(myRow);
				}
	*/

				return null;
			}
			else
			{

				return null;
			}
		}

		public override QuickInfoTableData GetQuickInfo()
		{
			QuickInfoTableData td = new QuickInfoTableData();

			td.AddRow("Number of Icons", idCount.ToString());
				
			return td;
		}

	}
}
