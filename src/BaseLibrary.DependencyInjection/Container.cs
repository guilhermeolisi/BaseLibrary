using System.Collections.Concurrent;

namespace BaseLibrary.DependencyInjection;

/// <summary>
/// Conteiner de DI seguro entre threads (E318): leitura sem trava pelos mapas concorrentes, e todo registro sob uma
/// trava so, porque o confere-e-escreve cruza mais de um mapa. A semantica de cada metodo de registro e a de antes; os
/// <c>TryRegister*</c> sao o primeiro-vence atomico que os bootstrappers usam para poder rodar mais de uma vez.
/// <para>Nenhum criador roda dentro da trava: o criador de um servico pode resolver ou registrar outro, e esperar outra
/// thread com a trava na mao inverteria a ordem das travas contra a do <see cref="Lazy{T}"/> (revisao do plano, achado 6).</para>
/// </summary>
public class Container : IContainer
{
    private readonly object gate = new();
    private readonly ConcurrentDictionary<Type, Func<object>> _registrations = new();
    private readonly ConcurrentDictionary<Type, object> _singletonInstances = new();
    private readonly ConcurrentDictionary<Type, Lazy<object>> _lazySingletons = new();
    // Registros nomeados (contract): permite múltiplas instâncias do mesmo TService diferenciadas por string,
    // equivalente ao contract do Splat (ex.: IThemeCollectionProvider "theme" e "transparency").
    private readonly ConcurrentDictionary<(Type, string), object> _keyedInstances = new();


    public void Register<TService, TImplementation>() where TImplementation : TService, new()
    {
        lock (gate)
        {
            //Verifica se já existe um registro para o tipo TService
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new InvalidOperationException($"Service of type {typeof(TService)} is already registered in Register.");
            }
            _registrations[typeof(TService)] = () => new TImplementation();
        }
    }

    public void Register<TService>(Func<TService> creator)
    {
        lock (gate)
        {
            // Verifica se já existe um registro para o tipo TService
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new InvalidOperationException($"Service of type {typeof(TService)} is already registered in Register.");
            }
            _registrations[typeof(TService)] = () => creator()!;
        }
    }

    public void RegisterSingleton<TService>(object instance)
    {
        // verifica se instance é do tipo TService
        if (instance is not TService)
        {
            throw new ArgumentException($"Instance must be of type {typeof(TService)}.", nameof(instance));
        }
        lock (gate)
        {
            // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
            if (_registrations.ContainsKey(typeof(TService)))
            {
                return;
            }
            _singletonInstances[typeof(TService)] = instance;
        }
    }
    public void RegisterSingleton<TService>(Func<TService> creator)
    {
        if (_registrations.ContainsKey(typeof(TService)))
        {
            return;
        }
        object instance = creator()!;
        lock (gate)
        {
            // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
            if (_registrations.ContainsKey(typeof(TService)))
            {
                return;
            }
            _singletonInstances[typeof(TService)] = instance;
        }
    }
    public TService RegisterSingletonAndReturn<TService>(Func<TService> creator)
    {
        // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
        if (_singletonInstances.TryGetValue(typeof(TService), out var existing) && existing is TService registered)
        {
            return registered;
        }
        TService service = creator();
        lock (gate)
        {
            // Outra thread pode ter registrado enquanto este criador rodava: vale a primeira, como no confere de cima.
            if (_singletonInstances.TryGetValue(typeof(TService), out existing) && existing is TService first)
            {
                return first;
            }
            _singletonInstances[typeof(TService)] = service!;
        }
        return service;
    }
    public void RegisterLazySingleton<TService, TImplementation>() where TImplementation : TService, new()
    {
        lock (gate)
        {
            // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
            if (_registrations.ContainsKey(typeof(TService)))
            {
                return;
            }
            _lazySingletons[typeof(TService)] = new Lazy<object>(() => new TImplementation()!);
        }
    }
    public void RegisterLazySingleton<TService>(Func<TService> creator)
    {
        lock (gate)
        {
            // Verifica se já existe um registro para o tipo TService, se já existir, não deve registrar novamente
            if (_registrations.ContainsKey(typeof(TService)))
            {
                return;
            }
            _lazySingletons[typeof(TService)] = new Lazy<object>(() => creator()!);
        }
    }

    public bool TryRegisterSingleton<TService>(Func<TService> creator)
    {
        ArgumentNullException.ThrowIfNull(creator);
        Lazy<object> lazy;
        lock (gate)
        {
            if (IsRegisteredUnderGate(typeof(TService)))
            {
                return false;
            }
            lazy = new Lazy<object>(() => creator()!);
            _lazySingletons[typeof(TService)] = lazy;
        }
        // Fora da trava: o criador roda uma vez so (modo padrao do Lazy), e a instancia nasce no registro, como num
        // RegisterSingleton com a instancia pronta.
        _ = lazy.Value;
        return true;
    }

    public bool TryRegisterLazySingleton<TService>(Func<TService> creator)
    {
        ArgumentNullException.ThrowIfNull(creator);
        lock (gate)
        {
            if (IsRegisteredUnderGate(typeof(TService)))
            {
                return false;
            }
            _lazySingletons[typeof(TService)] = new Lazy<object>(() => creator()!);
            return true;
        }
    }

    public bool TryRegister<TService>(Func<TService> creator)
    {
        ArgumentNullException.ThrowIfNull(creator);
        lock (gate)
        {
            if (IsRegisteredUnderGate(typeof(TService)))
            {
                return false;
            }
            _registrations[typeof(TService)] = () => creator()!;
            return true;
        }
    }

    private bool IsRegisteredUnderGate(Type serviceType)
        => _registrations.ContainsKey(serviceType)
            || _singletonInstances.ContainsKey(serviceType)
            || _lazySingletons.ContainsKey(serviceType);

    public void RegisterConstant<TService>(TService instance)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance));
        }
        lock (gate)
        {
            // Last-wins: sobrescreve qualquer registro anterior (mesma semântica do RegisterConstant do Splat).
            _singletonInstances[typeof(TService)] = instance;
        }
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
        lock (gate)
        {
            _keyedInstances[(typeof(TService), contract)] = instance;
        }
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
