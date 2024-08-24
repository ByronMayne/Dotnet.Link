using System.Xml;
using System.Xml.Serialization;

namespace Dotnet.Link.Build
{


	[XmlRoot]
    [XmlType("Project", Namespace = NAMESPACE)]
    public class MSProject
    {
        private const string NAMESPACE = "http://schemas.microsoft.com/developer/msbuild/2003";


		[XmlAttribute("ToolsVersion")]
        public string ToolsVersion { get; set; }

        [XmlArray("PropertyGroup")]
        public List<MSProperty> Properties { get; set; }

        [XmlArray("ItemGroup")]
        [XmlArrayItem(typeof(PackageReference))]
        [XmlArrayItem(typeof(Reference))]
        [XmlArrayItem(typeof(AnalyzerItem))]
        public List<MSItem> Items { get; set; }

        [XmlArray("ImportGroup")]
        public ImportGroup Imports { get; }

        public MSProject()
        {
            ToolsVersion = "14.0";
			Imports = new ImportGroup();
            Items = new List<MSItem>();
			Properties = new List<MSProperty>();
		}

        public void Serialize(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                CloseOutput = false,
            };
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                Type type = typeof(MSProject);
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
               
                namespaces.Add("", NAMESPACE);

                XmlSerializer serializer = new XmlSerializer(type, NAMESPACE);
                serializer.Serialize(writer, this, namespaces);
            }
        }

        public string Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                Serialize(stream);
                stream.Position = 0;
                return reader.ReadToEnd();
            }
        }
    }
}
