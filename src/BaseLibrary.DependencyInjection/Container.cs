namespace BaseLibrary.DependencyInjection;

public class Container : IContainer
{
    private readonly Dictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();
    private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();
    private readonly Dictionary<Type, Lazy<object>> _lazySingletons = new Dictionary<Type, Lazy<object>>();


    public void Register<TService, TImplementation>() where TImplementation : TService, new()
    {
        //Verifica se já existe um registro para o tipo TService
        if (_registrations.ContainsKey(typeof(TService)))
        {
            throw new InvalidOperationException($"Service of type {typeof(TService)} is already registered in Register.");
        }
        _registrations[typeof(TService)] = () => new TImplementation();
    }

    public void Register<TService>(Func<TService> creator)
    {
        // Verifica se já existe um registro para o tipo TService
        if (_registrations.ContainsKey(typeof(TService)))
        {
            throw new InvalidOperationException($"Service of type {typeof(TService)} is already registered in Register.");
        }
        _registrations[typeof(TService)] = () => creator();
    }

    public void RegisterSingleton<TService>(object instance)
    {
        // verifica se instance é do tipo TService
        if (instance is not TService)
        {
            throw new ArgumentException($"Instance must be of type {typeof(TService)}.", nameof(instance));
        }
        // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
        if (_registrations.ContainsKey(typeof(TService)))
        {
            return;
        }
        _singletonInstances[typeof(TService)] = instance;
    }
    public void RegisterSingleton<TService>(Func<TService> creator)
    {
        // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
        if (_registrations.ContainsKey(typeof(TService)))
        {
            return;
        }
        _singletonInstances[typeof(TService)] = creator();
    }
    public TService RegisterSingletonAndReturn<TService>(Func<TService> creator)
    {
        // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
        TService? service = (TService?)_singletonInstances[typeof(TService)];
        if (service is not null)
        {
            return service;
        }
        service = creator();
        _singletonInstances[typeof(TService)] = service;
        return service;
    }
    public void RegisterLazySingleton<TService, TImplementation>() where TImplementation : TService, new()
    {
        // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
        if (_registrations.ContainsKey(typeof(TService)))
        {
            return;
        }
        _lazySingletons[typeof(TService)] = new Lazy<object>(() => new TImplementation());
    }
    public void RegisterLazySingleton<TService>(Func<TService> creator)
    {
        // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
        if (_registrations.ContainsKey(typeof(TService)))
        {
            return;
        }
        _lazySingletons[typeof(TService)] = new Lazy<object>(() => creator());
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

