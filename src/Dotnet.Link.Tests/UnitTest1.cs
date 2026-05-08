using Dotnet.Link.Build;
using System.CommandLine;
using System.Reflection;

namespace Dotnet.Link.Tests;

public class BugRegressionTests
{
    [Fact]
    public void Cli_Should_Register_Unlink_Or_List_Subcommands()
    {
        RootCommand linkCommand = CreateInternalRootCommand("Mayne.Dotnet.Link.Commands.LinkCommand");
        Assert.NotEmpty(linkCommand.Subcommands);
    }

    [Fact]
    public void Unlink_Command_Should_Define_Options_For_Cleanup()
    {
        RootCommand unlinkCommand = CreateInternalRootCommand("Mayne.Dotnet.Link.Commands.UnlinkCommand");
        Assert.NotEmpty(unlinkCommand.Options);
    }

    [Fact]
    public async Task MultiTarget_Projects_Should_Not_Default_To_First_TargetFramework()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), $"dotnet-link-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        string projectPath = Path.Combine(tempDirectory, "MultiTarget.csproj");
        await File.WriteAllTextAsync(projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
              </PropertyGroup>
            </Project>
            """);

        try
        {
            Exception? exception = await Record.ExceptionAsync(() => InvokeGetTargetFrameworkAsync(projectPath));
            Assert.IsType<InvalidOperationException>(exception);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Import_Condition_Should_Serialize_As_Attribute()
    {
        MSProject project = new MSProject();
        project.Imports.Add(new Import
        {
            Project = "test.targets",
            Condition = "'$(Configuration)' == 'Debug'"
        });

        string xml = project.Serialize();
        Assert.Contains("Condition=", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void Unlink_Command_Should_Expose_Remove_Alias_Documented_In_Readme()
    {
        RootCommand unlinkCommand = CreateInternalRootCommand("Mayne.Dotnet.Link.Commands.UnlinkCommand");
        Assert.Contains("remove", unlinkCommand.Aliases, StringComparer.OrdinalIgnoreCase);
    }

    private static RootCommand CreateInternalRootCommand(string typeName)
    {
        Assembly assembly = typeof(MSProject).Assembly;
        Type? type = assembly.GetType(typeName, throwOnError: true);
        object? instance = Activator.CreateInstance(type!, nonPublic: true);
        Assert.NotNull(instance);
        return Assert.IsAssignableFrom<RootCommand>(instance);
    }

    private static async Task<string?> InvokeGetTargetFrameworkAsync(string projectPath)
    {
        Assembly assembly = typeof(MSProject).Assembly;
        Type? commandType = assembly.GetType("Mayne.Dotnet.Link.Commands.LinkCommand", throwOnError: true);
        MethodInfo? method = commandType!.GetMethod("GetTargetFrameworkAsync", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        try
        {
            object? taskObject = method!.Invoke(null, new object[] { new FileInfo(projectPath) });
            Assert.NotNull(taskObject);

            Task task = Assert.IsAssignableFrom<Task>(taskObject);
            await task;

            PropertyInfo? resultProperty = taskObject!.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            return resultProperty?.GetValue(taskObject) as string;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }
}
