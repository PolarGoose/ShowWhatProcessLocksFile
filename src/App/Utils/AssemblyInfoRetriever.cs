using System.Reflection;

namespace ShowWhatProcessLocksFile.Utils;

internal static class AssemblyInfo
{
    public static string InformationalVersion =>
        typeof(AssemblyInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    public static string ProgramName => typeof(AssemblyInfo).Assembly.GetName().Name!;
}
