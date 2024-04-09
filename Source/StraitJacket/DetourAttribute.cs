using System;
using System.Reflection;

namespace Cthulhu;

[AttributeUsage(AttributeTargets.Method)]
internal class DetourAttribute(Type source) : Attribute
{
    public readonly Type source = source;
    public BindingFlags bindingFlags;
}