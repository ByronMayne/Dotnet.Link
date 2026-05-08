using Dotnet.Link.Build;

namespace Dotnet.Link.Tests;

public class BuildSerializationTests
{
    [Fact]
    public void Import_Condition_Should_Serialize_As_Attribute()
    {
        MSProject project = new MSProject();
        project.Imports.Add(new Import
        {
            Project = "test.targets",
            Condition = "'$(Configuration)' == 'Debug'"
        });

        string xml = project.Serialize();
        Assert.Contains("Condition=", xml, StringComparison.Ordinal);
    }
}
