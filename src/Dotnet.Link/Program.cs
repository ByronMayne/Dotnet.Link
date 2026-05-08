using Mayne.Dotnet.Link.Commands;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace Dotnet.Link
{
	internal class Program
	{
		static async Task<int> Main(string[] args)
			=> await new CommandLineBuilder(new AppCommand())
				.UseDefaults()
				.Build()
				.InvokeAsync(args);
	}
}
