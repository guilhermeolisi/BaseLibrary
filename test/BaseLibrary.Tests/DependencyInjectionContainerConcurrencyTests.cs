using BaseLibrary.DependencyInjection;
using BaseLibrary.Numbers;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// O <see cref="Container"/> e lido e escrito por varias threads (varias instancias do motor no mesmo processo, E318):
/// registro e leitura juntos nao lancam nem corrompem, e o primeiro-vence dos <c>TryRegister*</c> e atomico.
/// </summary>
public class DependencyInjectionContainerConcurrencyTests
{
    private interface IProbe { int Id { get; } }
    private sealed class Probe(int id) : IProbe { public int Id { get; } = id; }
    private interface IOther { }
    private sealed class Other : IOther { }

    [Fact]
    public void RegisterAndResolve_ShouldNotThrowNorHang_WhenSixteenThreadsHammerTheContainer()
    {
        var container = new Container();
        var failures = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var stop = DateTime.UtcNow.AddSeconds(2);
        using var barrier = new Barrier(16);
        var threads = Enumerable.Range(0, 16).Select(t => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait();
                int i = 0;
                while (DateTime.UtcNow < stop)
                {
                    string key = $"t{t}-{i++ % 512}";
                    container.RegisterConstant<IProbe>(new Probe(i), key);
                    container.RegisterConstant<IProbe>(new Probe(i));
                    container.TryRegisterLazySingleton<INumberServices>(() => new NumberServices());
                    _ = container.Resolve<IProbe>(key);
                    _ = container.Resolve<IProbe>($"t{(t + 1) % 16}-{i % 512}");
                    _ = container.Resolve<IProbe>();
                    _ = container.Resolve<INumberServices>();
                }
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        })
        { IsBackground = true }).ToArray();

        foreach (Thread thread in threads) thread.Start();
        bool allJoined = threads.All(thread => thread.Join(TimeSpan.FromSeconds(30)));

        allJoined.Should().BeTrue("um dicionario corrompido por escrita concorrente pode prender a thread num laco");
        failures.Should().BeEmpty();
        container.Resolve<INumberServices>().Should().NotBeNull();
    }

    [Fact]
    public void TryRegisterSingleton_ShouldRunTheCreatorOnce_WhenThirtyTwoThreadsRaceForTheSameType()
    {
        // Oraculo: caso limite, primeiro-vence (um registro, um criador, a mesma instancia para todos).
        for (int round = 0; round < 200; round++)
        {
            var container = new Container();
            int created = 0;
            int winners = 0;
            using var barrier = new Barrier(32);
            var threads = Enumerable.Range(0, 32).Select(t => new Thread(() =>
            {
                barrier.SignalAndWait();
                if (container.TryRegisterSingleton<IProbe>(() => new Probe(Interlocked.Increment(ref created))))
                    Interlocked.Increment(ref winners);
            })
            { IsBackground = true }).ToArray();

            foreach (Thread thread in threads) thread.Start();
            foreach (Thread thread in threads) thread.Join();

            created.Should().Be(1, "rodada {0}: o criador roda uma vez so", round);
            winners.Should().Be(1, "rodada {0}: so uma chamada registra", round);
            container.Resolve<IProbe>()!.Id.Should().Be(1);
        }
    }

    [Fact]
    public void TryRegisterSingleton_ShouldCreateTheInstanceAtRegistration()
    {
        var container = new Container();
        int created = 0;

        container.TryRegisterSingleton<IProbe>(() => new Probe(++created));

        created.Should().Be(1, "como o RegisterSingleton com a instancia pronta, nao na primeira resolucao");
    }

    [Fact]
    public void TryRegisterVariants_ShouldReturnFalseAndKeepTheFirst_WhenTheTypeIsRegisteredInAnyMap()
    {
        var first = new Probe(1);
        var bySingleton = new Container();
        bySingleton.RegisterSingleton<IProbe>(first);
        var byLazy = new Container();
        byLazy.RegisterLazySingleton<IProbe>(() => first);
        var byFactory = new Container();
        byFactory.Register<IProbe>(() => first);

        foreach (Container container in new[] { bySingleton, byLazy, byFactory })
        {
            container.TryRegisterSingleton<IProbe>(() => new Probe(2)).Should().BeFalse();
            container.TryRegisterLazySingleton<IProbe>(() => new Probe(3)).Should().BeFalse();
            container.TryRegister<IProbe>(() => new Probe(4)).Should().BeFalse();
            container.Resolve<IProbe>().Should().BeSameAs(first);
        }
    }

    [Fact]
    public void TryRegister_ShouldRegisterATransientFactory_WhenTheTypeIsFree()
    {
        var container = new Container();

        container.TryRegister<IOther>(() => new Other()).Should().BeTrue();

        container.Resolve<IOther>().Should().NotBeSameAs(container.Resolve<IOther>());
    }

    [Fact]
    public void TryRegisterLazySingleton_ShouldDeferTheCreatorToTheFirstResolve()
    {
        var container = new Container();
        int created = 0;

        container.TryRegisterLazySingleton<IProbe>(() => new Probe(++created)).Should().BeTrue();
        created.Should().Be(0);

        IProbe a = container.Resolve<IProbe>()!;
        IProbe b = container.Resolve<IProbe>()!;
        created.Should().Be(1);
        a.Should().BeSameAs(b);
    }

    [Fact]
    public void RegisterSingleton_ShouldResolveTheLastInstance_WhenRegisteredTwice()
    {
        // Caracterizacao: os testes do Sindarin e do Nimloth trocam servico por mock com um segundo RegisterSingleton.
        var container = new Container();
        var first = new Probe(1);
        var second = new Probe(2);

        container.RegisterSingleton<IProbe>(first);
        container.RegisterSingleton<IProbe>(second);

        container.Resolve<IProbe>().Should().BeSameAs(second);
    }
}
