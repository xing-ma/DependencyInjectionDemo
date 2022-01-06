using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EasyContainer
{
    public class EasyContainer : IServiceProvider, IDisposable 
    {
        internal readonly EasyContainer _root;
        internal readonly ConcurrentDictionary<Type, ServiceRegistry> _registers;
        private readonly ConcurrentDictionary<Key, object> _services;
        private readonly ConcurrentBag<IDisposable> _disposables;
        private volatile bool _disposed;

        public EasyContainer()
        {
            _registers = new ConcurrentDictionary<Type, ServiceRegistry>();
            _root = this;
            _services = new ConcurrentDictionary<Key, object>();
            _disposables = new ConcurrentBag<IDisposable>();
        }

        internal EasyContainer (EasyContainer parent)
        {
            _root = parent._root;
            _registers = _root._registers;
            _services = new ConcurrentDictionary<Key, object>();
            _disposables = new ConcurrentBag<IDisposable>();
        }

        public EasyContainer Register(ServiceRegistry registry)
        {
            EnsureNotDisposed();
            
            if(_registers.TryGetValue(registry.ServiceType, out var existing))
            {
                _registers[registry.ServiceType] = registry;
                registry.Next = existing;
            }
            {
                _registers[registry.ServiceType] = registry;
            }

            return this;
        }

        private object GetServiceCore(ServiceRegistry registry,
            Type[] genericArguments)
        {
            var key = new Key(registry, genericArguments);
            var serviceType = registry.ServiceType;

            switch (registry.Lifetime)
            {
                case Lifetime.Root:
                    return GetOrCreate(_root._services, _root._disposables);
                case Lifetime.Self:
                    return GetOrCreate(_services, _disposables);
                default:
                    var service = registry.Factory(this, genericArguments);

                    if(service is IDisposable disposable  && !Equals(disposable, this))
                    {
                        _disposables.Add(disposable);
                    }

                    return service;
            }

            object GetOrCreate(ConcurrentDictionary<Key, object> services,
            ConcurrentBag<IDisposable> disposables)
            {
                if (services.TryGetValue(key, out var service))
                {
                    return service;
                }

                service = registry.Factory(this, genericArguments);

                services[key] = service;

                if (service is IDisposable disposable)
                {
                    disposables.Add(disposable);
                }

                return service;
            }
        }

        public void Dispose()
        {
            _disposed = true;

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
            _services.Clear();
        }

        public object GetService(Type serviceType)
        {
            EnsureNotDisposed();
            
            if(serviceType == null || serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            ServiceRegistry registry;

            switch (serviceType.IsGenericType)
            {
                case true when serviceType.GetGenericTypeDefinition()==
                               typeof(IEnumerable<>):
                {
                    var elementType = serviceType.GetGenericArguments()[0];

                    if(!_registers.TryGetValue(elementType, out registry))
                    {
                        return Array.CreateInstance(elementType, 0);
                    }

                    var registrys = registry.AsEnumerable();

                    var services = registrys.Select(it =>
                        GetServiceCore(it, Type.EmptyTypes)).ToArray();

                    var array = Array.CreateInstance(elementType, services.Length);
                    services.CopyTo(array, 0);
                    return array;
                }
                case true when !_registers.ContainsKey(serviceType):
                {
                    var definition = serviceType.GetGenericTypeDefinition();

                    return _registers.TryGetValue(definition, out registry)
                        ? GetServiceCore(registry, serviceType.GetGenericArguments())
                        : null;
                }
                default:
                    return _registers.TryGetValue(serviceType, out registry)
                        ? GetServiceCore(registry, new Type[0])
                        : null;
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("EasyContainer");
            }
        }
    }
}
