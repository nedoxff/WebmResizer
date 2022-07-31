using System.Reflection;
using Spectre.Cli;
using Spectre.Console;
using WebmResizer.Helpers;
using WebmResizer.ResizeTypes;

namespace WebmResizer.Commands;

public class ListTypesCommand: Command
{
    public override int Execute(CommandContext context)
    {
        foreach (var type in ResizeTypeHelper.Types)
            AnsiConsole.WriteLine($"- {type.Name} ({type.Name.ToLower()})");
        return 0;
    }
}