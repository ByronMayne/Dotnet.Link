using System.Xml.Serialization;

namespace Dotnet.Link.Build
{
	public abstract class MSItem
	{
		[XmlAttribute("Include")]
		public string? Include { get; set; } = null;

		[XmlAttribute("Remove")]
		public string? Remove { get; set; } = null;

		[XmlAttribute("Update")]
		public string? Update { get; set; } = null;
	}
}
