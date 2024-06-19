namespace BaseLibrary.DependencyInjection;

public class Container : IContainer
{
    private readonly Dictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();
    private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();

    // Método para registrar uma implementação para uma interface
    public void Register<TService, TImplementation>() where TImplementation : TService, new()
    {
        _registrations[typeof(TService)] = () => new TImplementation();
    }
    //public void Register<TService, TImplementation>() where TImplementation : TService
    //{
    //    _registrations[typeof(TService)] = () => Activator<TImplementation>();
    //}

    public void RegisterSingleton<TService>(TService instance)
    {
        _singletonInstances[typeof(TService)] = instance;
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

        if (_registrations.TryGetValue(serviceType, out var creator))
        {
            return creator();
        }

        //throw new InvalidOperationException($"Service of type {serviceType} is not registered.");
        return null;
    }

    //private TService Activator<TService>()
    //{
    //    // O código gerado automaticamente será usado aqui
    //    // Exemplo para Service e Client
    //    //if (typeof(TService) == typeof(Service))
    //    //{
    //    //    return (TService)(object)new Service();
    //    //}
    //    //else if (typeof(TService) == typeof(Client))
    //    //{
    //    //    var service = Resolve<IService>();
    //    //    return (TService)(object)new Client(service);
    //    //}

    //    //throw new InvalidOperationException($"No activation logic defined for {typeof(TService)}.");
    //}
}

