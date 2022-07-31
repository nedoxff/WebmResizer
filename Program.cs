using Serilog;
using Spectre.Cli;
using WebmResizer.Commands;

File.WriteAllText("log.txt", "");
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.txt")
    .MinimumLevel.Debug()
    .CreateLogger();

var app = new CommandApp<RunInteractiveCommand>();
app.Configure(c =>
{
    c.AddCommand<RunCommand>("run");
    c.AddCommand<ListTypesCommand>("list");
});
app.Run(args);

/*using System.Diagnostics;
using Spectre.Cli;
using Spectre.Console;
using TinyDialogsNet;

var app = new CommandApp();

var file = Dialogs.OpenFileDialog("Select a file to modify..", "", new []{"*.mp4", "*.webm", "*.mov"}, "Videos").First();
var saveTo = Dialogs.SaveFileDialog("Select a location to save the file to..", "", "*.webm", ".webm video");

AnsiConsole.Clear();
var splitEveryDictionary = new Dictionary<string, int>
{
    {"100 milliseconds", 100},
    {"250 milliseconds", 250},
    {"0.5 seconds", 500},
    {"1 second", 1000},
    {"2 seconds", 2000}
};

var prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
    .AddChoices(splitEveryDictionary.Keys.Concat(new[] {"Other.."}))
    .Title("Split every.."));

var splitEvery = !splitEveryDictionary.ContainsKey(prompt) ? AnsiConsole.Ask<int>("Please, input the delay between chunks of the video (in milliseconds):") : splitEveryDictionary[prompt];;

AnsiConsole.Clear();
var type = AnsiConsole.Prompt(new SelectionPrompt<ResizeType>()
    .AddChoices((ResizeType[])typeof(ResizeType).GetEnumValues())
    .Title("Please, select the type of resizing:"));

AnsiConsole.Clear();
AnsiConsole.MarkupLine("Doing some checks..");

if (!Directory.Exists("Temp"))
    Directory.CreateDirectory("Temp");
else
{
    Directory.Delete("Temp", true);
    Directory.CreateDirectory("Temp");
}

var extension = Path.GetExtension(file);
var input = Path.Join(Environment.CurrentDirectory, "Temp", "input" + extension);
File.Copy(file, input);

if (extension != ".mp4")
{
    AnsiConsole.MarkupLine("Converting the video to .mp4..");
    Run($"-i input{extension} input.mp4");
    File.Delete(input);
}

AnsiConsole.WriteLine("Splitting the video..");
var duration = GetDuration();
var (width, height) = GetSize();
var offset = 0;
var count = 0;
while(duration > 0)
{
    //AnsiConsole.Clear();
    Run($"-i input.mp4 -ss {ToFfmpegString(offset)} -t {(splitEvery > duration ? ToFfmpegString(duration) : ToFfmpegString(splitEvery))} {count}.mp4");
    offset += splitEvery;
    duration -= splitEvery > duration ? duration: splitEvery;
    count++;
}

var files = Directory.GetFiles(Path.Join(Environment.CurrentDirectory, "Temp"))
    .Where(x => !x.Contains("input.mp4")).Select(Path.GetFileName).ToList();

/*foreach(var i in files)
    File.Delete(i!);#1#

//files = files.Select(x => x.Replace(".mp4", ".webm")).ToList();
foreach (var i in files)
{
    //AnsiConsole.Clear();
    switch (type)
    {
        case ResizeType.Random:
        {
            var w = NewRandom().Next(Math.Min(100, width), width);
            var h = NewRandom().Next(Math.Min(100, height), height);
            w = w % 2 != 0 ? w + 1 : w;
            h = h % 2 != 0 ? h + 1 : h;
            AnsiConsole.MarkupLine($"Resizing [yellow]{i}[/] to [green]{w}x{h}[/]..");
            Run($"-i {i} -vf \"scale={w}:{h}\" {Path.GetFileNameWithoutExtension(i)}_resized.mp4");
            break;
        }
    }
}

foreach (var i in files.Select(x => x.Replace(".mp4", "_resized.mp4")))
{
    AnsiConsole.Clear();
    AnsiConsole.MarkupLine($"Converting [yellow]{i}[/] to [green]{Path.GetFileNameWithoutExtension(i)}.webm[/]..");
    Run($"-i {i} {Path.GetFileNameWithoutExtension(i)}.webm");
}

files = Directory.GetFiles("Temp").Where(x => x.Contains("_resized.webm")).Select(x => Path.GetFileName(x)).OrderBy(x => int.Parse(x.Replace("_resized.webm", ""))).ToList()!;
File.WriteAllText(Path.Join(Environment.CurrentDirectory, "Temp", "list.txt"), files.Aggregate("", (current, i) => current + "file '" + Path.GetFullPath(Path.Join(Environment.CurrentDirectory, "Temp", i)) + "'\n"));

AnsiConsole.WriteLine("Combining .webm files..");
Run($"-f concat -safe 0 -i list.txt -c copy {saveTo}");

AnsiConsole.WriteLine("Cleaning up..");
GC.Collect();
GC.WaitForPendingFinalizers();
foreach(var i in Directory.GetFiles("Temp"))
    File.Delete(i);
Directory.Delete("Temp", true);

void Run(string arguments)
{
    AnsiConsole.MarkupLine($"Running [green]ffmpeg {arguments}[/]");
    var info = new ProcessStartInfo
    {
        FileName = "ffmpeg",
        Arguments = arguments,
        CreateNoWindow = true,
        WorkingDirectory = Path.Join(Environment.CurrentDirectory, "Temp"),
        RedirectStandardError = true,
        UseShellExecute = false
    };
    var process = new Process
    {
        StartInfo = info
    };
    process.ErrorDataReceived += (o, e) =>
    {
        if (!string.IsNullOrEmpty(e.Data))
            AnsiConsole.WriteLine(e.Data);
    };
    process.Start();
    process.BeginErrorReadLine();
    process.WaitForExit();
}





Random NewRandom() => new(Guid.NewGuid().GetHashCode());

enum ResizeType
{
    Random
}*/