using CliWrap;
using Spectre.Console;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dotnet.Link
{
	internal class DotnetCommands
	{

		public static async Task<string?> GetPropertyAsync(
			string projectPath,
			string propertyName)
		{
			MemoryStream memoryStream = new MemoryStream();
			string arguments = $"msbuild {projectPath} --getProperty:{propertyName}";
			await Cli.Wrap("dotnet")
				.WithArguments(arguments)
				.WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
				.ExecuteAsync();

			using(StreamReader reader = new StreamReader(memoryStream))
			{
				memoryStream.Position = 0;
				string content = reader.ReadToEnd();
				return content.Trim();
			}
		}

		public static async Task<IList<ProjectItem>> GetItems(
			string projectPath,
			string itemName,
			string? target = null)
		{
			string arguments = $"msbuild";
			if (!string.IsNullOrWhiteSpace(projectPath)) arguments += $" {projectPath}";
			if (!string.IsNullOrWhiteSpace(target)) arguments += $" -t:{target}";
			if (!string.IsNullOrWhiteSpace(itemName)) arguments += $" --getItem:{itemName}";

			MemoryStream memoryStream = new MemoryStream();

			await Cli.Wrap("dotnet")
				.WithArguments(arguments)
				.WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
				.ExecuteAsync();

			AnsiConsole.MarkupLine($"[grey66] Evaluating [lightsalmon3]{itemName}[/] items from project [lightsalmon3]{Path.GetFileName(projectPath)}[/][/]");

			memoryStream.Position = 0;
			JsonObject? jsonPayload = await JsonNode.ParseAsync(memoryStream) as JsonObject;

			if (jsonPayload is not null &&
				jsonPayload["Items"] is JsonObject jItems &&
				jItems[itemName] is JsonArray jItemsArray)
			{
				List<ProjectItem>? parsedItems = JsonSerializer.Deserialize<List<ProjectItem>>(jItemsArray);
				if(parsedItems is not null)
				{

					AnsiConsole.MarkupLine($"[grey66] Found [lightsalmon3]{parsedItems.Count}[/] [dodgerblue1]NuGet[/] package inputs[/]");
					for (int i = 0; i < parsedItems.Count; i++)
					{
						ProjectItem item = parsedItems[i];
						AnsiConsole.MarkupLine($"[lightskyblue3]  [darkseagreen]{i+1}.[/] {item.Filename}{item.Extension}[/]");
					}

					return parsedItems;
				}
			}

			return Array.Empty<ProjectItem>();
		}
	}
}
