using System.Linq;
using System.Reflection;
using Verse;

namespace Cthulhu.NoCCL;

[StaticConstructorOnStartup]
internal static class DetourInjector
{
    static DetourInjector()
    {
        LongEventHandler.QueueLongEvent(Inject, "Initializing", true, null);
    }

    private static Assembly Assembly => Assembly.GetAssembly(typeof(DetourInjector));

    private static string AssemblyName => Assembly.FullName.Split(',').First();

    private static void Inject()
    {
        var cthulhu_SpecialInjector = new Cthulhu_SpecialInjector();
        if (cthulhu_SpecialInjector.Inject())
        {
            Log.Message($"{AssemblyName} injected.");
        }
        else
        {
            Log.Error($"{AssemblyName} failed to get injected properly.");
        }
    }
}