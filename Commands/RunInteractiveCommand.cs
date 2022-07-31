using SixLabors.ImageSharp.Processing;
using Spectre.Cli;
using Spectre.Console;
using TinyDialogsNet;
using WebmResizer.Helpers;
using WebmResizer.ResizeTypes;
using ValidationResult = Spectre.Cli.ValidationResult;

namespace WebmResizer.Commands;

public class RunInteractiveCommand : Command
{
    private readonly Dictionary<string, int> _splitEveryDictionary = new()
    {
        { "100 milliseconds", 100 },
        { "250 milliseconds", 250 },
        { "0.5 seconds", 500 },
        { "1 second", 1000 },
        { "2 seconds", 2000 }
    };

    public override int Execute(CommandContext context)
    {
        AnsiConsole.Markup("Welcome to [green]WebmResizer[/]! This utility will help you to create those funny resizing .webm files you see on Discord and other social medias.\n[yellow]I recommend maximizing the window to get the [/][red italic]purr[/][yellow]fect user experience.[/]\n[bold]Press any key to get started..[/]");
        Console.ReadKey(true);

        AnsiConsole.Clear();
        
        Shared.Input = Dialogs.OpenFileDialog("Select a file to modify..", "", new[] { "*.webm", "*.mp4", "*.mov" },
            "Video files").First();
        Shared.Output = Dialogs.SaveFileDialog("Select the save destination..", "", "*.webm", ".webm files");

        var prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .AddChoices(_splitEveryDictionary.Keys.Concat(new[] { "Other.." }))
            .Title("Split every.."));

        Shared.SplitEvery = !_splitEveryDictionary.ContainsKey(prompt)
            ? AnsiConsole.Ask<int>("Please, input the delay between chunks of the video (in milliseconds):")
            : _splitEveryDictionary[prompt];

        var type = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .AddChoices(ResizeTypeHelper.Types.Select(x => x.Name))
            .Title("Please select the resize type.."));
        var resizeType = ResizeTypeHelper.Find(type);
        if (resizeType == null)
        {
            AnsiConsole.MarkupLine($"[red]Resize type \"{type}\" was not found![/]");
            return 1;
        }

        Shared.Type = (ResizeType)Activator.CreateInstance(resizeType)!;

        bool hasInternetConnection;
        CanvasImage? image = null;
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Line)
            .Start("Checking for internet connection..", s =>
            {
                hasInternetConnection = CatImage.CheckForInternetConnection();
                if (hasInternetConnection)
                {
                    s.Status("Getting a cat image..");
                    s.Refresh();
                    image = new CanvasImage(CatImage.GetCatImage())
                        .PixelWidth(1)
                        .MaxWidth(AnsiConsole.Profile.Width);
                    image.Mutate(x => x.Resize(image.Width, image.Width / 4));
                }
            });

        if (image != null) AnsiConsole.Write(image);
        AnsiConsole.Cursor.SetPosition(0, 0);
        AnsiConsole.Progress()
            .Columns(new SpinnerColumn(Spinner.Known.Line), new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new CountColumn(), new ElapsedTimeColumn())
            .Start(c =>
            {
                var task = c.AddTask("Checking directories").IsIndeterminate();
                Shared.CheckDirectories();
                task.Value(100).StopTask();

                if (Path.GetExtension(Shared.Input).ToLower() != ".mp4")
                {
                    task = c.AddTask("Converting the video to .mp4..").IsIndeterminate();
                    Ffmpeg.ConvertTo(".mp4");
                    task.Value(100).StopTask();
                }
                else File.Copy(Shared.Input, Path.Join(Environment.CurrentDirectory, "Temp", Path.GetFileName(Shared.Input)), true);

                task = c.AddTask("Extracting audio").IsIndeterminate();
                Ffmpeg.ExtractAudio();
                task.Value(100).StopTask();

                var segments = Ffmpeg.GetSegmentsCount();
                
                task = c.AddTask("Splitting video into segments", true, segments);
                var files = Ffmpeg.SplitInput(() => task.Increment(1));
                task.StopTask();
                
                task = c.AddTask("Processing videos", true, segments);

                Shared.Type.GetType().GetProperty("MaxCount")!.SetValue(Shared.Type, segments);
                Shared.Type.GetType().GetProperty("Size")!.SetValue(Shared.Type, Ffmpeg.GetSize());
                var method = Shared.Type.GetType().GetMethod("ProcessVideo");

                for (var index = 0; index < files.Count; index++)
                {
                    var file = files[index];
                    method!.Invoke(Shared.Type, new object?[] { file, index });
                    if (!File.Exists(Path.Join(Environment.CurrentDirectory, "Temp", index + "_processed.mp4")))
                    {
                        Dialogs.MessageBox(Dialogs.MessageBoxButtons.Ok, Dialogs.MessageBoxIconType.Error,
                            Dialogs.MessageBoxDefaultButton.OkYes, "", $"An internal error occured: a _resized.mp4 was not found! (File No.{index})");
                        Environment.Exit(1);
                    }
                    task.Increment(1);
                }
                task.StopTask();
                
                task = c.AddTask("Converting videos to .webm files", true, segments);

                files = Directory.GetFiles("Temp").Where(x => x.Contains("_processed.mp4")).ToList();
                foreach (var file in files)
                {
                    Ffmpeg.ConvertTo(".webm", Path.GetFileName(file));
                    task.Increment(1);
                }
                task.StopTask();
                
                task = c.AddTask("Combining videos").IsIndeterminate();
                Ffmpeg.CombineToOutput(Directory.GetFiles("Temp").Where(x => x.Contains(".webm")).OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x.Replace("_processed", "")))).Select(Path.GetFileName)!);
                task.Value(100).StopTask();

                task = c.AddTask("Cleaning up").IsIndeterminate();
                Shared.DeleteTemp();
                task.Value(100).StopTask();
            });
        
        AnsiConsole.Markup("Thank you for using WebmResizer! Press any key to exit..");
        Console.ReadKey(true);

        return 0;
    }
}