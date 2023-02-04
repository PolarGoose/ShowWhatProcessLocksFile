using System.Reflection;

namespace ShowWhatProcessLocksFile.Utils;

internal static class AssemblyInfo
{
    public static string InformationalVersion =>
        Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

    public static string ProgramName => Assembly.GetExecutingAssembly().GetName().Name;
}
