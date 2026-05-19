namespace LoupedeckMSFSG1000;

using System.Reflection;
using Loupedeck;

internal static class PluginResources
{
    private static Assembly? _assembly;

    public static void Init(Assembly assembly) => _assembly = assembly;

    public static String[] FindFiles(String regexPattern) => GetAssembly().FindFiles(regexPattern);

    public static BitmapImage ReadImage(String resourceName)
    {
        var assembly = GetAssembly();
        return assembly.ReadImage(assembly.FindFileOrThrow(resourceName));
    }

    private static Assembly GetAssembly() => _assembly ?? throw new InvalidOperationException("Plugin resources are not initialized.");
}
