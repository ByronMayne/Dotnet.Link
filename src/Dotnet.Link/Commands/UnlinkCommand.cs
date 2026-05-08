using System.CommandLine;
using Spectre.Console;

namespace Mayne.Dotnet.Link.Commands
{
    internal class UnlinkCommand : Command
    {
        public UnlinkCommand() : base("unlink")
        {
            Description = "Removes locally linked NuGet references.";

            Option<FileInfo> targetProject = new Option<FileInfo>("--target-project", "The project to unlink from local generated artifacts.");
            targetProject.ExistingOnly();
            targetProject.SetDefaultValueFactory(GetDefaultTargetProject);
            targetProject.IsRequired = true;

            AddOption(targetProject);
            this.SetHandler(Invoke, targetProject);
        }

        private static void Invoke(FileInfo targetProject)
        {
            string targetFileName = $"{targetProject.Name}.link.g.targets";
            string propsFileName = $"{targetProject.Name}.link.g.props";
            string objFolder = Path.Combine(targetProject.Directory!.FullName, "obj");
            string targetsPath = Path.Combine(objFolder, targetFileName);
            string propsPath = Path.Combine(objFolder, propsFileName);

            DeleteIfExists(targetsPath, targetFileName);
            DeleteIfExists(propsPath, propsFileName);
        }

        private static void DeleteIfExists(string path, string displayName)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                AnsiConsole.MarkupLine($"[grey66] Deleted [lightsalmon3]{displayName}[/] from [lightsalmon3]obj/[/] folder[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[grey66] Skipping [lightsalmon3]{displayName}[/] because it was not found in [lightsalmon3]obj/[/][/]");
            }
        }

        private static FileInfo? GetDefaultTargetProject()
        {
            string? sourceProject = Directory.GetFiles(Environment.CurrentDirectory, "*.csproj")
                .FirstOrDefault();
            return sourceProject is not null
                ? new FileInfo(sourceProject)
                : null;
        }
    }
}
