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
    public void Unlink_Command_Should_Expose_Remove_Alias_Documented_In_Readme()
    {
        Command unlinkCommand = TestCommandFactory.CreateInternalCommand("Mayne.Dotnet.Link.Commands.UnlinkCommand");
        Assert.Contains("remove", unlinkCommand.Aliases, StringComparer.OrdinalIgnoreCase);
    }
}
