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
    // Testa o método RegisterConstant: registra uma instância e a resolve
    [Fact]
    public void RegisterConstant_WhenRegisteringInstance_ShouldResolveSameInstance()
    {
        // Arrange
        var container = new Container();
        var numberService = new NumberServices();
        container.RegisterConstant<INumberServices>(numberService);
        // Act
        var service = container.Resolve<INumberServices>();
        // Assert
        service.Should().BeSameAs(numberService);
    }
    // Testa que RegisterConstant é last-wins: a segunda chamada sobrescreve a primeira
    [Fact]
    public void RegisterConstant_WhenRegisteringTwice_ShouldResolveLastRegisteredInstance()
    {
        // Arrange
        var container = new Container();
        var first = new NumberServices();
        var second = new NumberServices();
        container.RegisterConstant<INumberServices>(first);
        container.RegisterConstant<INumberServices>(second);
        // Act
        var service = container.Resolve<INumberServices>();
        // Assert
        service.Should().BeSameAs(second);
    }
    // Testa o registro nomeado (contract): duas instâncias do mesmo serviço diferenciadas por contract
    [Fact]
    public void RegisterConstant_WhenRegisteringWithContracts_ShouldResolveEachByItsContract()
    {
        // Arrange
        var container = new Container();
        var themeProvider = new NumberServices();
        var transparencyProvider = new NumberServices();
        container.RegisterConstant<INumberServices>(themeProvider, "theme");
        container.RegisterConstant<INumberServices>(transparencyProvider, "transparency");
        // Act
        var resolvedTheme = container.Resolve<INumberServices>("theme");
        var resolvedTransparency = container.Resolve<INumberServices>("transparency");
        // Assert
        resolvedTheme.Should().BeSameAs(themeProvider);
        resolvedTransparency.Should().BeSameAs(transparencyProvider);
    }
    // Testa que um contract nomeado não colide com o registro keyless do mesmo tipo
    [Fact]
    public void Resolve_WhenContractRegisteredButKeylessRequested_ShouldReturnNull()
    {
        // Arrange
        var container = new Container();
        container.RegisterConstant<INumberServices>(new NumberServices(), "theme");
        // Act
        var keyless = container.Resolve<INumberServices>();
        // Assert
        keyless.Should().BeNull();
    }
    // Testa que resolver um contract inexistente retorna null
    [Fact]
    public void Resolve_WhenContractNotRegistered_ShouldReturnNull()
    {
        // Arrange
        var container = new Container();
        container.RegisterConstant<INumberServices>(new NumberServices(), "theme");
        // Act
        var service = container.Resolve<INumberServices>("transparency");
        // Assert
        service.Should().BeNull();
    }
}
