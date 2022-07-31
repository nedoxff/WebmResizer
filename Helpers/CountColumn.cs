using Spectre.Console;
using Spectre.Console.Rendering;

namespace WebmResizer.Helpers;

public class CountColumn: ProgressColumn
{
    public override IRenderable Render(RenderContext context, ProgressTask task, TimeSpan deltaTime)
    {
        return new Markup($"{task.Value}[grey]/[/]{task.MaxValue}");
    }
}