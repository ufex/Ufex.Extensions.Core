using System;
using System.IO;
using Ufex.API;
using Ufex.API.Validation;

namespace Ufex.Extensions.Core.ZIP.Data;

public class DataDescriptor : Section
{
	public UInt32? DataDescriptorSignature { get; init; }
	public UInt32 Crc32 { get; init; }
	public UInt32 CompressedSize { get; init; }
	public UInt32 UncompressedSize { get; init; }

	public DataDescriptor(BinaryReader br)
	{
		StartRead(br);
		UInt32 possibleSignature = br.ReadUInt32();
		if(possibleSignature == Signatures.DATA_DESCRIPTOR_SIGNATURE) 
		{
			DataDescriptorSignature = possibleSignature;
		} 
		else 
		{
			// No signature, rewind
			br.BaseStream.Seek(-4, SeekOrigin.Current);
		}
		Crc32 = br.ReadUInt32();
		CompressedSize = br.ReadUInt32();
		UncompressedSize = br.ReadUInt32();
		EndRead(br);
	}
}