namespace Dotnet.Link.Tests;

public class LinkTests
{
    [Fact]
    public async Task MultiTarget_Projects_Should_Not_Default_To_First_TargetFramework()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), $"dotnet-link-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        string projectPath = Path.Combine(tempDirectory, "MultiTarget.csproj");
        await File.WriteAllTextAsync(projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
              </PropertyGroup>
            </Project>
            """);

        try
        {
            Exception? exception = await Record.ExceptionAsync(() => TestCommandFactory.InvokeGetTargetFrameworkAsync(projectPath));
            Assert.IsType<InvalidOperationException>(exception);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }
}
