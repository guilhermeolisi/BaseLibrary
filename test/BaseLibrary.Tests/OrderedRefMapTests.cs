using BaseLibrary.Collections;
using FluentAssertions;

namespace BaseLibrary.Tests;

public class OrderedRefMapTests
{
    [Fact]
    public void SetFromOrdered_ShouldReplaceContent_AndExposeKeySortedView_WhenGivenAscendingArrays()
    {
        // Arrange
        var map = new OrderedRefMap<string>();
        int[] keys = { 1, 3, 7 };
        string[] values = { "a", "c", "g" };

        // Act
        map.SetFromOrdered(keys, values);

        // Assert
        map.Count.Should().Be(3);
        map.OrderedKeys.ToArray().Should().Equal(1, 3, 7);
        map.OrderedValues.ToArray().Should().Equal("a", "c", "g");
        map[3].Should().Be("c");
    }

    [Fact]
    public void SetFromOrdered_ShouldDiscardPreviousEntries_WhenCalledTwice()
    {
        // Arrange
        var map = new OrderedRefMap<string>();
        map.SetFromOrdered(new[] { 1, 2 }, new[] { "x", "y" });

        // Act: a segunda chamada substitui todo o conteúdo (semântica wholesale do bulk-fill da geração).
        map.SetFromOrdered(new[] { 5, 6, 7 }, new[] { "f", "g", "h" });

        // Assert
        map.Count.Should().Be(3);
        map.ContainsKey(1).Should().BeFalse();
        map.ContainsKey(2).Should().BeFalse();
        map.OrderedKeys.ToArray().Should().Equal(5, 6, 7);
    }

    [Fact]
    public void GetKeysList_ShouldReturnAllKeys_RegardlessOfOrder()
    {
        // Arrange
        var map = new OrderedRefMap<string>();
        map.SetFromOrdered(new[] { 7, 3, 1 }.OrderBy(k => k).ToArray(), new[] { "a", "c", "g" });

        // Act
        List<int> keys = map.GetKeysList();

        // Assert
        keys.Should().BeEquivalentTo(new[] { 1, 3, 7 });
    }

    [Fact]
    public void ContainsKeyUnsafe_ShouldMatchContainsKey()
    {
        // Arrange
        var map = new OrderedRefMap<string>();
        map.SetFromOrdered(new[] { 2, 4 }, new[] { "b", "d" });

        // Act & Assert
        map.ContainsKeyUnsafe(2).Should().BeTrue();
        map.ContainsKeyUnsafe(5).Should().BeFalse();
    }

    [Fact]
    public void RemoveUnsafe_ShouldDropKey_AndRebuildOrderedView()
    {
        // Arrange
        var map = new OrderedRefMap<string>();
        map.SetFromOrdered(new[] { 1, 3, 7 }, new[] { "a", "c", "g" });

        // Act
        bool removed = map.RemoveUnsafe(3);

        // Assert
        removed.Should().BeTrue();
        map.Count.Should().Be(2);
        map.OrderedKeys.ToArray().Should().Equal(1, 7);
        map.OrderedValues.ToArray().Should().Equal("a", "g");
    }

    [Fact]
    public void RemoveUnsafe_ShouldReturnFalse_WhenKeyAbsent()
    {
        // Arrange
        var map = new OrderedRefMap<string>();
        map.SetFromOrdered(new[] { 1 }, new[] { "a" });

        // Act & Assert
        map.RemoveUnsafe(99).Should().BeFalse();
        map.Count.Should().Be(1);
    }

    [Fact]
    public void TryAdd_AfterSetFromOrdered_ShouldInvalidateOrder_AndResortOnRead()
    {
        // Arrange
        var map = new OrderedRefMap<string>();
        map.SetFromOrdered(new[] { 10, 20 }, new[] { "j", "t" });

        // Act: add com chave intermediária deve aparecer na posição ordenada correta.
        bool added = map.TryAdd(15, "o");

        // Assert
        added.Should().BeTrue();
        map.OrderedKeys.ToArray().Should().Equal(10, 15, 20);
        map.OrderedValues.ToArray().Should().Equal("j", "o", "t");
    }

    [Fact]
    public void ParallelTryAdd_ThenOrderedKeys_ShouldContainAllKeysSorted()
    {
        // Arrange: espelha o uso real (várias threads inserindo chaves disjuntas, leitura ordenada depois).
        var map = new OrderedRefMap<int>();

        // Act
        Parallel.For(0, 1000, i => map.TryAdd(i, i * 2));

        // Assert
        map.Count.Should().Be(1000);
        int[] ordered = map.OrderedKeys.ToArray();
        ordered.Should().BeInAscendingOrder();
        ordered.Should().HaveCount(1000);
        ordered[0].Should().Be(0);
        ordered[999].Should().Be(999);
    }
}
