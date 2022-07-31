using System.Reflection;
using WebmResizer.ResizeTypes;

namespace WebmResizer.Helpers;

public class ResizeTypeHelper
{
    public static IEnumerable<Type> Types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Namespace == "WebmResizer.ResizeTypes" && x != typeof(ResizeType));
    public static Type? Find(string name) => Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
}