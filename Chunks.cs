using System;
using System.Collections.Generic;
using System.Linq;
using Ufex.API;
using Ufex.API.Tables;

namespace Ufex.FileTypes.Image
{
	internal struct RGB
	{
		public byte Red;
		public byte Green;
		public byte Blue;
	}

	struct CRC
	{
		public Byte[] crcValue;
	}

	struct SugPalEntry8
	{
		public Byte red;
		public Byte green;
		public Byte blue;
		public Byte alpha;
		public UInt16 frequency;
	}

	struct SugPalEntry16
	{
		public UInt16 red;
		public UInt16 green;
		public UInt16 blue;
		public UInt16 alpha;
		public UInt16 frequency;
	}

	class Chunk
	{
		public Chunk() { }
		public Chunk(UInt32 Length, char[] ChunkType)
		{
			length = Length;
			chunkType = ChunkType;
		}

		public virtual void ReadChunk(FileReader fr)
		{
		}

		public virtual TextTableData GetTableData(DataFormatter f)
		{
			TextTableData td = new TextTableData(2);
			td.SetColumn(0, "Property", 150);
			td.SetColumn(1, "Value", 200);

			td.AddRow("Length", f.UInt32(length));
			td.AddRow("ChunkType", new string(chunkType));

			Add2TableData(f, td);

			td.AddRow("CRC", f.ByteArray(crc.crcValue));
			return td;
		}

		public virtual void Add2TableData(DataFormatter nts, TextTableData td)
		{
		}

		public UInt32 length;
		public char[] chunkType;   // 4 bytes
		public CRC crc = new CRC();
	}

	// Critical Chunks

	/// <summary>
	/// IHDR - Image header
	/// </summary>
	class Chnk_IHDR : Chunk
	{
		public UInt32 width;
		public UInt32 height;
		public Byte bitDepth;
		public Byte colorType;
		public Byte compressionMethod;
		public Byte filterMethod;
		public Byte interlaceMethod;

		public override void ReadChunk(FileReader fr)
		{
			width = fr.ReadUInt32();
			height = fr.ReadUInt32();
			bitDepth = fr.ReadByte();
			colorType = fr.ReadByte();
			compressionMethod = fr.ReadByte();
			filterMethod = fr.ReadByte();
			interlaceMethod = fr.ReadByte();
		}

		public override void Add2TableData(DataFormatter f, TextTableData td)
		{
			td.AddRow("Width", f.UInt32(width));
			td.AddRow("Height", f.UInt32(height));
			td.AddRow("Bit Depth", f.Byte(bitDepth));
			td.AddRow("Color Type", f.Byte(colorType));
			td.AddRow("Compression Method", f.Byte(compressionMethod));
			td.AddRow("Filter Method", f.Byte(filterMethod));
			td.AddRow("Interlace Method", f.Byte(interlaceMethod));
		}
	}

	/// <summary>
	/// PLTE - Palette
	/// </summary>
	class Chnk_PLTE : Chunk
	{
		public RGB[] palette;

		public override void ReadChunk(FileReader fr)
		{
			uint numColors = length / 3;
			palette = new RGB[numColors];
			for (int i = 0; i < numColors; i++)
			{
				palette[i] = new RGB();
				palette[i].Red = fr.ReadByte();
				palette[i].Green = fr.ReadByte();
				palette[i].Blue = fr.ReadByte();
			}
		}

		public override TextTableData GetTableData(DataFormatter f)
		{
			TextTableData td = new TextTableData(3);
			td.SetColumn(0, "Red");
			td.SetColumn(1, "Green");
			td.SetColumn(2, "Blue");

			for (int i = 0; i < palette.Length; i++)
			{
				td.AddRow(f.Byte(palette[i].Red),
					f.Byte(palette[i].Green),
					f.Byte(palette[i].Blue));
			}
			return td;
		}
	}

	/// <summary>
	/// IDAT - Image data
	/// </summary>
	class Chnk_IDAT : Chunk
	{

	}

	/// <summary>
	/// IEND - Image trailer
	/// </summary>
	class Chnk_IEND : Chunk
	{

	}


	/****************** Ancillary Chunks **************************/


	/// <summary>
	/// tRNS - Transparency
	/// </summary>
	class Chnk_tRNS : Chunk
	{
		public int colorType;

		// For color type 0
		UInt16 greySampleValue;

		// For color type 2
		UInt16 redSampleValue;
		UInt16 greenSampleValue;
		UInt16 blueSampleValue;

		// For color type 3
		Byte[] paletteAlpha;

		public Chnk_tRNS() : this(-1) { }
		public Chnk_tRNS(int colorType)
		{
			this.colorType = colorType;
		}

		public override void ReadChunk(FileReader fr)
		{
			if (colorType == 0)
			{
				greySampleValue = fr.ReadUInt16();
			}
			else if (colorType == 2)
			{
				redSampleValue = fr.ReadUInt16();
				greenSampleValue = fr.ReadUInt16();
				blueSampleValue = fr.ReadUInt16();
			}
			else if (colorType == 3)
			{
				paletteAlpha = new Byte[this.length];
				for (int i = 0; i < paletteAlpha.Length; i++)
					paletteAlpha[i] = fr.ReadByte();
			}
			else if (colorType == -1)
			{
				if (this.length == 2)
				{
					colorType = 0;
					ReadChunk(fr);
				}
				else if (this.length == 6)
				{
					colorType = 2;
					ReadChunk(fr);
				}
			}
		}

		public override void Add2TableData(DataFormatter f, TextTableData td)
		{
			if (colorType == 0)
			{
				td.AddRow("Grey sample value", f.UInt16(greySampleValue));
			}
			else if (colorType == 2)
			{
				td.AddRow("Red sample value", f.UInt16(redSampleValue));
				td.AddRow("Green sample value", f.UInt16(greenSampleValue));
				td.AddRow("Blue sample value", f.UInt16(blueSampleValue));
			}
			else if (colorType == 3)
			{
				for (int i = 0; i < paletteAlpha.Length; i++)
					td.AddRow("Alpha for palette index " + i.ToString(), f.Byte(paletteAlpha[i]));
			}
		}
	}

	class Chnk_cHRM : Chunk
	{
		UInt32 whitePointX;
		UInt32 whitePointY;
		UInt32 redX;
		UInt32 redY;
		UInt32 greenX;
		UInt32 greenY;
		UInt32 blueX;
		UInt32 blueY;

		public override void ReadChunk(FileReader fr)
		{
			whitePointX = fr.ReadUInt32();
			whitePointY = fr.ReadUInt32();
			redX = fr.ReadUInt32();
			redY = fr.ReadUInt32();
			greenX = fr.ReadUInt32();
			greenY = fr.ReadUInt32();
			blueX = fr.ReadUInt32();
			blueY = fr.ReadUInt32();
		}

		public override void Add2TableData(DataFormatter nts, TextTableData td)
		{
			td.AddRow("White Point X", nts.UInt32(whitePointX));
			td.AddRow("White Point Y", nts.UInt32(whitePointY));
			td.AddRow("Red X", nts.UInt32(redX));
			td.AddRow("Red Y", nts.UInt32(redY));
			td.AddRow("Green X", nts.UInt32(greenX));
			td.AddRow("Green Y", nts.UInt32(greenY));
			td.AddRow("Blue X", nts.UInt32(blueX));
			td.AddRow("Blue Y", nts.UInt32(blueY));
		}
	}

	class Chnk_gAMA : Chunk
	{
		UInt32 gamma;

		public override void ReadChunk(FileReader fr)
		{
			gamma = fr.ReadUInt32();
		}

		public override void Add2TableData(DataFormatter nts, TextTableData td)
		{
			td.AddRow("Gamma", nts.UInt32(gamma));
		}
	}

	class Chnk_iCCP : Chunk
	{
		// TODO
	}

	class Chnk_sBIT : Chunk
	{
		// TODO
	}

	class Chnk_sRGB : Chunk
	{
		Byte renderingIntent;

		public override void ReadChunk(FileReader fr)
		{
			renderingIntent = fr.ReadByte();
		}

		public override void Add2TableData(DataFormatter nts, TextTableData td)
		{
			td.AddRow("Rendering Intent", nts.Byte(renderingIntent));
		}
	}

	/// <summary>
	/// iTXt - International textual data
	/// </summary>
	class Chnk_iTXt : Chunk
	{
		// TODO
	}

	/// <summary>
	/// tEXt - Textual data
	/// </summary>
	class Chnk_tEXt : Chunk
	{
		char[] keyword;
		char[] textString;

		public override void ReadChunk(FileReader fr)
		{
			byte[] chnkData = fr.ReadBytes((int)length);

			byte tmpByte = 0xFF;
			int nullPos = 0;
			int i;
			for (i = 0; i < chnkData.Length && tmpByte != '\0'; ++i)
			{
				tmpByte = chnkData[i];
			}
			nullPos = i - 1;

			keyword = new char[nullPos];
			textString = new char[chnkData.Length - nullPos - 1];

			for (i = 0; i < nullPos; i++)
			{
				keyword[i] = (char)chnkData[i];
			}

			for (i = nullPos + 1; i < chnkData.Length; i++)
			{
				textString[i - (nullPos + 1)] = (char)chnkData[i];
			}
		}

		public override void Add2TableData(DataFormatter f, TextTableData td)
		{
			td.AddRow("Keyword", new string(keyword));
			td.AddRow("Text String", new string(textString));
		}
	}

	/// <summary>
	/// zTXt - Compressed textual data
	/// </summary>
	class Chnk_zTXt : Chunk
	{
		// TODO
	}

	/// <summary>
	/// bKGD - Background colour
	/// </summary>
	class Chnk_bKGD : Chunk
	{
		int colorType;

		// For color type 0 & 4
		UInt16 greyscale;

		// For color types 2 & 6
		UInt16 red;
		UInt16 green;
		UInt16 blue;

		// For color type 3
		Byte paletteIndex;

		public Chnk_bKGD() : this(-1) { }
		public Chnk_bKGD(int colorType)
		{
			this.colorType = colorType;
		}

		public override void ReadChunk(FileReader fr)
		{
			if (colorType == 0 || colorType == 4)
			{
				greyscale = fr.ReadUInt16();
			}
			else if (colorType == 2 || colorType == 6)
			{
				red = fr.ReadUInt16();
				green = fr.ReadUInt16();
				blue = fr.ReadUInt16();
			}
			else if (colorType == 3)
			{
				paletteIndex = fr.ReadByte();
			}
			else if (colorType == -1)
			{
				if (this.length == 2)
				{
					colorType = 0;
					ReadChunk(fr);
				}
				else if (this.length == 6)
				{
					colorType = 2;
					ReadChunk(fr);
				}
				else if (this.length == 1)
				{
					colorType = 3;
					ReadChunk(fr);
				}
			}
		}

		public override void Add2TableData(DataFormatter f, TextTableData td)
		{
			if (colorType == 0 || colorType == 4)
			{
				td.AddRow("Greyscale", f.UInt16(greyscale));
			}
			else if (colorType == 2 || colorType == 6)
			{
				td.AddRow("Red", f.UInt16(red));
				td.AddRow("Green", f.UInt16(green));
				td.AddRow("Blue", f.UInt16(blue));
			}
			else if (colorType == 3)
			{
				td.AddRow("Palette index", f.Byte(paletteIndex));
			}
		}
	}

	/// <summary>
	/// hIST - Image histogram
	/// </summary>
	class Chnk_hIST : Chunk
	{
		// TODO
	}

	/// <summary>
	/// pHYs - Physical pixel dimensions
	/// </summary>
	class Chnk_pHYs : Chunk
	{
		readonly static Dictionary<byte, string> UNITS = new Dictionary<byte, string>()
		{
			{ 0x00, "Unknown" },
			{ 0x01, "Metre" }
		};

		UInt32 ppuX;
		UInt32 ppuY;
		Byte unit;

		public override void ReadChunk(FileReader fr)
		{
			ppuX = fr.ReadUInt32();
			ppuY = fr.ReadUInt32();
			unit = fr.ReadByte();
		}

		public override void Add2TableData(DataFormatter f, TextTableData td)
		{
			td.AddRow("Pixels per unit X", f.UInt32(ppuX));
			td.AddRow("Pixels per unit Y", f.UInt32(ppuY));
			td.AddRow("Unit", f.Byte(unit), UNITS.ContainsKey(unit) ? UNITS[unit] : "");
		}
	}

	/// <summary>
	/// sPLT - Suggested palette
	/// </summary>
	class Chnk_sPLT : Chunk
	{
		string paletteName;
		Byte sampleDepth;

		// If sampleDepth == 8
		SugPalEntry8[] palette8;

		// If sampleDepth == 16
		SugPalEntry16[] palette16;

		public override void ReadChunk(FileReader fr)
		{
			paletteName = fr.ReadNullTermString();
			sampleDepth = fr.ReadByte();
			int numEntries = 0;
			if (sampleDepth == 8)
			{
				numEntries = (int)(this.length - paletteName.Length - 1 - 1) / 6;
				palette8 = new SugPalEntry8[numEntries];
				for (int i = 0; i < numEntries; i++)
				{
					palette8[i].red = fr.ReadByte();
					palette8[i].green = fr.ReadByte();
					palette8[i].blue = fr.ReadByte();
					palette8[i].alpha = fr.ReadByte();
					palette8[i].frequency = fr.ReadUInt16();
				}
			}
			else if (sampleDepth == 16)
			{
				numEntries = (int)(this.length - paletteName.Length - 1 - 1) / 10;
				palette16 = new SugPalEntry16[numEntries];
				for (int i = 0; i < numEntries; i++)
				{
					palette16[i].red = fr.ReadUInt16();
					palette16[i].green = fr.ReadUInt16();
					palette16[i].blue = fr.ReadByte();
					palette16[i].alpha = fr.ReadUInt16();
					palette16[i].frequency = fr.ReadUInt16();
				}
			}
			else
			{

			}
		}

		public override void Add2TableData(DataFormatter nts, TextTableData td)
		{
			td.AddRow("Palette name", paletteName);
			td.AddRow("Sample depth", nts.Byte(sampleDepth));
		}
	}

	/// <summary>
	/// tIME - Image last-modification time
	/// </summary>
	class Chnk_tIME : Chunk
	{
		UInt16 year;
		Byte month;
		Byte day;
		Byte hour;
		Byte minute;
		Byte second;

		public override void ReadChunk(FileReader fr)
		{
			year = fr.ReadUInt16();
			month = fr.ReadByte();
			day = fr.ReadByte();
			hour = fr.ReadByte();
			minute = fr.ReadByte();
			second = fr.ReadByte();
		}

		public override void Add2TableData(DataFormatter f, TextTableData td)
		{
			td.AddRow("Year", f.UInt16(year));
			td.AddRow("Month", f.Byte(month));
			td.AddRow("Day", f.Byte(day));
			td.AddRow("Hour", f.Byte(hour));
			td.AddRow("Minute", f.Byte(minute));
			td.AddRow("Second", f.Byte(second));
		}
	}

}
