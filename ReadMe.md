# Dotnet Link

Dotnet Link helps you test a local NuGet package project inside another project without publishing a package on every change.

## What Problems It Solves

When you reference a package from NuGet, MSBuild sets up more than just DLL references. It can also bring in:

- analyzers
- build .props and .targets files
- transitive package references

During local development, reproducing all of that by hand is time consuming and error prone.

Dotnet Link solves this by generating MSBuild files in the target project's obj folder so your local project behaves more like a real package reference.

## How It Works

The link command generates these files in the target project's obj folder:

- {TargetProjectName}.link.g.props
- {TargetProjectName}.link.g.targets

MSBuild automatically imports generated .props and .targets files from obj, so these files become part of the normal build.

The unlink command removes those generated files.

## Commands

### link

Links a local package project into a target project.

Options:

- --nuget-project <path> (required)
- --target-project <path> (required)
- --target-framework <tfm> (optional, required for multi-target projects)

Example:

```powershell
dotnet link link \
	--nuget-project .\src\MyPackage\MyPackage.csproj \
	--target-project .\src\MyApp\MyApp.csproj
```

Multi-target example:

```powershell
dotnet link link \
	--nuget-project .\src\MyPackage\MyPackage.csproj \
	--target-project .\src\MyApp\MyApp.csproj \
	--target-framework net8.0
```

### unlink

Removes generated link artifacts from the target project.

Options:

- --target-project <path> (required)

Example:

```powershell
dotnet link unlink --target-project .\src\MyApp\MyApp.csproj
```

## Typical Workflow

1. Run link once for your package project and target app.
2. Build and run the target app as normal.
3. Make changes in your package project.
4. Rebuild to validate changes in the target app.
5. Run unlink when you want to remove the local link setup.

## Notes

- For multi-target target projects, pass --target-framework to avoid ambiguity.
- Generated link files are created in obj and are safe to regenerate.
- The list command exists but is not implemented yet.