using System;

namespace EasyContainer
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class InjectionAttribute :Attribute
    {
    }
}
