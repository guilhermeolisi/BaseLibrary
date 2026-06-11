namespace BaseLibrary.DependencyInjection;

public class Container : IContainer
{
    private readonly Dictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();
    private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();
    private readonly Dictionary<Type, Lazy<object>> _lazySingletons = new Dictionary<Type, Lazy<object>>();
    // Registros nomeados (contract): permite múltiplas instâncias do mesmo TService diferenciadas por string,
    // equivalente ao contract do Splat (ex.: IThemeCollectionProvider "theme" e "transparency").
    private readonly Dictionary<(Type, string), object> _keyedInstances = new Dictionary<(Type, string), object>();


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
        if (_singletonInstances.TryGetValue(typeof(TService), out var existing) && existing is TService registered)
        {
            return registered;
        }
        TService service = creator();
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
    public void RegisterConstant<TService>(TService instance)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance));
        }
        // Last-wins: sobrescreve qualquer registro anterior (mesma semântica do RegisterConstant do Splat).
        _singletonInstances[typeof(TService)] = instance;
    }

    public void RegisterConstant<TService>(TService instance, string contract)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance));
        }
        if (string.IsNullOrEmpty(contract))
        {
            throw new ArgumentException("Contract cannot be null or empty.", nameof(contract));
        }
        _keyedInstances[(typeof(TService), contract)] = instance;
    }

    public TService? Resolve<TService>()
    {
        return (TService?)Resolve(typeof(TService));
    }

    public TService? Resolve<TService>(string contract)
    {
        if (string.IsNullOrEmpty(contract))
        {
            throw new ArgumentException("Contract cannot be null or empty.", nameof(contract));
        }
        if (_keyedInstances.TryGetValue((typeof(TService), contract), out var instance))
        {
            return (TService?)instance;
        }
        return default;
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

