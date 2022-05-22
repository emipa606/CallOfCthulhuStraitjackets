using System;
using System.Reflection;

namespace Cthulhu;

[AttributeUsage(AttributeTargets.Method)]
internal class DetourAttribute : Attribute
{
    public BindingFlags bindingFlags;
    public Type source;

    public DetourAttribute(Type source)
    {
        this.source = source;
    }
}