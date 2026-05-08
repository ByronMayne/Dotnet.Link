using Dotnet.Link;
using Dotnet.Link.Build;
using Spectre.Console;
using System.CommandLine;
using System.ComponentModel;

namespace Mayne.Dotnet.Link.Commands
{

	internal class LinkCommand : Command
	{
		public LinkCommand() : base("link")
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

			Option<string?> targetFramework = new Option<string?>("--target-framework", "Target framework to use when evaluating multi-target projects.");

			AddOption(nugetProject);
			AddOption(targetProject);
			AddOption(targetFramework);

			this.SetHandler(InvokeAsync, nugetProject, targetProject, targetFramework);
		}

		private async Task InvokeAsync(FileInfo nugetProject, FileInfo targetProject, string? targetFramework)
		{

			AnsiConsole.MarkupLine($"[grey66] Linking [lightsalmon3]{targetProject.Name}[/] to [lightsalmon3]{nugetProject.Name}[/][/]");

			MSProject propsProject = new MSProject();
			MSProject targetsProject = new MSProject();
			targetFramework = await ProjectHelpers.GetTargetFrameworkAsync(targetProject, targetFramework);

			// Remove the existing nuget package 
			string? nugetPackageName = await DotnetCommands.GetPropertyAsync(nugetProject.FullName, "PackageId");
			if (!string.IsNullOrEmpty(nugetPackageName))
			{
				AnsiConsole.MarkupLine($"[grey66] Removing existing nuget package [lightsalmon3]{nugetPackageName}[/] from [lightsalmon3]{targetProject.Name}[/][/]");
				targetsProject.Items.Add(new PackageReference() { Remove = nugetPackageName });
			}

			// Add nuget packages 
			IList<ProjectItem> nugetReferences = await DotnetCommands.GetItems(nugetProject.FullName, "PackageReference", targetFramework: targetFramework);
			IList<ProjectItem> packageVersions = await DotnetCommands.GetItems(nugetProject.FullName, "PackageVersion", targetFramework: targetFramework);
			IList<ProjectItem> targetProjectPackageReferences = await DotnetCommands.GetItems(targetProject.FullName, "PackageReference");
			bool.TryParse(await DotnetCommands.GetPropertyAsync(targetProject.FullName, "ManagePackageVersionsCentrally"), out bool isCentrallyManaged);

			AddTransativeNugetReferences(targetProject, targetsProject, nugetPackageName, nugetReferences, packageVersions, targetProjectPackageReferences, isCentrallyManaged);

			IList<ProjectItem> nugetInputs = await DotnetCommands.GetItems(nugetProject.FullName, "NuGetPackInput", "GenerateNuspec", targetFramework);
			if (nugetInputs.Count == 0 && !string.IsNullOrWhiteSpace(targetFramework))
			{
				nugetInputs = await DotnetCommands.GetItems(nugetProject.FullName, "BuiltProjectOutputGroupOutput", "BuiltProjectOutputGroup", targetFramework);
			}

			foreach (ProjectItem nuget in nugetInputs)
			{
				// Dlls from main project 
				if (nuget.IsKeyOutput && string.Equals(nuget.Extension, ".dll", StringComparison.OrdinalIgnoreCase))
				{
					Reference reference = new Reference()
					{
						Include = nuget.Filename,
						HintPath = GetReferencePath(nuget)
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

		private static void AddTransativeNugetReferences(
			FileInfo targetProject,
			MSProject targetsProject,
			string? nugetPackageName,
			IList<ProjectItem> nugetReferences,
			IList<ProjectItem> packageVersions,
			IList<ProjectItem> targetProjectPackageReferences,
			bool isCentrallyManaged)
		{
			int includeCount = 1;
			Dictionary<string, string> packageVersionLookup = packageVersions
				.Where(p => !string.IsNullOrWhiteSpace(p.Identity) && TryGetVersion(p) is not null)
				.ToDictionary(p => p.Identity, p => TryGetVersion(p)!, StringComparer.OrdinalIgnoreCase);
			HashSet<string> directTargetPackageReferences = targetProjectPackageReferences
				.Where(p => !string.IsNullOrWhiteSpace(p.Identity))
				.Select(p => p.Identity)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			AnsiConsole.MarkupLine($"[grey66] Adding transitive NuGet references of of [lightsalmon3]{nugetPackageName}[/] to [lightsalmon3]{targetProject.Name}[/][/]");
			foreach (ProjectItem package in nugetReferences)
			{
				if (!package.TryGet("IsImplicitlyDefined", false))
				{
					string packageName = package.Identity;

					if (directTargetPackageReferences.Contains(packageName))
					{
						AnsiConsole.MarkupLine($"[grey66] Skipping [lightsalmon3]{packageName}[/] because {targetProject.Name} already references it directly[/]");
						continue;
					}

					string? packageVersion = TryGetVersion(package);
					if (string.IsNullOrWhiteSpace(packageVersion))
					{
						packageVersionLookup.TryGetValue(packageName, out packageVersion);
					}

					if (string.IsNullOrWhiteSpace(packageVersion))
					{
						throw new InvalidOperationException($"Unable to resolve a version for transitive package '{packageName}'.");
					}

					includeCount++;
					targetsProject.Items.Add(PackageReference.Create(packageName, packageVersion, isCentrallyManaged));
					AnsiConsole.MarkupLine($"[lightskyblue3]  [darkseagreen]{includeCount}.[/] {packageName} @ {packageVersion}[/]");
				}
			}
		}

		private static string? TryGetVersion(ProjectItem package)
		{
			return package.TryGetValue("Version", out string? version) && !string.IsNullOrWhiteSpace(version)
				? version
				: null;
		}

		private static string GetReferencePath(ProjectItem nugetInput)
		{
			return nugetInput.TryGetValue("FinalOutputPath", out string? finalOutputPath) && !string.IsNullOrWhiteSpace(finalOutputPath)
				? finalOutputPath
				: nugetInput.FullPath;
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
