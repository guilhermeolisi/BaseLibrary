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

        container = Locator.ConstantContainer;
        // Primeiro-vence atomico (E318): o bootstrap roda quantas vezes e de quantas threads quiserem, e o servico
        // registrado antes (inclusive um mock de teste) fica.

        container.TryRegisterSingleton<IFileServicesText>(() => new FileServicesText());
        container.TryRegisterSingleton<IFileServicesCheck>(() => new FileServicesCheck());
        container.TryRegisterSingleton<IFileServicesName>(() => new FileServicesName());
        container.TryRegisterSingleton<IFileServicesDirectory>(() => new FileServicesDirectory());
        container.TryRegisterSingleton<IFileServicesFile>(() => new FileServicesFile());
        container.TryRegisterSingleton<IFileServices>(() => new FileServices());
        container.TryRegister<IFileTXTIO>(() => new FileTXTIO());
    }
    private void BusinessLocator()
    {

    }
    private void ViewModelLocator()
    {

    }
}
