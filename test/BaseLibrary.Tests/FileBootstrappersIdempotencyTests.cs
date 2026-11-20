using BaseLibrary.DependencyInjection;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// O <see cref="FileBootstrappers"/> roda quantas vezes e de quantas threads quiserem (E318): antes, o segundo parava no
/// <c>Register&lt;IFileTXTIO&gt;</c> depois de ja ter trocado os servicos de arquivo do processo.
/// </summary>
public class FileBootstrappersIdempotencyTests
{
    [Fact]
    public void SecondBootstrap_ShouldNotThrow_AndShouldKeepTheRegisteredServices()
    {
        _ = new FileBootstrappers();
        object[] before = Resolve();

        Action again = () => _ = new FileBootstrappers();

        again.Should().NotThrow();
        object[] after = Resolve();
        for (int i = 0; i < before.Length; i++)
            after[i].Should().BeSameAs(before[i], "o segundo bootstrap nao troca {0}", before[i].GetType().Name);
        Locator.ConstantContainer.Resolve<IFileTXTIO>().Should().NotBeNull();
    }

    [Fact]
    public void ConcurrentBootstraps_ShouldNotThrow_AndShouldLeaveOneInstancePerService()
    {
        var failures = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        using var barrier = new Barrier(16);
        var threads = Enumerable.Range(0, 16).Select(i => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait();
                _ = new FileBootstrappers();
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        })
        { IsBackground = true }).ToArray();

        foreach (Thread thread in threads) thread.Start();
        foreach (Thread thread in threads) thread.Join();

        failures.Should().BeEmpty();
        object[] first = Resolve();
        object[] second = Resolve();
        for (int i = 0; i < first.Length; i++)
            second[i].Should().BeSameAs(first[i]);
    }

    private static object[] Resolve()
    {
        IContainer c = Locator.ConstantContainer;
        return
        [
            c.Resolve<IFileServicesText>()!,
            c.Resolve<IFileServicesCheck>()!,
            c.Resolve<IFileServicesName>()!,
            c.Resolve<IFileServicesDirectory>()!,
            c.Resolve<IFileServicesFile>()!,
            c.Resolve<IFileServices>()!,
        ];
    }
}
