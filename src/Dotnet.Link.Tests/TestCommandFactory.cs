using Dotnet.Link.Build;
using System.CommandLine;
using System.Reflection;

namespace Dotnet.Link.Tests;

internal static class TestCommandFactory
{
    internal static RootCommand CreateInternalRootCommand(string typeName)
    {
        Command command = CreateInternalCommand(typeName);
        return Assert.IsAssignableFrom<RootCommand>(command);
    }

    internal static Command CreateInternalCommand(string typeName)
    {
        Assembly assembly = typeof(MSProject).Assembly;
        Type? type = assembly.GetType(typeName, throwOnError: true);
        object? instance = Activator.CreateInstance(type!, nonPublic: true);
        Assert.NotNull(instance);
        return Assert.IsAssignableFrom<Command>(instance);
    }

    internal static async Task<string?> InvokeGetTargetFrameworkAsync(string projectPath)
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

            PropertyInfo? resultProperty = taskObject.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            return resultProperty?.GetValue(taskObject) as string;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }
}
