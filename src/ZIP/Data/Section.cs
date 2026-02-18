using System;
using System.IO;
using System.Text;
using Ufex.API;
using Ufex.API.Validation;

namespace Ufex.Extensions.Core.ZIP.Data;

public abstract class Section
{

	public long StartPosition { get; protected set; }
	public long EndPosition { get; protected set; }

	protected void StartRead(BinaryReader br)
	{
		StartPosition = (long)br.BaseStream.Position;
	}

	protected void EndRead(BinaryReader br)
	{
		EndPosition = (long)br.BaseStream.Position;
	}

	protected string DecodeString(byte[] data, UInt16 GeneralPurposeBitFlag)
	{
		var encoding = ByteUtil.GetBit(GeneralPurposeBitFlag, 11) ? Encoding.UTF8 : Encoding.GetEncoding(437);
		return encoding.GetString(data);
	}

	/// <summary>
	/// Decodes a DOS date format into a string representation YYYY-MM-DD
	/// </summary>
	/// <param name="date"></param>
	/// <returns></returns>
	protected string DecodeDate(UInt16 date)
	{
		int year = ((date >> 9) & 0x7F) + 1980;
		int month = (date >> 5) & 0x0F;
		int day = date & 0x1F;
		return $"{year:D4}-{month:D2}-{day:D2}";
	}

	/// <summary>
	/// Decodes a DOS time format into a string representation HH:MM:SS
	/// </summary>
	/// <param name="time"></param>
	/// <returns>Time in HH:MM:SS format</returns>
	protected string DecodeTime(UInt16 time)
	{
		int hours = (time >> 11) & 0x1F;
		int minutes = (time >> 5) & 0x3F;
		int seconds = (time & 0x1F) * 2;
		return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
	}
}
