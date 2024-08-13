using System.Xml;
using System.Xml.Serialization;

namespace Dotnet.Link.Build
{
    public class Import
    {
        [XmlAttribute("Project")]
        public string Project { get; set; }

        public string? Condition { get; set; }

        public Import()
        {
            Project = "";
            Condition = null;
        }
    }
}
