using System;
using System.Collections.Generic;
using System.Text;

namespace EasyContainer
{
   public class Key
    {
        private readonly ServiceRegistry registry;
        private readonly Type[] genericArguments;

        public Key(ServiceRegistry registry, Type[] genericArguments)
        {
            this.registry = registry;
            this.genericArguments = genericArguments;
        }

        public bool Equals(Key other)
        {
            if (registry != other.registry) return false;

            if (genericArguments.Length != other.genericArguments.Length)
                return false;

            for (var i = 0; i < genericArguments.Length; i++)
            {
                if (genericArguments[i] != other.genericArguments[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = registry.GetHashCode();

            for (var i = 0; i < genericArguments.Length; i++)
            {
                hashCode ^= genericArguments[i].GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object obj) => obj is Key key ? Equals(key) : false;
    }
}
