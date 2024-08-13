using System.Xml;
using System.Xml.Serialization;

namespace Dotnet.Link.Build
{
	public abstract class MSProperty
    {
        [XmlText]
        public string Value { get; }

        public MSProperty()
        {
            Value = null!;
        }
    }
}
