using System;
using System.Collections.Generic;
using System.Linq;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Validation;

namespace Ufex.Extensions.Core.GZIP;

public class GzipStreamReader
{
	private Stream _fileStream;
	internal List<Member> Members { get; init; }
	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }
	public int ColorType { get; private set; }

	public GzipStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		_fileStream = fileStream;
		Members = new List<Member>();
		Log = log;
		ValidationReport = validationReport;
	}

	public bool Read()
	{
		FileReader fr = new FileReader(_fileStream, Endian.Little);

		while(_fileStream.Position < _fileStream.Length)
		{
			Member member = new Member(fr);
			Members.Add(member);
			break; // TODO: Implement reading the compressed data and the footer, and then loop to read the next member if it exists
			Log.Info($"Position = {_fileStream.Position}");
		}

		return true;
	}
}