namespace BaseLibrary.DependencyInjection;

public static class Locator
{
    private readonly static IContainer _constantContainer = new Container();
    private static IContainer? _mutableContainer = new Container();
    public static IContainer? MutableContainer
    {
        get => _mutableContainer;
    }
    public static IContainer ConstanteContainer
    {
        get => _constantContainer;
    }

    public static void SetMutableContainer(IContainer container)
    {
        if (_mutableContainer is null)
            throw new ArgumentNullException(nameof(container));
        _mutableContainer = container;
    }
}
