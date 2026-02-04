
using System;
using System.IO;
using Ufex.API;
using Ufex.API.Validation;

namespace Ufex.FileTypes.ZIP.Data;

internal class CompressedFile : Section
{
	public LocalFileHeader Header { get; init; }
	public FileData FileData { get; init; }
	public DataDescriptor? DataDescriptor { get; init; }

	public CompressedFile(BinaryReader br, ulong dataSize, ValidationReport validationReport)
	{
		StartRead(br);
		// Read Local File Header
		Header = new LocalFileHeader(br);
		
		// Read File Data
		FileData = new FileData(br, dataSize, Header.CompressionMethod, validationReport);

		// Read Data Descriptor if present
		if(Header.HasDataDescriptor)
		{
			DataDescriptor = new DataDescriptor(br);
		}
		EndRead(br);
	}
}