using System.Xml.Serialization;

namespace Dotnet.Link.Build
{
	public class PackageReference : MSItem
	{
		[XmlAttribute("Version")]
		public string? Version { get; set; }

		[XmlAttribute("OverrideVersion")]
		public string? OverrideVersion { get; set; }

		public static PackageReference Create(string name, string version, bool isCentrallyManaged = false)
			=> new PackageReference()
			{
				Include = name,
				Version = isCentrallyManaged ? null : version,
				OverrideVersion = isCentrallyManaged ? version : null
			};
	}
}
