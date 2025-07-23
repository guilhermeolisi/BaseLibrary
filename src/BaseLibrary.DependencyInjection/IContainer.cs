
namespace BaseLibrary.DependencyInjection;

public interface IContainer
{
    /// <summary>
    /// Registers a service type with its corresponding implementation type.
    /// </summary>
    /// <remarks>This method maps the specified service type to a factory that creates instances of the
    /// implementation type.  The implementation type must have a parameterless constructor.</remarks>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use for the service. Must implement <typeparamref name="TService"/> and have a
    /// parameterless constructor.</typeparam>
    void Register<TService, TImplementation>() where TImplementation : TService, new();
    /// <summary>
    /// Registers a service with a specified creation function.
    /// </summary>
    /// <remarks>The service type <typeparamref name="TService"/> will be associated with the provided
    /// creation function. Subsequent calls to resolve the service will invoke this function to obtain an
    /// instance.</remarks>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="creator">A function that creates an instance of the service. Cannot be null.</param>
    void Register<TService>(Func<TService> creator);
    /// <summary>
    /// Registers a singleton service of type <typeparamref name="TService"/> with lazy initialization.
    /// </summary>
    /// <remarks>The service is created lazily, meaning the <paramref name="creator"/> function is invoked
    /// only when the service is first requested. Subsequent requests will return the same instance.</remarks>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="creator">A function that creates an instance of <typeparamref name="TService"/>. This function is called only once when
    /// the service is first requested.</param>
    void RegisterLazySingleton<TService>(Func<TService> creator);
    /// <summary>
    /// Registers a service as a lazy singleton, ensuring that a single instance of the specified implementation is
    /// created and shared across all requests for the service.
    /// </summary>
    /// <remarks>The instance of <typeparamref name="TImplementation"/> is created only when it is first
    /// requested, and the same instance is returned for all subsequent requests.</remarks>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to instantiate. Must be a subclass of <typeparamref name="TService"/> and have a
    /// parameterless constructor.</typeparam>
    void RegisterLazySingleton<TService, TImplementation>() where TImplementation : TService, new();

    /// <summary>
    /// Registers a singleton instance of the specified service type.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="instance">The instance of the service to register as a singleton. Cannot be <see langword="null"/>.</param>
    void RegisterSingleton<TService>(object instance);
    void RegisterSingleton<TService>(Func<TService> creator);
    TService RegisterSingletonAndReturn<TService>(Func<TService> creator);

    /// <summary>
    /// Resolves an instance of the specified service type.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <returns>An instance of <typeparamref name="TService"/> if the service is registered; otherwise, <see langword="null"/>.</returns>
    TService? Resolve<TService>();
}