using System.CommandLine;

namespace Dotnet.Link.Tests;

public class UnlinkTests
{
    [Fact]
    public void Unlink_Command_Should_Define_Options_For_Cleanup()
    {
        Command unlinkCommand = TestCommandFactory.CreateInternalCommand("Mayne.Dotnet.Link.Commands.UnlinkCommand");
        Assert.NotEmpty(unlinkCommand.Options);
    }

    [Fact]
    public async Task Unlink_Command_Should_Remove_Generated_Link_Files()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), $"dotnet-link-unlink-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        string projectPath = Path.Combine(tempDirectory, "TargetProject.csproj");
        string objPath = Path.Combine(tempDirectory, "obj");
        Directory.CreateDirectory(objPath);

        string generatedTargets = Path.Combine(objPath, "TargetProject.csproj.link.g.targets");
        string generatedProps = Path.Combine(objPath, "TargetProject.csproj.link.g.props");

        await File.WriteAllTextAsync(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        await File.WriteAllTextAsync(generatedTargets, "<Project />");
        await File.WriteAllTextAsync(generatedProps, "<Project />");

        try
        {
            RootCommand appCommand = TestCommandFactory.CreateInternalRootCommand("Mayne.Dotnet.Link.Commands.AppCommand");
            int exitCode = await appCommand.InvokeAsync(new[] { "unlink", "--target-project", projectPath });

            Assert.Equal(0, exitCode);
            Assert.False(File.Exists(generatedTargets));
            Assert.False(File.Exists(generatedProps));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Unlink_Command_Should_Expose_Remove_Alias_Documented_In_Readme()
    {
        Command unlinkCommand = TestCommandFactory.CreateInternalCommand("Mayne.Dotnet.Link.Commands.UnlinkCommand");
        Assert.Contains("remove", unlinkCommand.Aliases, StringComparer.OrdinalIgnoreCase);
    }
}
