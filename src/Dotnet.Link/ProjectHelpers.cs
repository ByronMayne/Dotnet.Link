namespace Dotnet.Link;

public static class ProjectHelpers
{
    public static async Task<string?> GetTargetFrameworkAsync(FileInfo targetProject, string? requestedTargetFramework = null)
    {
        string? targetFramework = await DotnetCommands.GetPropertyAsync(targetProject.FullName, "TargetFramework");
        if (!string.IsNullOrWhiteSpace(targetFramework))
        {
            if (!string.IsNullOrWhiteSpace(requestedTargetFramework) &&
                !string.Equals(targetFramework, requestedTargetFramework, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Requested target framework '{requestedTargetFramework}' does not match project target framework '{targetFramework}'.");
            }

            return targetFramework;
        }

        string? targetFrameworks = await DotnetCommands.GetPropertyAsync(targetProject.FullName, "TargetFrameworks");
        string[] frameworks = targetFrameworks?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .ToArray()
            ?? Array.Empty<string>();

        if (frameworks.Length == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(requestedTargetFramework))
        {
            if (frameworks.Contains(requestedTargetFramework, StringComparer.OrdinalIgnoreCase))
            {
                return frameworks.First(f => string.Equals(f, requestedTargetFramework, StringComparison.OrdinalIgnoreCase));
            }

            throw new InvalidOperationException(
                $"Requested target framework '{requestedTargetFramework}' was not found in target frameworks '{string.Join(";", frameworks)}'.");
        }

        if (frameworks.Length == 1)
        {
            return frameworks[0];
        }

        throw new InvalidOperationException(
            $"Project '{targetProject.Name}' targets multiple frameworks ({string.Join(", ", frameworks)}). Use --target-framework to select one.");
    }
}
