using BaseLibrary;
using FluentAssertions;

namespace BaseLibrary.Tests;

public class TelemetryScrubberTests
{
    private readonly TelemetryScrubber sut = new();

    [Fact]
    public void Scrub_ShouldRedactWindowsUserAccount_WhenPathContainsUsersFolder()
    {
        // Arrange
        string input = @"C:\Users\guilh\Documents\Library\Works\Projeto\Workfile.sin";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be(@"C:\Users\<redacted>\Documents\Library\Works\Projeto\Workfile.sin");
        result.Should().NotContain("guilh");
    }

    [Fact]
    public void Scrub_ShouldBeCaseInsensitiveAndPreserveRootCasing_OnWindowsPath()
    {
        // Arrange
        string input = @"c:\users\Guilh\AppData\file.db";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be(@"c:\users\<redacted>\AppData\file.db");
        result.Should().NotContain("Guilh");
    }

    [Fact]
    public void Scrub_ShouldRedactLinuxHome_WhenPathContainsHomeFolder()
    {
        // Arrange
        string input = "/home/guilh/data/sample.xye";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be("/home/<redacted>/data/sample.xye");
    }

    [Fact]
    public void Scrub_ShouldRedactMacOsHome_WhenPathContainsUsersFolder()
    {
        // Arrange
        string input = "/Users/guilh/Documents/sample.cif";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be("/Users/<redacted>/Documents/sample.cif");
    }

    [Fact]
    public void Scrub_ShouldRedactEveryOccurrence_WhenMultiplePathsPresent()
    {
        // Arrange
        string input = @"open C:\Users\alice\a.cif failed; also C:\Users\bob\b.cif";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be(@"open C:\Users\<redacted>\a.cif failed; also C:\Users\<redacted>\b.cif");
        result.Should().NotContain("alice");
        result.Should().NotContain("bob");
    }

    [Fact]
    public void Scrub_ShouldLeaveTextUnchanged_WhenNoUserPathPresent()
    {
        // Arrange
        string input = "Search Match (Phase 3) [GOS] Class sender DiffractogramProcessingViewModel";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be(input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Scrub_ShouldReturnInputAsIs_WhenNullOrEmpty(string? input)
    {
        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be(input);
    }
}
