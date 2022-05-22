using System;
using System.Collections.Generic;
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
            Log.Message(AssemblyName + " injected.");
        }
        else
        {
            Log.Error(AssemblyName + " failed to get injected properly.");
        }
    }
}

public class SpecialInjector
{
    public virtual bool Inject()
    {
        Log.Error("This should never be called.");
        return false;
    }
}

public static class Detours
{
    private static readonly List<string> detoured = new List<string>();

    private static readonly List<string> destinations = new List<string>();

    public static unsafe bool TryDetourFromTo(MethodInfo source, MethodInfo destination)
    {
        bool result;
        if (source == null)
        {
            Log.Error("Source MethodInfo is null: Detours");
            result = false;
        }
        else
        {
            if (destination == null)
            {
                Log.Error("Destination MethodInfo is null: Detours");
                result = false;
            }
            else
            {
                var item = string.Concat(source.DeclaringType?.FullName, ".", source.Name, " @ 0x",
                    source.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2)));
                var item2 = string.Concat(destination.DeclaringType?.FullName, ".", destination.Name, " @ 0x",
                    destination.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2)));
                detoured.Add(item);
                destinations.Add(item2);
                if (IntPtr.Size == 8)
                {
                    var num = source.MethodHandle.GetFunctionPointer().ToInt64();
                    var num2 = destination.MethodHandle.GetFunctionPointer().ToInt64();
                    var ptr = (byte*)num;
                    var ptr2 = (long*)(ptr + 2);
                    *ptr = 72;
                    if (ptr != null)
                    {
                        ptr[1] = 184;
                        *ptr2 = num2;
                        ptr[10] = 255;
                        ptr[11] = 224;
                    }
                }
                else
                {
                    var num3 = source.MethodHandle.GetFunctionPointer().ToInt32();
                    var num4 = destination.MethodHandle.GetFunctionPointer().ToInt32();
                    var ptr3 = (byte*)num3;
                    var ptr4 = (int*)(ptr3 + 1);
                    var num5 = num4 - num3 - 5;
                    *ptr3 = 233;
                    *ptr4 = num5;
                }

                result = true;
            }
        }

        return result;
    }
}

public class Cthulhu_SpecialInjector : SpecialInjector
{
    private static readonly BindingFlags[] bindingFlagCombos =
    {
        BindingFlags.Instance | BindingFlags.Public,
        BindingFlags.Static | BindingFlags.Public,
        BindingFlags.Instance | BindingFlags.NonPublic,
        BindingFlags.Static | BindingFlags.NonPublic
    };

    private static Assembly Assembly => Assembly.GetAssembly(typeof(DetourInjector));

    public override bool Inject()
    {
        var types = Assembly.GetTypes();
        bool result;
        for (var i = 0; i < types.Length; i++)
        {
            var type = types[i];
            var array = bindingFlagCombos;
            for (var j = 0; j < array.Length; j++)
            {
                var bindingFlags = array[j];
                var methods = type.GetMethods(bindingFlags);
                for (var k = 0; k < methods.Length; k++)
                {
                    var methodInfo = methods[k];
                    var customAttributes = methodInfo.GetCustomAttributes(typeof(DetourAttribute), true);
                    for (var l = 0; l < customAttributes.Length; l++)
                    {
                        var detourAttribute = (DetourAttribute)customAttributes[l];
                        var bindingFlags2 = detourAttribute.bindingFlags != BindingFlags.Default
                            ? detourAttribute.bindingFlags
                            : bindingFlags;
                        var method = detourAttribute.source.GetMethod(methodInfo.Name, bindingFlags2);
                        if (method == null)
                        {
                            Log.Error(string.Format(
                                "Cthulhu :: Detours :: Can't find source method '{0} with bindingflags {1}",
                                methodInfo.Name, bindingFlags2));
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