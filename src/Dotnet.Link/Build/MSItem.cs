using System.Xml.Serialization;

namespace Dotnet.Link.Build
{
	public abstract class MSItem
	{
		[XmlAttribute("Include")]
		public required string Include { get; set; }
    }
}
