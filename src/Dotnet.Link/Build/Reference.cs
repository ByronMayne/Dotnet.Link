using System.Xml.Serialization;

namespace Dotnet.Link.Build
{
	public class Reference : MSItem
	{
		[XmlElement("HintPath")]
		public string? HintPath { get; set; }
	}
}
