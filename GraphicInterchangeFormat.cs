using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Ufex.API;
using Ufex.API.Tables;

namespace Ufex.FileTypes.Images
{
	internal struct RGB
	{
		public byte Red;
		public byte Green;
		public byte Blue;
	}

	public class GraphicInterchangeFormat : FileType
	{
		struct AppExt
		{
			public byte extIntro;
			public byte extLabel;
			public byte blockSize;
			public byte[] appId;            // 8 Bytes
			public byte[] appAuthCode;      // 3 Bytes
			public byte[] appData;
		}
		/*
		struct ComExt
		{
			public byte extIntro;
			public byte comLabel;
			public byte[] comData;
		}
		*/
		struct GraphCtrlExt
		{
			public byte extIntro;           // 0x21
			public byte graphCtrlLabel;     // 0xF9
			public byte blockSize;          // 0x04
			public byte packedFields;       // 1 Byte - 4 Vars
			public byte reserved;           // 3 Bits
			public byte disposalMethod;     // 3 Bits
			public bool userInputFlag;      // 1 Bit
			public bool transColorFlag;     // 1 Bit
			public ushort delayTime;
			public byte transColorIndex;
			public byte blockTerminator;
		}

		struct LogicalScreenDescriptor      // Total Size = 9 Bytes and 2 Bits
		{
			public ushort LogScreenWidth;
			public ushort LogScreenHeight;
			public byte PackedFields;
			public bool GlobalClrTab;
			public byte ColorResolution;
			public bool SortFlag;
			public byte SizeOfGlobalClrTable;
			public byte BackColorIndex;
			public byte PixelAspectRatio;
		}

		struct ColorTable
		{
			public int Size;
			public int numColors;
			public RGB[] Colors;
		}

		struct ImageDescriptor   // Total Size = 12 Bytes and 3 Bits
		{
			public byte imageSeperator;         // Image Seperator
			public ushort imageLeftPos;         // Image Left Position
			public ushort imageTopPos;          // Image Top Position
			public ushort imageWidth;           // Image width
			public ushort imageHeight;          // Image height
			public byte packedFields;           // Packed Fields
			public bool locColorTabFlag;        // Local Color Table Flag
			public bool interlaceFlag;          // Interlace Flag
			public bool sortFlag;               // Sort Flag
			public byte reserved;               // 2 Bits
			public byte sizeOfLocClrTable;      // 3 Bits
		}

		struct TableBasedImageData
		{
			public byte lzwMinCodeSize;
			public ushort clearCode;
			public int numBlocks;
		}

		struct ControlExt
		{
			public byte label;
			public byte size;
		}

		struct GifImage
		{
			public GraphCtrlExt grphCtrlExt;          // Graphic Control Extension
			public ControlExt conExt;
			public ImageDescriptor imgDesc;           // Image Descriptor
			public ColorTable locClrTab;              // Local Color Table
			public TableBasedImageData tabBasImgDat;
			public ArrayList extensions;
		}

		struct DataStream
		{
			public LogicalScreenDescriptor logScreenDesc;
			public ColorTable gct;
			public ArrayList images;
		}

		const byte EXT_PLAINTEXT =      0x01;
		const byte EXT_GRAPHCTRL =      0xF9;
		const byte EXT_APPLICATION =    0xFF;
		const byte EXT_COMMENT =        0xFE;

		const byte BLOCK_PLAINTEXT =	0x01;
		const byte BLOCK_EXTINTRO =     0x21;
		const byte BLOCK_IMAGEDESC =	0x2C;
		const byte BLOCK_TRAILER =		0x3B;
		const byte BLOCK_GRPHCTRLEXT =  0xF9;
		const byte BLOCK_COMEXT	=       0xFE;
		const byte BLOCK_APPEXT	=       0xFF;
				
		bool GetBit(Byte aByte, Byte position) { return (bool)(((aByte >> position) & 1) == 1); }

		DataStream m_DataStream;

		TreeNode tnScreenDescriptor;
		TreeNode tnImage;
	
		string signature;
		string version;
		
		public GraphicInterchangeFormat()
		{
			Description = "Graphic Interchange Format (GIF)";
			Log.SetLogName("GIF.log");
		
			// Set which tabs to display
			ShowTechnical = true;
			ShowGraphic = true;
			ShowFileCheck = true;
			UseTreeViewIcons = true;
		}

		public override bool ProcessFile()
		{	
			m_FileStream.Position = 0;
			BinaryReader r = new BinaryReader(m_FileStream);

			int ti = 0;

			char[] tempSign;
			tempSign = r.ReadChars(3);

			// Check the signature
			if(tempSign[0] != 'G' || tempSign[1] != 'I' || tempSign[2] != 'F')
			{
				FileCheck.Error("Invalid signature");		
				return false;
			}

			signature = new String(tempSign);
		
			char[] tempVers;
			tempVers = r.ReadChars(3);
			version = new String(tempVers);

			ti++;

			TreeNodes.Add(new TreeNode("Header", (int)TreeViewIcon.Header, (int)TreeViewIcon.Header));
			
			// Read Data Stream
			m_DataStream = new DataStream();
			m_DataStream.images = new ArrayList();
			
			// Read Logical Screen Descriptor
			m_DataStream.logScreenDesc = new LogicalScreenDescriptor();
			m_DataStream.logScreenDesc.LogScreenWidth = r.ReadUInt16();
			m_DataStream.logScreenDesc.LogScreenHeight = r.ReadUInt16();
			
			Byte PackedFields = r.ReadByte();
			m_DataStream.logScreenDesc.GlobalClrTab = GetBit(PackedFields, 7);
			m_DataStream.logScreenDesc.ColorResolution = 0;
			
			if(GetBit(PackedFields, 6))
				m_DataStream.logScreenDesc.ColorResolution = 4;
			if(GetBit(PackedFields, 5))
				m_DataStream.logScreenDesc.ColorResolution += 2;
			if(GetBit(PackedFields, 4))
				m_DataStream.logScreenDesc.ColorResolution += 1;
			m_DataStream.logScreenDesc.SortFlag = GetBit( PackedFields, 3 );
			
			if(GetBit(PackedFields, 2))
				m_DataStream.logScreenDesc.SizeOfGlobalClrTable = 4;
			if(GetBit(PackedFields, 1))
				m_DataStream.logScreenDesc.SizeOfGlobalClrTable += 2;
			if(GetBit(PackedFields, 0))
				m_DataStream.logScreenDesc.SizeOfGlobalClrTable += 1;

			m_DataStream.logScreenDesc.PackedFields = PackedFields;
			m_DataStream.logScreenDesc.BackColorIndex = r.ReadByte();
			m_DataStream.logScreenDesc.PixelAspectRatio = r.ReadByte();
		
			tnScreenDescriptor = new TreeNode("Logical Screen Descriptor", (int)TreeViewIcon.Properties, (int)TreeViewIcon.Properties);
			TreeNodes.Add(tnScreenDescriptor);
		
			// Read in Global Color Table
			if(m_DataStream.logScreenDesc.GlobalClrTab)
			{
				m_DataStream.gct = new ColorTable();
				m_DataStream.gct.Size = 3 * (int)(Math.Pow(2, m_DataStream.logScreenDesc.SizeOfGlobalClrTable + 1)) ; 
				int numColors = (int)(Math.Pow(2, m_DataStream.logScreenDesc.SizeOfGlobalClrTable + 1));
				m_DataStream.gct.Colors = new RGB[numColors];
				for(int i = 0; i < numColors; i++)
				{
					RGB tmpColor = new RGB();
					tmpColor.Red = r.ReadByte();
					tmpColor.Green = r.ReadByte();
					tmpColor.Blue = r.ReadByte();
					m_DataStream.gct.Colors[i] = tmpColor; 
				}
				TreeNodes.Add(new TreeNode("Global Color Table", (int)TreeViewIcon.Palette, (int)TreeViewIcon.Palette));
			}
		
			bool moreImages = true;
		
			GifImage tmpImage = new GifImage();
			ImageDescriptor tmpImageDesc = new ImageDescriptor();
			ColorTable tmpLocClrTab = new ColorTable();
			int imgNum = 0;
			while(moreImages == true)
			{
				// Create TreeNode for image
				tnImage = new TreeNode("Image", (int)TreeViewIcon.Image, (int)TreeViewIcon.Image);
				tnImage.Tag = imgNum;

				tmpImage = new GifImage();
				tmpImage.extensions = new ArrayList();
				tmpImageDesc = new ImageDescriptor();
			
				tmpImage.conExt = new ControlExt();
		
				byte nextByte = r.ReadByte();

				while(nextByte == BLOCK_EXTINTRO)
				{
					// Read Label and Block Size
					Byte tmpLabel = r.ReadByte();
					Byte tmpSize = r.ReadByte();

					if(tmpLabel == EXT_GRAPHCTRL)
					{
						//DebugOut("Graphic Control Extension");
						tmpImage.grphCtrlExt = new GraphCtrlExt();
						tmpImage.grphCtrlExt.extIntro = nextByte;
						tmpImage.grphCtrlExt.graphCtrlLabel = tmpLabel;
						tmpImage.grphCtrlExt.blockSize = tmpSize;
						tmpImage.grphCtrlExt.packedFields = r.ReadByte();
						
						tmpImage.grphCtrlExt.reserved = 0;
						if(GetBit(tmpImage.grphCtrlExt.packedFields, 7))
							tmpImage.grphCtrlExt.reserved  = 4;
						if(GetBit(tmpImage.grphCtrlExt.packedFields, 6))
							tmpImage.grphCtrlExt.reserved  += 2;
						if(GetBit(tmpImage.grphCtrlExt.packedFields, 5))
							tmpImage.grphCtrlExt.reserved  += 1;
						
						tmpImage.grphCtrlExt.disposalMethod = 0;
						if(GetBit(tmpImage.grphCtrlExt.packedFields, 4))
							tmpImage.grphCtrlExt.disposalMethod  = 4;
						if(GetBit(tmpImage.grphCtrlExt.packedFields, 3))
							tmpImage.grphCtrlExt.disposalMethod  += 2;
						if(GetBit(tmpImage.grphCtrlExt.packedFields, 2))
							tmpImage.grphCtrlExt.disposalMethod  += 1;
		
						tmpImage.grphCtrlExt.userInputFlag = GetBit(tmpImage.grphCtrlExt.packedFields, 1);
						tmpImage.grphCtrlExt.transColorFlag = GetBit(tmpImage.grphCtrlExt.packedFields, 0);
						tmpImage.grphCtrlExt.delayTime = r.ReadUInt16();
						tmpImage.grphCtrlExt.transColorIndex = r.ReadByte();
						tmpImage.grphCtrlExt.blockTerminator = r.ReadByte();
		
						tnImage.Nodes.Add(new TreeNode("Graphic Control Extension", (int)TreeViewIcon.Properties, (int)TreeViewIcon.Properties));
						tmpImage.extensions.Add(null);
					}
					else if(tmpLabel == BLOCK_APPEXT)
					{
						//DebugOut("Application Extension");
						AppExt tmpAppExt = new AppExt();
						tmpAppExt.extIntro = nextByte;
						tmpAppExt.extLabel = tmpLabel;
						tmpAppExt.blockSize = tmpSize;
						tmpAppExt.appId  = r.ReadBytes(8);	// 8 Bytes
						tmpAppExt.appAuthCode = r.ReadBytes(3);	// 3 Bytes

						//Byte appData __gc[];
						bool moreAppExtBlocks = true;
						while(moreAppExtBlocks)
						{
							Byte blockSize = r.ReadByte();
							if(blockSize != 0x00)
								tmpAppExt.appData = r.ReadBytes(blockSize);
							else
								moreAppExtBlocks = false;
						}
						tmpImage.extensions.Add(tmpAppExt);
						tnImage.Nodes.Add(new TreeNode("Application Extension", (int)TreeViewIcon.Properties, (int)TreeViewIcon.Properties));
					}
					else 
					{
						tnImage.Nodes.Add(new TreeNode("Unknown Extension", (int)TreeViewIcon.Properties, (int)TreeViewIcon.Properties));
						r.ReadBytes(tmpSize + 1);
						tmpImage.extensions.Add(null);
						Log.Info("Unknown Extension: " + tmpLabel.ToString(), "GIF", "ProcessFile()");
					}
		
					nextByte = r.ReadByte();
				}
				
		
				// Create TreeNode for Image Descriptor
				tnImage.Nodes.Add(new TreeNode("Image Descriptor", (int)TreeViewIcon.Properties, (int)TreeViewIcon.Properties));
				tmpImage.extensions.Add(null);

				// Read Image Descriptor
				tmpImageDesc.imageSeperator = nextByte;
				tmpImageDesc.imageLeftPos = r.ReadUInt16();
				tmpImageDesc.imageTopPos = r.ReadUInt16();
				tmpImageDesc.imageWidth = r.ReadUInt16();
				tmpImageDesc.imageHeight = r.ReadUInt16();
				tmpImageDesc.packedFields = r.ReadByte();
				tmpImageDesc.locColorTabFlag = GetBit(tmpImageDesc.packedFields, 7);
				tmpImageDesc.interlaceFlag = GetBit(tmpImageDesc.packedFields, 6);
				tmpImageDesc.sortFlag = GetBit(tmpImageDesc.packedFields, 5);
				
				if(GetBit(tmpImageDesc.packedFields, 4))
					tmpImageDesc.reserved = 2;
				if(GetBit(tmpImageDesc.packedFields, 3))
					tmpImageDesc.reserved += 1;
			
				if(GetBit(tmpImageDesc.packedFields, 2))
					tmpImageDesc.sizeOfLocClrTable = 4;
				if(GetBit(tmpImageDesc.packedFields, 1))
					tmpImageDesc.sizeOfLocClrTable += 2;
				if(GetBit(tmpImageDesc.packedFields, 0))
					tmpImageDesc.sizeOfLocClrTable += 1;


				// Read Local Image Table
				if(tmpImageDesc.locColorTabFlag)
				{
					tmpLocClrTab = new ColorTable();
					tmpLocClrTab.Size = 3 * (int)(Math.Pow(2, tmpImageDesc.sizeOfLocClrTable + 1)); 
					tmpLocClrTab.numColors = (int)(Math.Pow(2, tmpImageDesc.sizeOfLocClrTable + 1));
					tmpLocClrTab.Colors = new RGB[tmpLocClrTab.numColors];
					RGB tmpColor = new RGB();
					for(int i = 0; i < tmpLocClrTab.numColors; i++)
					{
						tmpColor.Red = r.ReadByte();
						tmpColor.Green = r.ReadByte();
						tmpColor.Blue = r.ReadByte();
						tmpLocClrTab.Colors[i] = tmpColor;
					}
					tnImage.Nodes.Add(new TreeNode("Local Color Table", (int)TreeViewIcon.Palette, (int)TreeViewIcon.Palette));
					tmpImage.extensions.Add(null);
					tmpImage.locClrTab = tmpLocClrTab;
				}
				tmpImage.imgDesc = tmpImageDesc;
				
				tmpImage.tabBasImgDat = new TableBasedImageData();
		
		
				bool moreBlocks = true;
				Byte tmpBlockSize = 0;
				nextByte = 0;
				tmpImage.tabBasImgDat.lzwMinCodeSize = r.ReadByte();
				//DebugOut(String.Concat("Min code size = ", tmpImage.tabBasImgDat.lzwMinCodeSize.ToString()));
				while(moreBlocks)
				{
					try
					{
						tmpBlockSize = r.ReadByte();
					}
					catch(Exception e)
					{
						Log.NewException(e, "GIF", "ProcessFile", "Exception while reading block size: ");
						return false;
					}

					if(tmpBlockSize == 0x00)
					{
						tmpBlockSize = r.ReadByte();
						if(tmpBlockSize == 0x3B)
						{
							//DebugOut(String.Concat("Finished Reading Image Stream @ ", m_FileStream.get_Position().ToString()));
							moreBlocks = false;
						}
						else if(tmpBlockSize == BLOCK_EXTINTRO)
						{
							
							byte label = r.ReadByte();
							//DebugOut(String.Concat("label = ", label.ToString()));
							if(label == BLOCK_APPEXT)
							{
								//DebugOut("Application Extension");
								AppExt tmpAppExt = new AppExt();
								tmpAppExt.extIntro = tmpBlockSize;
								tmpAppExt.extLabel = label;
								tmpAppExt.blockSize = r.ReadByte();
								tmpAppExt.appId  = r.ReadBytes(8);	// 8 Bytes
								tmpAppExt.appAuthCode = r.ReadBytes(3);	// 3 Bytes

								bool moreAppExtBlocks = true;
								while(moreAppExtBlocks)
								{
									byte blockSize = r.ReadByte();
									if(blockSize != 0x00)
									{
										tmpAppExt.appData = r.ReadBytes(blockSize);
									}
									else
									{
										m_FileStream.Position--;
										moreAppExtBlocks = false;
									}
								}
								
								tmpImage.extensions.Add(tmpAppExt);
								tnImage.Nodes.Add(new TreeNode("Application Extension", (int)TreeViewIcon.Properties, (int)TreeViewIcon.Properties));
							}
							else if (label == EXT_COMMENT)
							{

							}
							else if(label == EXT_PLAINTEXT)
							{
								//DebugOut("Plain Text Extension");
							}
						}
					}
					else
					{

						Byte[] tabImgDat;
						tabImgDat = r.ReadBytes(tmpBlockSize);

						// Code to process table based image data
						//UInt16 code_mask __gc[] = {0x0000, 0x0001, 0x0003, 0x0007, 
						//							0x000F,	0x001F, 0x003F, 0x007F, 
						//							0x00FF, 0x01FF, 0x03FF, 0x07FF, 0x0FFF };
						//short curr_size;                     /* The current code size */
						//short clear;                         /* Value for a clear code */
						//short ending;                        /* Value for a ending code */
						//short newcodes;                      /* First available code */
						//short top_slot;                      /* Highest code for current size */
						//short slot;                          /* Last read code */
						//short navail_bytes = 0;              /* # bytes left in block */
						//short nbits_left = 0;                /* # bits left in current byte */
						//Byte b1;                           /* Current byte */
						//Byte byte_buff[257];               /* Current block */
						//Byte *pbytes;                      /* Pointer to next byte in block */
						//curr_size = size + 1;
						//top_slot = 1 << curr_size;
						//clear = 1 << size;
						//ending = clear + 1;
						//slot = newcodes = ending + 1;
						//navail_bytes = nbits_left = 0;
						
						tmpImage.tabBasImgDat.numBlocks++;
					}
				}
		
				TreeNodes.Add(tnImage);
				m_DataStream.images.Add(tmpImage);
				imgNum++;
				moreImages = false;
			}

			return true;
		}

		public override TableData GetData(TreeNode tn)
		{
			int tnIndex = tn.Index;
			String tnFullPath = tn.FullPath;

			if(tnFullPath.Equals("Header"))
			{			
				TextTableData td = new TextTableData(2);
				td.SetColumn(0, "Property", 100);
				td.SetColumn(1, "Value", 100);

				td.AddRow("Signature", signature);
				td.AddRow("Version", version);
				return td;
			}
			else if(tnFullPath.Equals("Logical Screen Descriptor"))
			{
				TextTableData td = new TextTableData(2);
				td.SetColumn(0, "Property", 75);
				td.SetColumn(1, "Value", 25);

				td.AddRow("Logical Screen Width", NTS.UInt16(m_DataStream.logScreenDesc.LogScreenWidth));//, String.Concat("Image width = ", m_DataStream.logScreenDesc.LogScreenWidth.ToString(), " pixels"));		
				td.AddRow("Logical Screen Height", NTS.UInt16(m_DataStream.logScreenDesc.LogScreenHeight));//, String.Concat( "Image height = ", m_DataStream.logScreenDesc.LogScreenHeight.ToString(), " pixels" ));
				td.AddRow("Packed Fields", NTS.UInt8(m_DataStream.logScreenDesc.PackedFields));
				td.AddRow("Global Color Table Flag", NTS.Bool(m_DataStream.logScreenDesc.GlobalClrTab));
				td.AddRow("Color Resolution", NTS.UInt8(m_DataStream.logScreenDesc.ColorResolution));//, String.Concat("Original Image had ", (m_DataStream.logScreenDesc.ColorResolution + 1).ToString(), " bits per primary color"));	
				td.AddRow("Sort Flag", NTS.Bool(m_DataStream.logScreenDesc.SortFlag));
				td.AddRow("Size of Global Color Table", NTS.UInt8( m_DataStream.logScreenDesc.SizeOfGlobalClrTable));//, String.Concat("Global Color Table Size is ", (Math.Pow(2, m_DataStream.logScreenDesc.SizeOfGlobalClrTable + 1)).ToString(), " bytes"));
				td.AddRow("Back Color Index", NTS.UInt8(m_DataStream.logScreenDesc.BackColorIndex)); 
				td.AddRow("Aspect Ratio", NTS.UInt8(m_DataStream.logScreenDesc.PixelAspectRatio));
				
				return td;
			}
			else if(tnFullPath.Equals("Global Color Table"))
			{
				TextTableData td = new TextTableData(3);
				td.SetColumn(0, "Red", 65, ColumnAlignment.Center);
				td.SetColumn(1, "Green", 65, ColumnAlignment.Center);
				td.SetColumn(2, "Blue", 65, ColumnAlignment.Center);

				//int gctnum = TreeIndex[i];
				//DATASTREAM* tmpDS = static_cast<DATASTREAM*>( DataStreams.get_Item( gctnum ) );

				//tmpDS.gct = new GLOBALCOLORTABLE;
				//tmpDS.gct.Size = 3 * static_cast<int>( Math.Pow( 2, tmpDS.logScreenDesc.SizeOfGlobalClrTable + 1 ) ) ; 
				int NumColors = (int)(Math.Pow(2, m_DataStream.logScreenDesc.SizeOfGlobalClrTable + 1));
				//tmpDS.gct.colors = new RGB2[NumColors];
				
				RGB tmpColor = new RGB();
				for(int i = 0; i < NumColors; i++)
				{		
					tmpColor = m_DataStream.gct.Colors[i];
					td.AddRow(NTS.UInt8(tmpColor.Red), NTS.UInt8(tmpColor.Green), NTS.UInt8(tmpColor.Blue));
				}

				return td;
			}
			else if(tnFullPath.StartsWith("Image"))
			{	
				TreeNode tnImageNode;
				
				if(tnFullPath.Equals("Image"))
					tnImageNode = tn;
				else
					tnImageNode = tn.Parent;

				int imageNum = (int)(tnImageNode.Tag);

				GifImage tmpImage = (GifImage)(m_DataStream.images[imageNum]);
				
				if(tnFullPath.EndsWith("Image Descriptor"))
				{		
					ImageDescriptor tmpImgDesc = tmpImage.imgDesc;

					TextTableData td = new TextTableData(2);
					td.SetColumn(0, "Property", 200);
					td.SetColumn(1, "Value", 100);
					
					td.AddRow("Image Seperator", NTS.UInt8(tmpImgDesc.imageSeperator));
					td.AddRow("Image Left Position", NTS.UInt16(tmpImgDesc.imageLeftPos));
					td.AddRow("Image Top Position", NTS.UInt16(tmpImgDesc.imageTopPos));
					td.AddRow("Image Width", NTS.UInt16(tmpImgDesc.imageWidth));
					td.AddRow("Image Height", NTS.UInt16(tmpImgDesc.imageHeight));
					td.AddRow("Local Color Table Flag", NTS.Bool(tmpImgDesc.locColorTabFlag));			
					td.AddRow("Interlace Flag", NTS.Bool(tmpImgDesc.interlaceFlag));			
					td.AddRow("Sort Flag", NTS.Bool(tmpImgDesc.sortFlag));			
					td.AddRow("Reserved", NTS.UInt8(tmpImgDesc.reserved));			
					td.AddRow("Size of Local Color Table", NTS.UInt8(tmpImgDesc.sizeOfLocClrTable));			
					return td;
				}
				else if(tnFullPath.EndsWith("Graphic Control Extension"))
				{
					GraphCtrlExt tmpGraphCtrlExt = tmpImage.grphCtrlExt;

					TextTableData td = new TextTableData(2);
					td.SetColumn(0, "Property", 200);
					td.SetColumn(1, "Value", 100);
					
					td.AddRow("Extension Introducer", NTS.UInt8(tmpGraphCtrlExt.extIntro));
					td.AddRow("Graphic Control Label", NTS.UInt8(tmpGraphCtrlExt.graphCtrlLabel));
					td.AddRow("Block Size", NTS.UInt8(tmpGraphCtrlExt.blockSize));
					td.AddRow("Packed Fields", NTS.UInt8(tmpGraphCtrlExt.packedFields));
					td.AddRow("Reserved", NTS.UInt8(tmpGraphCtrlExt.reserved));
					td.AddRow("Disposal Method", NTS.UInt8(tmpGraphCtrlExt.disposalMethod));
					td.AddRow("User Input Flag", NTS.Bool(tmpGraphCtrlExt.userInputFlag));
					td.AddRow("Transparent Color Flag", NTS.Bool(tmpGraphCtrlExt.transColorFlag));
					td.AddRow("Delay Time", NTS.UInt16(tmpGraphCtrlExt.delayTime));
					td.AddRow("Transparent Color Index", NTS.UInt8(tmpGraphCtrlExt.transColorIndex));
					td.AddRow("Block Terminator", NTS.UInt8(tmpGraphCtrlExt.blockTerminator));
					return td;
				}
				else if(tnFullPath.EndsWith("Application Extension"))
				{	
					AppExt tmpAppExt = (AppExt)(tmpImage.extensions[tnIndex]);

					TextTableData td = new TextTableData(2);
					td.SetColumn(0, "Property", 225);
					td.SetColumn(1, "Value", 100);

					td.AddRow("Extension Introducer", NTS.UInt8(tmpAppExt.extIntro));
					td.AddRow("Application Extension Label", NTS.UInt8(tmpAppExt.extLabel));
					td.AddRow("Block Size", NTS.UInt8(tmpAppExt.blockSize));
					td.AddRow("Application Identifier", Encoding.ASCII.GetString(tmpAppExt.appId));
					td.AddRow("Application Authentication Code", Encoding.ASCII.GetString(tmpAppExt.appAuthCode));
					td.AddRow("Application Data", NTS.ByteArray(tmpAppExt.appData));
					
					return td;
				}
			}

			return null;
		}

		public override QuickInfoTableData GetQuickInfo()
		{		
			QuickInfoTableData td = new QuickInfoTableData();
			
			// Image width
			td.AddRow("Image Width", m_DataStream.logScreenDesc.LogScreenWidth.ToString());

			// Image height
			td.AddRow("Image Height", m_DataStream.logScreenDesc.LogScreenHeight.ToString());
			
			// Global Color Table Size
			if(m_DataStream.logScreenDesc.GlobalClrTab)
			{
				int numColors = (int)Math.Pow(2, m_DataStream.logScreenDesc.SizeOfGlobalClrTable + 1);
				td.AddRow("Number of Colors", numColors.ToString());
			}
				
			return td;
		}
		
		public override Image GetImage()
		{
			Image myImage = null;
			try
			{							
				m_FileStream.Position = 0;
				myImage = Image.FromStream(m_FileStream);
			}
			catch(System.Runtime.InteropServices.ExternalException e)
			{
				Log.NewException(e, "GIF", "GetImage()", "Error drawing Image (ExternalException)");
			}
			catch(Exception e)
			{
				Log.NewException(e, "GIF", "GetImage()", "Error drawing Image (Exception)");
			}

			return myImage;
		}
	}
}
