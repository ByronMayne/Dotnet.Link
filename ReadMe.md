# Dotnet Link

When working on NuGet packages it can be quite annoying iterate on. When referencing a NuGet package it will setup analyzers, build files, assembly references, and content for you. If you want to iterate locally it's either keep building the package and increment the version and doing updates, or manually setup all the references. 

What this tool does is generate build `props` and `targets` to link a nuget project to another project as if it was really a NuGet package. This allows you do development of the library and use it in another one.


## How It Works 
When you run the tool it will generate `/obj/{project-name}.csproj.link.g.props` and `/obj/{project-name}.csproj.link.g.props` and add them to the referencing project. Because of the naming convention MSBuild will load them and within this files we setup all the references to make it work like a NuGet reference. If you want to remove the reference either clean the project, or do `dotnet link remove` command to get rid of them. 