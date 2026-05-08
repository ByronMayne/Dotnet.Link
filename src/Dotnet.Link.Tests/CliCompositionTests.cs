using System.CommandLine;

namespace Dotnet.Link.Tests;

public class CliCompositionTests
{
    [Fact]
    public void Cli_Should_Register_Unlink_Or_List_Subcommands()
    {
        RootCommand appCommand = TestCommandFactory.CreateInternalRootCommand("Mayne.Dotnet.Link.Commands.AppCommand");
        Assert.Contains(appCommand.Subcommands, command => command.Name == "unlink");
        Assert.Contains(appCommand.Subcommands, command => command.Name == "list");
    }
}
