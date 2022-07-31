namespace WebmResizer.ResizeTypes;

public class Random: ResizeType
{
    public override void ProcessVideo(string video, int count)
    {
        var width = count == 0 ? Size.Item1 : Shared.Random.Next(Math.Min(100, Size.Item1), Size.Item1);
        width = width % 2 != 0 ? width + 1 : width;
        var height = count == 0 ? Size.Item2 : Shared.Random.Next(Math.Min(100, Size.Item2), Size.Item2);
        height = height % 2 != 0 ? height + 1 : height;
        Ffmpeg.Run($"-i {Path.GetFileName(video)} -vf \"scale={width}:{height}\" {Path.GetFileName(video).Replace(".mp4", "_processed.mp4")}");
    }
}