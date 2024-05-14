using VContainer;

namespace Provider
{
    public static class ObjectResolverExtensions
    {
        public static RegistrationBuilder RegisterPlainEntryPoint<T>(this IContainerBuilder builder,
            Lifetime lifetime = Lifetime.Singleton)
        {
            builder.RegisterBuildCallback(objectResolver => objectResolver.Resolve<T>());
            return builder.Register<T>(lifetime);
        }
    }
}