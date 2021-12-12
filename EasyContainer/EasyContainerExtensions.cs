using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EasyContainer
{
    public static class EasyContainerExtensions
    {
        public static EasyContainer Register(this EasyContainer container,
            Type from,Type to, Lifetime lifetime)
        {
            Func<EasyContainer, Type[], object> factory =
                (_, arguments) => Create(_, to, arguments);

            container.Register(new ServiceRegistry(from, lifetime, factory));

            return container;
        }

        public static EasyContainer Register<TFrom, TTo>(this EasyContainer container,
            Lifetime lifetime) where TTo : TFrom
        => container.Register(typeof(TFrom), typeof(TTo), lifetime);

        public static EasyContainer Register(this EasyContainer container,
            Type serviceType, object instance)
        {
            Func<EasyContainer, Type[], object> factory =
                (_, arguments) => instance;

            container.Register(new ServiceRegistry(serviceType, Lifetime.Root, factory));

            return container;
        }

        public static EasyContainer Register(this EasyContainer container,
            Type serviceType, Func<EasyContainer, object> factory, Lifetime lifetime)
        {
            container.Register(new ServiceRegistry(serviceType, lifetime,
                (_, arguments) => factory(_)));

            return container;
        }

        public static EasyContainer Register<TService>(this EasyContainer container,
            Func<EasyContainer, TService> factory, Lifetime lifetime)
        {
            container.Register(new ServiceRegistry(typeof(TService), lifetime,
                (_, arguments) => factory(_)));

            return container;
        }

        public static EasyContainer Register(this EasyContainer container, Assembly assembly)
        {
            var typeAttributes = from type in assembly.GetExportedTypes()
                                 let atttibute = type.GetCustomAttribute<MapToAttribute>()
                                 where atttibute != null
                                 select new
                                 {
                                     ServiveType = type,
                                     Attribute = atttibute
                                 };

            foreach (var typeAttribute in typeAttributes)
            {
                container.Register(typeAttribute.Attribute.ServiceType,
                    typeAttribute.ServiveType, typeAttribute.Attribute.Lifetime);
            }

            return container;
        }

        public static T GetService<T>(this EasyContainer container)
            => (T)container.GetService(typeof(T));

        public static IEnumerable<T> GetServices<T>(this EasyContainer container)
        => container.GetService<IEnumerable<T>>();

        public static EasyContainer CreateChild(this EasyContainer container)
            => new EasyContainer(container);

        private static object Create(EasyContainer container, Type type,
            Type[] genericArguments)
        {
            if(genericArguments.Length > 0)
            {
                type = type.MakeGenericType(genericArguments);
            }

            var constructors = type.GetConstructors();
            if(constructors.Length == 0)
            {
                throw new InvalidOperationException($"Cannot create the instace of {type} which  does not have a public constructor");
            }

            var constructor = constructors.FirstOrDefault(it =>
            it.GetCustomAttributes(false).OfType<InjectionAttribute>().Any());

            constructor ??= constructors.First();

            var paraments = constructor.GetParameters();

            if(paraments.Length == 0)
            {
                return Activator.CreateInstance(type);
            }

            var arguments = new object[paraments.Length];

            for (var index = 0; index < arguments.Length; index++)
            {
                arguments[index] = container.GetService(paraments[index].ParameterType);
            }

            return constructor.Invoke(arguments);
        }
    }
}
