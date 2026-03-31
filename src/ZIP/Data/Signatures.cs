namespace Ufex.Extensions.Core.ZIP.Data;

public static class Signatures
{
	public const UInt32 LOCAL_FILE_SIGNATURE = 0x04034b50;
	public const UInt32 CENTRAL_FILE_SIGNATURE = 0x02014b50;
	public const UInt32 DATA_DESCRIPTOR_SIGNATURE = 0x08074b50;
	public const UInt32 DIGITAL_SIGNATURE_SIGNATURE = 0x05054b50;
	public const UInt32 ARCHIVE_EXTRA_DATA_SIGNATURE = 0x08064b50;
	public const UInt32 ZIP64_END_OF_CENTRAL_DIR_SIGNATURE = 0x07064b50;
	public const UInt32 END_OF_CENTRAL_DIR_SIGNATURE = 0x06054b50;
}