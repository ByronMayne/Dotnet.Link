using System.CommandLine;

namespace Mayne.Dotnet.Link.Commands
{
    internal class AppCommand : RootCommand
    {
        public AppCommand() : base("Dotnet Link commands")
        {
            AddCommand(new LinkCommand());
            AddCommand(new UnlinkCommand());
            AddCommand(new ListCommand());
        }
    }
}