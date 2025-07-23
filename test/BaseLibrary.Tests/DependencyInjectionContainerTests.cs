using BaseLibrary.DependencyInjection;
using BaseLibrary.Numbers;
using FluentAssertions;
using System.Globalization;

namespace BaseLibrary.Tests;

public class DependencyInjectionContainerTests
{
    public DependencyInjectionContainerTests()
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
    // Testa o método Register com um tipo de classe com new() de um container de injeção de dependência
    [Fact]
    public void Register_WhenRegisteringServiceByRegisterClass_ShouldRegisterSuccessfully()
    {
        // Arrange
        var container = new Container();
        container.Register<INumberServices, NumberServices>();
        // Act
        var service = container.Resolve<INumberServices>();
        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<NumberServices>();
    }
    // Testa o método Register com uma função de criação personalizada de um container de injeção de dependência
    [Fact]
    public void Register_WhenRegisteringServiceByRegisterFactory_ShouldRegisterSuccessfully()
    {
        // Arrange
        var container = new Container();
        container.Register<INumberServices>(() => new NumberServices());
        // Act
        var service = container.Resolve<INumberServices>();
        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<NumberServices>();
    }
    // Testa o método RegisterSingleton de um container de injeção de dependência
    [Fact]
    public void RegisterSingleton_WhenRegisteringSingletonService_ShouldRegisterSuccessfully()
    {
        // Arrange
        var container = new Container();
        var numberService = new NumberServices();
        container.RegisterSingleton<INumberServices>(numberService);
        // Act
        var service = container.Resolve<INumberServices>();
        // Assert
        service.Should().NotBeNull();
        service.Should().BeSameAs(numberService);
    }
}
