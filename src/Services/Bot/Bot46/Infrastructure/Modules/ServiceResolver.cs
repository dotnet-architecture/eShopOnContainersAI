using Autofac;

namespace Bot46.API.Infrastructure.Modules
{
    public static class ServiceResolver
    {
        public static IContainer Container;

        public static T Get<T>()
        {
            using (var scope = Container.BeginLifetimeScope())
                return scope.Resolve<T>();
        }
    }
}