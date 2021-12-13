using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using Ufex.API;
using Ufex.API.Tables;

namespace Ufex.FileTypes.Image
{
	/// <summary>
	/// Ufex FileType module for Windows Bitmap files
	/// </summary>
	public class WindowsBitmap : ImageFileType
	{
		/* constants for the biCompression field */
		const int BI_RGB = 0;
		const int BI_RLE8 = 1;
		const int BI_RLE4 = 2;
		const int BI_BITFIELDS = 3;
		const int BI_JPEG = 4;
		const int BI_PNG = 5;

		const int INFO_HEAD_SZ3 = 0x28;
		const int INFO_HEAD_SZ4 = 0x6C;
		const int INFO_HEAD_SZ5 = 0x7C;

		BitmapFileHeader bmpFileHeader;
		BitmapHeader3 bmpHeader;

		UInt32 infoHeaderSize;

		uint numColors;
		RGBR[] colors;

		public WindowsBitmap()
		{	
			// Set the description for the file type
			Description = "Windows Bitmap (BMP)";

			// Set the name of the log file
			Log.SetLogName("BMP.log");
			
			// Set which tabs to display
			ShowTechnical = true;
			ShowGraphic = true;
			ShowFileCheck = true;
			UseTreeViewIcons = true;
			SetATNEndian(Endian.Little);
		}

		public override bool ProcessFile()
		{
			FileCheck.Message("Processing " + m_FileStream.Name);

			// Set the FileStream Position to 0
			m_FileStream.Position = 0;

			BinaryReader r = new BinaryReader(m_FileStream);
		
			// Read File Header
			bmpFileHeader = new BitmapFileHeader();
			bmpFileHeader.Type = r.ReadUInt16();
			bmpFileHeader.Size = r.ReadUInt32();
			bmpFileHeader.Reserved1 = r.ReadUInt16();
			bmpFileHeader.Reserved2 = r.ReadUInt16();
			bmpFileHeader.OffBits = r.ReadUInt32();

			CheckFileHeader();

			infoHeaderSize = r.ReadUInt32();
			r.BaseStream.Seek(-4, SeekOrigin.Current);
			if (infoHeaderSize == INFO_HEAD_SZ3)
			{
				bmpHeader = new BitmapHeader3(r);
			}
			else if (infoHeaderSize == INFO_HEAD_SZ4)
			{
				bmpHeader = new BitmapHeader4(r);
			}
			else if (infoHeaderSize == INFO_HEAD_SZ5)
			{
				bmpHeader = new BitmapHeader5(r);
			}
			else
			{
				FileCheck.Error("Invalid bitmap header size: " + infoHeaderSize.ToString());
			}

			if (bmpHeader != null)
			{
				CheckBitmapFileHeader();
				numColors = NumColors();
				UInt32 paletteSize = (UInt32)(bmpFileHeader.OffBits - r.BaseStream.Position);
				UInt32 expectedPaletteSize = numColors * 4;
				if (paletteSize != expectedPaletteSize)
				{
					FileCheck.Error("Expected " + expectedPaletteSize.ToString() + 
						" bytes for palette between header and bitmap data. Header End = " +
						r.BaseStream.Position.ToString() + ", Bitmap Data Start = " + bmpFileHeader.OffBits
					);
				}
			}

			colors = new RGBR[numColors];
			RGBR tempQuad = new RGBR(); 
			int resvColorWarn = 0;
			for(int i = 0; i < numColors; i++)
			{
				tempQuad = new RGBR();
				tempQuad.Red = r.ReadByte();
				tempQuad.Green = r.ReadByte();
				tempQuad.Blue = r.ReadByte();
				tempQuad.Reserved = r.ReadByte();
				colors.SetValue(tempQuad, i);
				if(tempQuad.Reserved != 0)
					resvColorWarn++;
			}

			if(resvColorWarn > 0)
			{
				FileCheck.Warning(String.Concat(resvColorWarn.ToString(), " rgbReserved byte(s) not set to 0x00 in the color table"));
			}
		
			// Temp TreeNodes
			TreeNode tnFile;
			TreeNode tnInfo;

			// Add Children for Headers
			tnFile = new TreeNode("File Header", (int)TreeViewIcon.Header, (int)TreeViewIcon.Header);
			TreeNodes.Add(tnFile);
			if (bmpHeader != null)
			{
				tnInfo = new TreeNode("Info Header", (int)TreeViewIcon.Header, (int)TreeViewIcon.Header);
				TreeNodes.Add(tnInfo);
			}

			TreeNode tnColors = new TreeNode("Colors", (int)TreeViewIcon.Palette, (int)TreeViewIcon.Palette);
			if(numColors > 0)
			{
				TreeNodes.Add(tnColors);
			}

			FileCheck.Message("Finished");

			return true;
		}

		public override TableData GetData(TreeNode tn)
		{	
			int tnIndex = tn.Index;
			string tnFullPath = tn.FullPath;
			
			if(tnFullPath.Equals("File Header"))
			{
				TextTableData td = new TextTableData(2);
				td.SetColumn(0, "Property", 50);
				td.SetColumn(1, "Value", 50);

				td.AddRow("Type", NTS.UInt16(bmpFileHeader.Type));
				td.AddRow("Size", NTS.UInt32(bmpFileHeader.Size));
				td.AddRow("Reserved1", NTS.UInt16(bmpFileHeader.Reserved1));
				td.AddRow("Reserved2", NTS.UInt16(bmpFileHeader.Reserved2));
				td.AddRow("OffBits", NTS.UInt32(bmpFileHeader.OffBits));

				return td;
			}
			else if(tnFullPath.Equals("Info Header"))
			{			
				TextTableData td = new TextTableData(2);
				td.SetColumn(0, "Property", 50);
				td.SetColumn(1, "Value", 50);

				td.AddRow("Size", NTS.UInt32(bmpHeader.Size));
				td.AddRow("Width", NTS.Int32(bmpHeader.Width));
				td.AddRow("Height", NTS.Int32(bmpHeader.Height));
				td.AddRow("Planes", NTS.UInt16(bmpHeader.Planes));
				td.AddRow("BitCount", NTS.UInt16(bmpHeader.BitsPerPixel));
				td.AddRow("Compression", NTS.UInt32(bmpHeader.Compression));
				td.AddRow("SizeImage", NTS.UInt32(bmpHeader.SizeOfBitmap));
				td.AddRow("XPelsPerMeter", NTS.Int32(bmpHeader.HorzResolution));
				td.AddRow("YPelsPerMeter", NTS.Int32(bmpHeader.VertResolution));
				td.AddRow("ClrUsed", NTS.UInt32(bmpHeader.ColorsUsed));
				td.AddRow("ClrImportant", NTS.UInt32(bmpHeader.ColorsImportant));
				if(bmpHeader.GetType() == typeof(BitmapHeader4) || bmpHeader.GetType() == typeof(BitmapHeader5))
				{
					BitmapHeader4 bmpHeader4 = (BitmapHeader4)bmpHeader;
					td.AddRow("RedMask", NTS.UInt32(bmpHeader4.RedMask));
					td.AddRow("GreenMask", NTS.UInt32(bmpHeader4.GreenMask));
					td.AddRow("BlueMask", NTS.UInt32(bmpHeader4.BlueMask));
					td.AddRow("CSType", NTS.UInt32(bmpHeader4.CSType));
					td.AddRow("EndPoints->Red", NTS.Int32Array(new int[] { bmpHeader4.Endpoints.CieXyzRed.CieXyzX, bmpHeader4.Endpoints.CieXyzRed.CieXyzY, bmpHeader4.Endpoints.CieXyzRed.CieXyzZ }));
					td.AddRow("EndPoints->Green", NTS.Int32Array(new int[] { bmpHeader4.Endpoints.CieXyzGreen.CieXyzX, bmpHeader4.Endpoints.CieXyzGreen.CieXyzY, bmpHeader4.Endpoints.CieXyzGreen.CieXyzZ }));
					td.AddRow("EndPoints->Blue", NTS.Int32Array(new int[] { bmpHeader4.Endpoints.CieXyzBlue.CieXyzX, bmpHeader4.Endpoints.CieXyzBlue.CieXyzY, bmpHeader4.Endpoints.CieXyzBlue.CieXyzZ }));
					td.AddRow("GammaRed", NTS.UInt32(bmpHeader4.GammaRed));
					td.AddRow("GammaGreen", NTS.UInt32(bmpHeader4.GammaGreen));
					td.AddRow("GammaBlue", NTS.UInt32(bmpHeader4.GammaBlue));
					if(bmpHeader.GetType() == typeof(BitmapHeader5))
					{
						BitmapHeader5 bmpHeader5 = (BitmapHeader5)bmpHeader;
						td.AddRow("Intent", NTS.UInt32(bmpHeader5.Intent));
						td.AddRow("ProfileData", NTS.UInt32(bmpHeader5.ProfileData));
						td.AddRow("ProfileSize", NTS.UInt32(bmpHeader5.ProfileSize));
						td.AddRow("Reserved", NTS.UInt32(bmpHeader5.Reserved));
					}
				}
				return td;
			}
			else if(tnFullPath.Equals("Colors"))
			{
				TextTableData td = new TextTableData(5);
				td.SetColumn(0, "i", 50);
				td.SetColumn(1, "rgbRed", 75, ColumnAlignment.Center);
				td.SetColumn(2, "rgbGreen", 75, ColumnAlignment.Center);
				td.SetColumn(3, "rgbBlue", 75, ColumnAlignment.Center);
				td.SetColumn(4, "rgbReserved", 75, ColumnAlignment.Center);
				
				td.SetCapacity(colors.Length);
				for(UInt16 i = 0; i < colors.Length; i++)
				{
					td.AddRow(NTS.UInt16(i), 
						NTS.UInt8(colors[i].Red),
						NTS.UInt8(colors[i].Green), 
						NTS.UInt8(colors[i].Blue), 
						NTS.UInt8(colors[i].Reserved));
				}

				return td;
			}
			else
			{
				return null;
			}
		}

		public override QuickInfoTableData GetQuickInfo()
		{
			QuickInfoTableData td = new QuickInfoTableData();
			if (bmpHeader != null)
			{
				td.AddRow("Width", bmpHeader.Width.ToString());
				td.AddRow("Height", bmpHeader.Height.ToString());
				if (numColors > 0)
				{
					td.AddRow("# of Colors", numColors.ToString());
				}
				td.AddRow("Bit Count", bmpHeader.BitsPerPixel.ToString());
			}
			return td;
		}

		private void CheckFileHeader()
		{
			// File Check Info
			if (bmpFileHeader.Type != 0x4D42)
				FileCheck.Error("Invalid value for bfType: " + bmpFileHeader.Type.ToString());
			if (bmpFileHeader.Size != m_FileStream.Length)
				FileCheck.Warning("bfSize does not match actual file size");
			if (bmpFileHeader.Reserved1 != 0x00)
				FileCheck.Warning("bfReserved1 must be zero");
			if (bmpFileHeader.Reserved2 != 0x00)
				FileCheck.Warning("bfReserved2 must be zero");
		}

		private void CheckBitmapFileHeader()
		{
			// File Check Info
			UInt32 compression = bmpHeader.Compression;
			UInt16 planes = bmpHeader.Planes;
			UInt16 bitCount = bmpHeader.BitsPerPixel;
			Int32 width = bmpHeader.Width;
			Int32 height = bmpHeader.Height;
			if (planes != 1)
				FileCheck.Warning("biPlanes must be 1");
			if (width < 0)
				FileCheck.Warning("biWidth should be a positive number");
			if (height < 0)
				FileCheck.Message("biHeight is negative, bitmap will be displayed as a top-down DIB");
			if (!(compression == BI_RGB || compression == BI_RLE8 || compression == BI_RLE4 || compression == BI_BITFIELDS || compression == BI_JPEG || compression == BI_PNG))
				FileCheck.Warning("Invalid Value for biCompression: " + compression.ToString());
		}

		private uint NumColors()
		{
			UInt32 compression = bmpHeader.Compression;
			UInt16 bitCount = bmpHeader.BitsPerPixel;
			UInt32 colorsIndexUsed = bmpHeader.ColorsUsed;
			uint numColors = 0;

			if (bitCount == 1)
			{
				numColors = 2;
			}
			else if (bitCount > 1 && bitCount < 16)
			{
				if (colorsIndexUsed == 0)
					numColors = (uint)Math.Pow(2, bitCount);
				else
					numColors = colorsIndexUsed;
			}
			else if (bitCount == 16)
			{
				if (compression == BI_RGB)
				{
					numColors = colorsIndexUsed;
				}
				else if (compression == BI_BITFIELDS)
				{
					numColors = 3;
				}
			}
			else if (bitCount == 24 || bitCount == 32)
			{
				numColors = colorsIndexUsed;
			}
			else
			{
				FileCheck.Error("Invalid Bit Count: " + bitCount.ToString());
			}
			return numColors;
		}
	}
}
