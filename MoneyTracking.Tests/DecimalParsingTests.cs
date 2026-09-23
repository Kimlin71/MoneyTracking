using Xunit;

namespace MoneyTracking.Tests;

// Mirrors the comma/dot normalization logic used in PromptDecimal and EditItem
public class DecimalParsingTests
{
    private static decimal? ParseAmount(string input)
    {
        string normalized = input.Replace(',', '.');
        if (decimal.TryParse(normalized,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal value) && value > 0)
            return value;
        return null;
    }

    [Fact]
    public void ParseAmount_DotSeparator_ParsesCorrectly()
        => Assert.Equal(1250.50m, ParseAmount("1250.50"));

    [Fact]
    public void ParseAmount_CommaSeparator_ParsesCorrectly()
        => Assert.Equal(1250.50m, ParseAmount("1250,50"));

    [Fact]
    public void ParseAmount_WholeNumber_ParsesCorrectly()
        => Assert.Equal(1250m, ParseAmount("1250"));

    [Fact]
    public void ParseAmount_SwedishLocaleFormat_ParsesCorrectly()
        => Assert.Equal(32000.00m, ParseAmount("32000,00"));

    [Fact]
    public void ParseAmount_Zero_ReturnsNull()
        => Assert.Null(ParseAmount("0"));

    [Fact]
    public void ParseAmount_Negative_ReturnsNull()
        => Assert.Null(ParseAmount("-100"));

    [Fact]
    public void ParseAmount_Empty_ReturnsNull()
        => Assert.Null(ParseAmount(""));

    [Fact]
    public void ParseAmount_NonNumeric_ReturnsNull()
        => Assert.Null(ParseAmount("abc"));

    [Fact]
    public void ParseAmount_LeadingWhitespace_ReturnsNull()
        => Assert.Null(ParseAmount("  "));
}
