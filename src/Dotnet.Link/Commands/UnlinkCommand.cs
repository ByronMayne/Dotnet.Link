using System.CommandLine;

namespace Mayne.Dotnet.Link.Commands
{
    internal class UnlinkCommand : RootCommand
    {
        public UnlinkCommand() : base()
        {
            Description = "Removes locally linked NuGet references.";
        }
    }
}
