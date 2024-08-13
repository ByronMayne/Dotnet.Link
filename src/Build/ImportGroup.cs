using System.Xml.Serialization;

namespace Dotnet.Link.Build
{
	[XmlType("ImportGroup")]
    public class ImportGroup : List<Import>
    {

    }
}
