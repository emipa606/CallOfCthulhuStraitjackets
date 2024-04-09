using System.Reflection;
using Verse;

namespace Cthulhu.NoCCL;

public class Cthulhu_SpecialInjector : SpecialInjector
{
    private static readonly BindingFlags[] bindingFlagCombos =
    [
        BindingFlags.Instance | BindingFlags.Public,
        BindingFlags.Static | BindingFlags.Public,
        BindingFlags.Instance | BindingFlags.NonPublic,
        BindingFlags.Static | BindingFlags.NonPublic
    ];

    private static Assembly Assembly => Assembly.GetAssembly(typeof(DetourInjector));

    public override bool Inject()
    {
        var types = Assembly.GetTypes();
        foreach (var type in types)
        {
            var array = bindingFlagCombos;
            foreach (var bindingFlags in array)
            {
                var methods = type.GetMethods(bindingFlags);
                foreach (var methodInfo in methods)
                {
                    var customAttributes = methodInfo.GetCustomAttributes(typeof(DetourAttribute), true);
                    foreach (var attribute in customAttributes)
                    {
                        var detourAttribute = (DetourAttribute)attribute;
                        var bindingAttr = detourAttribute.bindingFlags != BindingFlags.Default
                            ? detourAttribute.bindingFlags
                            : bindingFlags;
                        var method = detourAttribute.source.GetMethod(methodInfo.Name, bindingAttr);
                        if (method == null)
                        {
                            Log.Error(string.Format(
                                "Cthulhu :: Detours :: Can't find source method '{0} with bindingflags {1}",
                                methodInfo.Name, bindingAttr));
                            return false;
                        }

                        if (!Detours.TryDetourFromTo(method, methodInfo))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }
}