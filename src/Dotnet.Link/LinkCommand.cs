using Dotnet.Link.Build;
using Spectre.Console;
using System.CommandLine;

namespace Dotnet.Link
{
	internal class LinkCommand : RootCommand
	{
		public LinkCommand() : base()
		{
			Description = "Takes a project and references another project on your computer as if it was a NuGet package. " +
				"allowing you to itorate on the package without having to publish or build NuGet references";

			Option<FileInfo> nugetProject = new Option<FileInfo>("--nuget-project", "The file path to the local nuget project that you want to link.");
			nugetProject.ExistingOnly();
			nugetProject.IsRequired = true;


			Option<FileInfo> targetProject = new Option<FileInfo>("--target-project", "The project that you want to have reference the source project.");
			targetProject.ExistingOnly();
			targetProject.SetDefaultValueFactory(GetDefaultTargetProject);
			targetProject.IsRequired = true;

			AddOption(nugetProject);
			AddOption(targetProject);

			this.SetHandler(InvokeAsync, nugetProject, targetProject);
		}

		private async Task InvokeAsync(FileInfo nugetProject, FileInfo targetProject)
		{

			AnsiConsole.MarkupLine($"[grey66] Linking [lightsalmon3]{targetProject.Name}[/] to [lightsalmon3]{nugetProject.Name}[/][/]");

			MSProject propsProject = new MSProject();
			MSProject targetsProject = new MSProject();

			IList<ProjectItem> nugetInputs = await GetProjectItemsCommand.GetItems(nugetProject.FullName, "NuGetPackInput", "GenerateNuspec");

			foreach (ProjectItem nuget in nugetInputs)
			{
				// Dlls from main project 
				if (nuget.IsKeyOutput && string.Equals(nuget.Extension, ".dll", StringComparison.OrdinalIgnoreCase))
				{
					Reference reference = new Reference()
					{
						Include = nuget.Filename,
						HintPath = nuget.FullPath
					};

					propsProject.Items.Add(reference);
				}

				if (nuget.Pack || !string.IsNullOrWhiteSpace(nuget.PackagePath))
				{
					string packagePath = $"{nuget.Filename}{nuget.Extension}";

					if (!string.IsNullOrWhiteSpace(nuget.PackagePath))
					{
						packagePath = string.Equals(Path.GetExtension(nuget.PackagePath), nuget.Extension)
							? nuget.PackagePath
							: Path.Combine(nuget.PackagePath, $"{nuget.Filename}{nuget.Extension}");
					}

					// Check if it's a builder folder
					switch (packagePath
						.TrimStart('/', '\\')
						.Split('/', '\\')
						.FirstOrDefault()
						?.ToLower())
					{
						case "analyzers":
							propsProject.Items.Add(new AnalyzerItem()
							{
								Include = nuget.FullPath
							});
							break;
						case "build":
							Import import = new Import() { Project = nuget.FullPath };
							switch (Path.GetExtension(nuget.FullPath).ToLower())
							{
								case ".props":
									propsProject.Imports.Add(import);
									break;
								case ".targets":
									targetsProject.Imports.Add(import);
									break;

							}
							break;
					}
				}

			}


			string targetFileName = $"{targetProject.Name}.link.g.targets";
			string propsFileName = $"{targetProject.Name}.link.g.props";
			string objFolder = Path.Combine(targetProject.Directory!.FullName, "obj");
			string targetsPath = Path.Combine(objFolder, targetFileName);
			string propsPath = Path.Combine(objFolder, propsFileName);



			Directory.CreateDirectory(objFolder);
			File.WriteAllText(targetsPath, targetsProject.Serialize());
			AnsiConsole.MarkupLine($"[grey66] Writing [lightsalmon3]{targetFileName}[/] to [lightsalmon3]obj/[/] folder[/]");
			File.WriteAllText(propsPath, propsProject.Serialize());
			AnsiConsole.MarkupLine($"[grey66] Writing [lightsalmon3]{propsFileName}[/] to [lightsalmon3]obj/[/] folder[/]");
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
