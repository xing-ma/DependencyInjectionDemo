using System;
using System.Reflection;

namespace EasyContainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = new EasyContainer()
                .Register<IFoo, Foo>(Lifetime.Transient)
                .Register<IBar>(_ => new Bar(), Lifetime.Self)
                .Register<IBaz, Baz>(Lifetime.Root)
                .Register(Assembly.GetEntryAssembly());

            var container1 = root.CreateChild();
            var container2 = root.CreateChild();

            void GetService<TService>(EasyContainer container)
            {
                container.GetService<TService>();
                container.GetService<TService>();
            }

            GetService<IFoo>(container1);
            GetService<IBar>(container1);
            GetService<IBaz>(container1);
            GetService<IQux>(container1);
            Console.WriteLine();
            GetService<IFoo>(container2);
            GetService<IBar>(container2);
            GetService<IBaz>(container2);
            GetService<IQux>(container2);

            Console.ReadKey();
        }
    }
}
