using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic;
using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

internal class Chunk
{
	private static readonly Dictionary<string, Type> CHUNK_TYPES = new()
	{
		{ "RIFF", typeof(RiffChunk) },
		{ "LIST", typeof(ListChunk) },
		{ "CSET", typeof(CsetChunk) }
	};

	private static readonly Dictionary<string, Type> RIFF_CHUNK_TYPES = new()
	{
		{ "WAVE", typeof(Data.Wave.WaveRiffChunk) }
	};

	private static readonly Dictionary<string, Type> LIST_CHUNK_TYPES = new()
	{
		{ "INFO", typeof(InfoListChunk) }
	};

	/// <summary>
	/// 4-character code that identifies the type of chunk (FourCC). 
	/// For RIFF files, this is typically an ASCII string.
	/// </summary>
	public Byte[] ChunkID { get; init; }   // 4 bytes

	/// <summary>
	/// Size of the chunk data in bytes, not including the 8 bytes for the chunk ID and 
	/// size fields, or the optional padding byte. 
	/// If this value is odd, there will be a padding byte after the chunk data to ensure 
	/// that the next chunk starts on an even byte boundary.
	/// </summary>
	public UInt32 Size { get; init; }

	/// <summary>
	/// The offset in the file where this chunk starts, including the 8 bytes for the chunk ID and size fields.
	/// </summary>
	public long Offset { get; init; }

	public string ChunkIDString
	{
		get { return System.Text.Encoding.ASCII.GetString(ChunkID); }
	}

	public Chunk(FileReader fr)
	{
		Offset = fr.BaseStream.Position;
		ChunkID = fr.ReadBytes(4);
		Size = fr.ReadUInt32();
	}

	public static List<Chunk> ReadSubChunks(FileReader fr, UInt32 size, Dictionary<string, Type>? subChunkTypes = null)
	{
		List<Chunk> subChunks = new List<Chunk>();
		long endOfData = fr.BaseStream.Position + size;

		while (fr.BaseStream.Position + 8 <= endOfData)
		{
			// Determine the subchunk id/size
			byte[] chunkID = fr.ReadBytes(4);
			UInt32 chunkSize = fr.ReadUInt32(); // Does not include the id, size, or padding byte
			string chunkIDString = System.Text.Encoding.ASCII.GetString(chunkID);
			byte[]? secondaryChunkID = null;
			if(chunkIDString == "LIST" || chunkIDString == "RIFF")
			{
				secondaryChunkID = fr.ReadBytes(4);
			}

			// Rewind, so the chunk constructor can read the full chunk data
			if(secondaryChunkID != null)
			{
				fr.BaseStream.Seek(-4, SeekOrigin.Current);
			}
			fr.BaseStream.Seek(-8, SeekOrigin.Current);

			// Determine the end position of the chunk in the file
			Int64 endPosition = fr.BaseStream.Position + 8 + chunkSize;
			bool hasPaddingByte = chunkSize % 2 != 0;
			if(hasPaddingByte)
			{
				// Add one byte for the padding byte
				endPosition += 1;
			}

			// Clamp to avoid reading past the parent chunk boundary
			endPosition = Math.Min(endPosition, endOfData);

			Chunk? subChunk;
			if(secondaryChunkID != null)
			{
				subChunk = Chunk.CreateListChunk(secondaryChunkID, fr);
			}
			else 
			{
				subChunk = Chunk.CreateChunk(chunkID, fr, subChunkTypes);
			}

			// If no specialized chunk type was found, fall back to a generic Chunk
			subChunk ??= new Chunk(fr);
			subChunks.Add(subChunk);

			// Move the stream position to the end of this sub-chunk's data
			fr.BaseStream.Seek(endPosition, SeekOrigin.Begin);
		}
		return subChunks;
	}

	public static Chunk? CreateChunk(byte[] chunkType, FileReader fr, Dictionary<string, Type>? subChunkTypes = null)
	{
		return CreateChunk(System.Text.Encoding.ASCII.GetString(chunkType), fr, subChunkTypes);
	}

	public static Chunk? CreateChunk(string chunkType, FileReader fr, Dictionary<string, Type>? subChunkTypes = null)
	{
		Type? chunkClass = null;
		if (!CHUNK_TYPES.TryGetValue(chunkType, out chunkClass))
		{
			// If no chunk class was found in the main dictionary, check the provided subChunkTypes dictionary (if any)
			if(subChunkTypes != null)
			{
				if(!subChunkTypes.TryGetValue(chunkType, out chunkClass))
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		return (Chunk?)Activator.CreateInstance(chunkClass, fr);
	}
	public static Chunk? CreateRiffChunk(byte[] chunkType, FileReader fr)
	{
		return CreateRiffChunk(System.Text.Encoding.ASCII.GetString(chunkType), fr);
	}

	public static Chunk? CreateRiffChunk(string chunkType, FileReader fr)
	{
		if (RIFF_CHUNK_TYPES.TryGetValue(chunkType, out var chunkClass))
			return (Chunk?)Activator.CreateInstance(chunkClass, fr);
		return new RiffChunk(fr);
	}

	public static Chunk? CreateListChunk(byte[] chunkType, FileReader fr)
	{
		return CreateListChunk(System.Text.Encoding.ASCII.GetString(chunkType), fr);
	}

	public static Chunk? CreateListChunk(string chunkType, FileReader fr)
	{
		if (!LIST_CHUNK_TYPES.TryGetValue(chunkType, out var chunkClass))
			return null;

		return (Chunk?)Activator.CreateInstance(chunkClass, fr);
	}

}