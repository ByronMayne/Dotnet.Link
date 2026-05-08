using System.CommandLine;

namespace Mayne.Dotnet.Link.Commands
{
    internal class UnlinkCommand : Command
    {
        public UnlinkCommand() : base("unlink")
        {
            Description = "Removes locally linked NuGet references.";
        }
    }
}
