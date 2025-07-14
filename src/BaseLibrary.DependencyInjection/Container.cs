namespace BaseLibrary.DependencyInjection;

public class Container : IContainer
{
    private readonly Dictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();
    private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();
    private readonly Dictionary<Type, Lazy<object>> _lazySingletons = new Dictionary<Type, Lazy<object>>();

    // Método para registrar uma implementação para uma interface
    public void Register<TService, TImplementation>() where TImplementation : TService, new()
    {
        _registrations[typeof(TService)] = () => new TImplementation();
    }

    public void RegisterSingleton<TService>(TService instance)
    {
        _singletonInstances[typeof(TService)] = instance;
    }
    public void RegisterLazySingleton<TService, TImplementation>() where TImplementation : TService, new()
    {
        _lazySingletons[typeof(TService)] = new Lazy<object>(() => new TImplementation());
    }
    public TService? Resolve<TService>()
    {
        return (TService?)Resolve(typeof(TService));
    }

    private object? Resolve(Type serviceType)
    {
        if (_singletonInstances.TryGetValue(serviceType, out var singletonInstance))
        {
            return singletonInstance;
        }

        if (_lazySingletons.TryGetValue(serviceType, out var lazySingleton))
        {
            return lazySingleton.Value;
        }

        if (_registrations.TryGetValue(serviceType, out var creator))
        {
            return creator();
        }

        //throw new InvalidOperationException($"Service of type {serviceType} is not registered.");
        return null;
    }
}

