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
}
