using System;

namespace EasyContainer
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MapToAttribute : Attribute
    {
        public Type ServiceType { get; set; }
        public Lifetime Lifetime  { get; set; }

        public MapToAttribute(Type serviceType, Lifetime lifetime)
        {
            Lifetime = lifetime;
            ServiceType = serviceType;
        }
    }
}
