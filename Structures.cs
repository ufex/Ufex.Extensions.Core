using System;
using System.IO;

namespace Ufex.FileTypes.Image
{
	struct RGB
	{
		public byte Red;
		public byte Green;
		public byte Blue;
	}

	struct RGBR
	{
		public byte Red;
		public byte Green;
		public byte Blue;
		public byte Reserved;
	}

	struct CIEXYZ
	{
		public Int32 CieXyzX;
		public Int32 CieXyzY;
		public Int32 CieXyzZ;

		public CIEXYZ(BinaryReader br)
		{
			CieXyzX = br.ReadInt32();
			CieXyzY = br.ReadInt32();
			CieXyzZ = br.ReadInt32();
		}
	}

	struct CIEXYZTRIPLE
	{
		public CIEXYZ CieXyzRed;
		public CIEXYZ CieXyzGreen;
		public CIEXYZ CieXyzBlue;

		public CIEXYZTRIPLE(BinaryReader br)
		{
			CieXyzRed = new CIEXYZ(br);
			CieXyzGreen = new CIEXYZ(br);
			CieXyzBlue = new CIEXYZ(br);
		}
	}

	struct BitmapFileHeader
	{
		public UInt16 Type;
		public UInt32 Size;
		public UInt16 Reserved1;
		public UInt16 Reserved2;
		public UInt32 OffBits;
	}

	/// <summary>
	/// Bitmap Header v3
	/// 40 bytes
	/// </summary>
	class BitmapHeader3
	{
		public UInt32 Size;             // Size of this header in bytes
		public Int32 Width;             // Image width in pixels
		public Int32 Height;            // Image height in pixels
		public UInt16 Planes;           // Number of color planes
		public UInt16 BitsPerPixel;     // Number of bits per pixel
		public UInt32 Compression;      // compression methods used
		public UInt32 SizeOfBitmap;     // Size of bitmap in bytes
		public Int32 HorzResolution;    // Horizontal resolution in pixels per meter
		public Int32 VertResolution;    // Vertical resolution in pixels per meter
		public UInt32 ColorsUsed;       // Number of colors in the image
		public UInt32 ColorsImportant;  // Minimum number of important colors

		public BitmapHeader3()
		{
		}

		public BitmapHeader3(BinaryReader br)
		{
			Read(br);
		}

		public virtual void Read(BinaryReader br)
		{
			Size = br.ReadUInt32();
			Width = br.ReadInt32();
			Height = br.ReadInt32();
			Planes = br.ReadUInt16();
			BitsPerPixel = br.ReadUInt16();
			Compression = br.ReadUInt32();
			SizeOfBitmap = br.ReadUInt32();
			HorzResolution = br.ReadInt32();
			VertResolution = br.ReadInt32();
			ColorsUsed = br.ReadUInt32();
			ColorsImportant = br.ReadUInt32();
		}
	}

	/// <summary>
	/// Bitmap Header v4
	/// 108 bytes
	/// </summary>
	class BitmapHeader4 : BitmapHeader3
	{
		public UInt32 RedMask;
		public UInt32 GreenMask;
		public UInt32 BlueMask;
		public UInt32 AlphaMask;
		public UInt32 CSType;
		public CIEXYZTRIPLE Endpoints;
		public UInt32 GammaRed;
		public UInt32 GammaGreen;
		public UInt32 GammaBlue;

		public BitmapHeader4(BinaryReader br) : base(br)
		{
		}

		public override void Read(BinaryReader br)
		{
			base.Read(br);
			RedMask = br.ReadUInt32();
			GreenMask = br.ReadUInt32();
			BlueMask = br.ReadUInt32();
			AlphaMask = br.ReadUInt16();
			CSType = br.ReadUInt16();
			Endpoints = new CIEXYZTRIPLE(br);
			GammaRed = br.ReadUInt32();
			GammaGreen = br.ReadUInt32();
			GammaBlue = br.ReadUInt32();
		}
	}

	/// <summary>
	/// Bitmap Header v5
	/// 124 bytes
	/// </summary>
	class BitmapHeader5 : BitmapHeader4
	{
		public UInt32 Intent;
		public UInt32 ProfileData;
		public UInt32 ProfileSize;
		public UInt32 Reserved;

		public BitmapHeader5(BinaryReader br) : base(br)
		{
		}

		public override void Read(BinaryReader br)
		{
			Intent = br.ReadUInt32();
			ProfileData = br.ReadUInt32();
			ProfileSize = br.ReadUInt32();
			Reserved = br.ReadUInt32();
		}
	}
}
