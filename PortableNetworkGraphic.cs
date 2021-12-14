using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Text;

using Ufex.API;
using Ufex.API.Tables;
using System.Collections.Generic;
using System.Linq;

namespace Ufex.FileTypes.Image
{
	/// <summary>
	/// PNG FileType module for Ufex
	/// </summary>
	public class PortableNetworkGraphic : ImageFileType
	{
		Byte[] PNG_HEADER = new Byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

		Dictionary<string, Type> CHUNK_TYPES = new Dictionary<string, Type>()
		{
			{ "IHDR", typeof(Chnk_IHDR) },
			{ "IDAT", typeof(Chnk_IDAT) },
			{ "IEND", typeof(Chnk_IEND) },
			{ "PLTE", typeof(Chnk_PLTE) },
			{ "bKGD", typeof(Chnk_bKGD) },
			{ "gAMA", typeof(Chnk_gAMA) },
			{ "cHRM", typeof(Chnk_cHRM) },
			{ "iCCP", typeof(Chnk_iCCP) },
			{ "tIME", typeof(Chnk_tIME) },
			{ "tRNS", typeof(Chnk_tRNS) },
			{ "hIST", typeof(Chnk_hIST) },
			{ "sPLT", typeof(Chnk_sPLT) },
			{ "sRGB", typeof(Chnk_sRGB) },
			{ "sBIT", typeof(Chnk_sBIT) },
			{ "pHYs", typeof(Chnk_pHYs) },
			{ "tEXt", typeof(Chnk_tEXt) },
			{ "zTXt", typeof(Chnk_zTXt) },
			{ "iTXt", typeof(Chnk_iTXt) },
		};

		const String CHUNK_IHDR = "IHDR";
		const String CHUNK_tRNS = "tRNS";


		Byte[] header;
		ArrayList m_Chunks;
		FileReader fr;
		
		public PortableNetworkGraphic()
		{
			ShowGraphic = true;
			ShowTechnical = true;
			ShowFileCheck = false;
			Log.SetLogName("PNG.log");
		}

		public override bool ProcessFile()
		{
			fr = new FileReader(m_FileStream, false);
			
			m_Chunks = new ArrayList();

			// Read the header
			header = fr.ReadBytes(8);
			if(!CompareArrays(header, PNG_HEADER))
			{
				// Invalid header
				return false;
			}

			while(m_FileStream.Position < m_FileStream.Length)
			{
				int colorType = -1;

				UInt32 length = fr.ReadUInt32();
				char[] chunkType = fr.ReadChars(4);
				string chunkTypeStr = new string(chunkType);

				Int64 startPos = m_FileStream.Position;
				TreeNode tnTemp = new TreeNode(chunkTypeStr, (int)TreeViewIcon.Section, (int)TreeViewIcon.Section);
				TreeNodeTag tag = new TreeNodeTag();
				tag.FileRegion = new FileSpan(m_FileStream.Position - 8, m_FileStream.Position + length + 4);
				tnTemp.Tag = tag;
				TreeNodes.Add(tnTemp);

				Chunk newChunk = null;

				if(chunkType == null)
					newChunk = new Chunk();
				else if(CHUNK_TYPES.ContainsKey(chunkTypeStr))
				{
					newChunk = (Chunk)Activator.CreateInstance(CHUNK_TYPES[chunkTypeStr]);
					if(chunkTypeStr.Equals(CHUNK_IHDR))
					{
						colorType = (int)((Chnk_IHDR)newChunk).colorType;
					}
					else if(chunkTypeStr == CHUNK_tRNS)
					{
						Chnk_tRNS tmp = (Chnk_tRNS)newChunk;
						tmp.colorType = colorType;
					}
				}

				if(newChunk == null)
					newChunk = new Chunk();

				newChunk.length = length;
				newChunk.chunkType = chunkType;

				newChunk.ReadChunk(fr);

				Int64 endPos = m_FileStream.Position;
				m_FileStream.Seek(length - (endPos - startPos), SeekOrigin.Current);

				newChunk.crc.crcValue = fr.ReadBytes(4);

				m_Chunks.Add(newChunk);
			}

			return true;
		}

		public override TableData GetData(TreeNode tn)
		{
			if(tn.Index < m_Chunks.Count)
			{
				Chunk tmpChunk = (Chunk)m_Chunks[tn.Index];
				return tmpChunk.GetTableData(NTS);
			}
			return base.GetData (tn);
		}

		public override QuickInfoTableData GetQuickInfo()
		{
			QuickInfoTableData td = new QuickInfoTableData();

			System.Drawing.Image myImage = null;
			try
			{							
				m_FileStream.Position = 0;
				myImage = System.Drawing.Image.FromStream(m_FileStream);
			}
			catch(System.Runtime.InteropServices.ExternalException e)
			{
				ExceptionOut(e);
				DebugOut("Error drawing Image: " + e.ToString());
			}
			catch(Exception e)
			{
				ExceptionOut(e);
				DebugOut("An error occured while drawing the image: " + e.ToString());
			}

			if(myImage != null)
			{
				td.AddRow("Width", myImage.Width.ToString());
				td.AddRow("Height", myImage.Height.ToString());
				string pixFormat = myImage.PixelFormat.ToString();

				if(myImage.PixelFormat == PixelFormat.Format16bppArgb1555)
					pixFormat = "16-bpp ARGB 1555";
				else if(myImage.PixelFormat == PixelFormat.Format16bppGrayScale)
					pixFormat = "16-bpp Gray Scale";
				else if(myImage.PixelFormat == PixelFormat.Format24bppRgb)
					pixFormat = "24-bpp RGB";
				else if(myImage.PixelFormat == PixelFormat.Format32bppArgb)
					pixFormat = "32-bpp ARGB";
				else if(myImage.PixelFormat == PixelFormat.Format8bppIndexed)
					pixFormat = "8-bpp Indexed";


				td.AddRow("Pixel Format", pixFormat);

				int [] propIDList = myImage.PropertyIdList;
				PropertyItem [] propItems = myImage.PropertyItems;
				
				for(int i = 0; i < propItems.GetLength(0); i++)
				{
					DebugOut(String.Concat("Prop: ", propItems[i].Id.ToString()));
					string valueString = null;

					if(propItems[i].Type == 2)			// String
					{
						valueString = Encoding.ASCII.GetString(propItems[i].Value);
					}

					switch(propItems[i].Id)
					{
						case 0x0131:
							td.AddRow("Software Used", valueString);
							break;

						case 0x0132:
							td.AddRow("Date/Time", valueString);
							break;

						case 0x0301:
							ArrayToNum atn = new ArrayToNum(Endian.Big);
							double gamma = (double)(atn.GetInt32(propItems[i].Value, 0)) / (double)(atn.GetInt32(propItems[i].Value, 4));
							td.AddRow("Gamma", gamma.ToString());
							break;

						case 0x5110:
							td.AddRow("Pixel Unit", propItems[i].Value[0].ToString());
							break;

						default:
							// Do Nothing
							break;
					}
				
				}

				myImage.Dispose();
			}

			return td;
		}

		private bool CompareArrays(byte[] array1, byte[] array2)
		{
			if(array1 == null || array2 == null)
				return false;

			return array1.SequenceEqual(array2);
		}
	}
}
