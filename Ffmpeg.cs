using System.Diagnostics;
using Serilog;
using Spectre.Console;
using WebmResizer.ResizeTypes;

namespace WebmResizer;

public class Ffmpeg
{
    public static void Run(string arguments)
    {
        var info = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = "-y " + arguments,
            CreateNoWindow = true,
            WorkingDirectory = Path.Join(Environment.CurrentDirectory, "Temp"),
            RedirectStandardError = true,
            UseShellExecute = false
        };
        var process = new Process
        {
            StartInfo = info
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Log.Debug(e.Data);
        };
        process.Start();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }

    public static void ConvertTo(string extension, string input = "")
    {
        if (string.IsNullOrEmpty(input)) input = Shared.Input;
        var name = Path.GetFileNameWithoutExtension(input);
        Run($"-i \"{input}\" {name}{extension}");
    }

    public static void CombineToOutput(IEnumerable<string> files)
    {
        var path = Path.Join(Environment.CurrentDirectory, "Temp", "list.txt");
        File.WriteAllText(path, files.Aggregate("", (current, file) => current + "file '" + file + "'\n"));
        Run($"-f concat -safe 0 -i list.txt -i {Path.GetFileNameWithoutExtension(Shared.Input)}.ogg -c:v copy \"{Shared.Output}\"");
    }

    public static void ExtractAudio()
    {
        Run($"-i {Path.GetFileNameWithoutExtension(Shared.Input)}.mp4 -vn -acodec libvorbis {Path.GetFileNameWithoutExtension(Shared.Input)}.ogg");
    }

    public static List<string> SplitInput(Action? onSegmentCreated = null)
    {
        var duration = GetDuration();
        var offset = 0;
        var count = 0;
        var list = new List<string>();
        while(duration > 0)
        {
            Run($"-i {Path.GetFileNameWithoutExtension(Shared.Input)}.mp4 -ss {ToFfmpegString(offset)} -t {(Shared.SplitEvery > duration ? ToFfmpegString(duration) : ToFfmpegString(Shared.SplitEvery))} {count}.mp4");
            list.Add(Path.Join(Environment.CurrentDirectory, "Temp", count + ".mp4"));
            offset += Shared.SplitEvery;
            duration -= Shared.SplitEvery > duration ? duration: Shared.SplitEvery;
            count++;
            onSegmentCreated?.Invoke();
        }

        return list;
    }

    public static int GetSegmentsCount()
    {
        var duration = GetDuration();
        return (int)Math.Ceiling(duration / (decimal)Shared.SplitEvery);
    }

    public static int GetDuration()
    {
        var info = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-sexagesimal -v error -select_streams v:0 -show_entries stream=duration -of default=noprint_wrappers=1:nokey=1 {Path.GetFileNameWithoutExtension(Shared.Input)}.mp4",
            CreateNoWindow = true,
            WorkingDirectory = Path.Join(Environment.CurrentDirectory, "Temp"),
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        var process = new Process
        {
            StartInfo = info
        };
        var result = -1;
        process.OutputDataReceived += (o, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            if (TimeSpan.TryParse(e.Data, out var res))
                result = (int)res.TotalMilliseconds;
        };
        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
        return result;
    }

    public static (int, int) GetSize()
    {
        var info = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -select_streams v:0 -show_entries stream=width,height -of csv=s=x:p=0 {Path.GetFileNameWithoutExtension(Shared.Input)}.mp4",
            CreateNoWindow = true,
            WorkingDirectory = Path.Join(Environment.CurrentDirectory, "Temp"),
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        var process = new Process
        {
            StartInfo = info
        };
        var result = (-1, -1);
        process.OutputDataReceived += (o, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            result = (int.Parse(e.Data.Split("x")[0]), int.Parse(e.Data.Split("x")[1]));
        };
        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
        return result;
    }
    
    private static string ToFfmpegString(int ms)
    {
        var timeSpan = TimeSpan.FromMilliseconds(ms);
        return timeSpan.ToString(@"hh\:mm\:ss\.fff");
    }
}