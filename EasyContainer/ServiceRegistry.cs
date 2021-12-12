using System;
using System.Collections.Generic;
using System.Text;

namespace EasyContainer
{
    public class ServiceRegistry
    {
        
        public ServiceRegistry(Type serviceType, Lifetime lifetime,
            Func<EasyContainer, Type[], object> factory)
        {
            ServiceType = serviceType;
            Lifetime = lifetime;
            Factory = factory;
        }

        public Type ServiceType { get; }
        public Lifetime Lifetime { get; }
        public Func<EasyContainer, Type[], object> Factory { get; }
        internal ServiceRegistry Next { get; set; }
        internal IEnumerable<ServiceRegistry> AsEnumerable()
        {
            var list = new List<ServiceRegistry>();

            for (var self = this;  self != null; self = self.Next)
            {
                list.Add(self);
            }

            return list;
        }
    }
}
