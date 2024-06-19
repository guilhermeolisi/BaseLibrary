namespace BaseLibrary.DependencyInjection;

public interface IContainer
{
    void Register<TService, TImplementation>() where TImplementation : TService, new();
    void RegisterSingleton<TService>(TService instance);
    TService? Resolve<TService>();
}