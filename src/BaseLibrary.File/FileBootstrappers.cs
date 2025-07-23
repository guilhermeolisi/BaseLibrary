using BaseLibrary;
using BaseLibrary.DependencyInjection;


public class FileBootstrappers
{
    public FileBootstrappers()
    {
        ServiceLocator();
        BusinessLocator();
        ViewModelLocator();
    }
    private void ServiceLocator()
    {
        IContainer container;

        container = Locator.ConstanteContainer;

        container.RegisterSingleton<IFileServicesText>(new FileServicesText());
        container.RegisterSingleton<IFileServicesCheck>(new FileServicesCheck());
        container.RegisterSingleton<IFileServicesName>(new FileServicesName());
        container.RegisterSingleton<IFileServicesDirectory>(new FileServicesDirectory());
        container.RegisterSingleton<IFileServicesFile>(new FileServicesFile());
        container.RegisterSingleton<IFileServices>(new FileServices());
        container.Register<IFileTXTIO>(() => new FileTXTIO());
    }
    private void BusinessLocator()
    {

    }
    private void ViewModelLocator()
    {

    }
}
