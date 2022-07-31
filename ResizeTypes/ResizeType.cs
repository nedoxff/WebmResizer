namespace WebmResizer.ResizeTypes;

public abstract class ResizeType
{
    public int MaxCount { get; set; }
    public (int, int) Size { get; set; }
    public abstract void ProcessVideo(string video, int count);
}