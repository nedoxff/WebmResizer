namespace WebmResizer.ResizeTypes;

public class ReduceToOnePixel: ResizeType
{
    private decimal OffsetX => Size.Item1 / (decimal)(MaxCount - 1);
    private decimal OffsetY => Size.Item2 / (decimal)(MaxCount - 1);
    
    public override void ProcessVideo(string video, int count)
    {
        var currentX = (int)Math.Ceiling(Size.Item1 - OffsetX * count);
        currentX = currentX == 0 ? 2: currentX % 2 != 0 ? currentX + 1 : currentX;
        var currentY = (int)Math.Ceiling(Size.Item2 - OffsetY * count);
        currentY = currentY == 0 ? 2: currentY % 2 != 0 ? currentY + 1 : currentY;
        Ffmpeg.Run($"-i {Path.GetFileName(video)} -vf \"scale={currentX}:{currentY}\" {Path.GetFileName(video).Replace(".mp4", "_processed.mp4")}");
    }
}