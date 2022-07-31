using WebmResizer.Helpers;
using WebmResizer.ResizeTypes;
using Random = System.Random;

namespace WebmResizer;

public class Shared
{
    public static string Input;
    public static string Output;
    public static int SplitEvery;
    public static ResizeType Type;
    public static Random Random => new(Guid.NewGuid().GetHashCode());

    public static void CheckDirectories()
    {
        if(Directory.Exists("Temp"))
            DeleteTemp();
        Directory.CreateDirectory("Temp");
    }

    public static void DeleteTemp()
    {
        if (!Directory.Exists("Temp")) return;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        foreach (var file in Directory.GetFiles("Temp"))
            File.Delete(file);
        Directory.Delete("Temp");
    }
}