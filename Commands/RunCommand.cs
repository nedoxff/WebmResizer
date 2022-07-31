using Serilog.Sinks.File;
using Spectre.Cli;
using Spectre.Console;
using WebmResizer.Helpers;
using WebmResizer.ResizeTypes;

namespace WebmResizer.Commands;

public class RunCommand: Command<RunCommand.RunCommandOptions>
{
    public class RunCommandOptions : CommandSettings
    {
        [CommandOption("--input|-i")]
        public string Input { get; set; }
        [CommandOption("--output|-o")]
        public string Output { get; set; }
        [CommandOption("--split_every|-s")]
        public int SplitEvery { get; set; }
        [CommandOption("--type|-t")]
        public string Type { get; set; }
    }

    public override int Execute(CommandContext context, RunCommandOptions settings)
    {
        if (string.IsNullOrEmpty(settings.Input) || string.IsNullOrEmpty(settings.Output) ||
            string.IsNullOrEmpty(settings.Type) || settings.SplitEvery == 0)
        {
            AnsiConsole.MarkupLine("[red]Please specify all properties![/]");
            return 1;
        }

        Shared.Input = settings.Input;
        Shared.Output = settings.Output;
        Shared.SplitEvery = settings.SplitEvery;
        var type = ResizeTypeHelper.Find(settings.Type);
        if (type == null)
        {
            AnsiConsole.MarkupLine($"[red]Resize type \"{settings.Type}\" was not found![/]");
            return 1;
        }

        Shared.Type = (ResizeType)Activator.CreateInstance(type)!;
        
        Shared.CheckDirectories();
        if(Path.GetExtension(Shared.Input).ToLower() != ".mp4")
            Ffmpeg.ConvertTo(".mp4");
        else File.Copy(Shared.Input, Path.Join(Environment.CurrentDirectory, "Temp", Path.GetFileName(Shared.Input)), true);
        Ffmpeg.ExtractAudio();
        var files = Ffmpeg.SplitInput();

        var segments = Ffmpeg.GetSegmentsCount();
        Shared.Type.GetType().GetProperty("MaxCount")!.SetValue(Shared.Type, segments);
        Shared.Type.GetType().GetProperty("Size")!.SetValue(Shared.Type, Ffmpeg.GetSize());
        
        var method = Shared.Type.GetType().GetMethod("ProcessVideo");

        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];
            method!.Invoke(Shared.Type, new object?[] { file, index });
            if (!File.Exists(Path.Join(Environment.CurrentDirectory, "Temp", index + "_processed.mp4")))
                return 1;
        }
        
        files = Directory.GetFiles("Temp").Where(x => x.Contains("_processed.mp4")).ToList();
        foreach (var file in files)
            Ffmpeg.ConvertTo(".webm", Path.GetFileName(file));
        
        Ffmpeg.CombineToOutput(Directory.GetFiles("Temp").Where(x => x.Contains(".webm")).OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x.Replace("_processed", "")))).Select(Path.GetFileName)!);
        Shared.DeleteTemp();
        return 0;
    }
}