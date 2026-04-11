using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.ThreeGpp;

/// <summary>
/// loci — Location Information Box node. Displays geolocation data.
/// </summary>
internal class LociBoxNode : BoxNode
{
	public LociBoxNode(LociBox box)
		: base(box, "loci", "Location Information", TreeViewIcon.Information)
	{
	}

	public override object[][] GetRows()
	{
		var box = (LociBox)_box;
		string roleDesc = LociBox.RoleNames.GetValueOrDefault(box.Role, "Unknown");

		return [
			[ "Language", box.Language, box.LanguageString ],
			[ "Name", box.Name, box.NameString ],
			[ "Role", box.Role, roleDesc ],
			[ "Longitude", box.Longitude, $"{box.LongitudeDecimal:F6}°" ],
			[ "Latitude", box.Latitude, $"{box.LatitudeDecimal:F6}°" ],
			[ "Altitude", box.Altitude, $"{box.AltitudeDecimal:F2} m" ],
			[ "Astronomical Body", box.AstronomicalBody, box.AstronomicalBodyString ],
			[ "Additional Notes", box.AdditionalNotes, box.AdditionalNotesString ],
		];
	}
}
