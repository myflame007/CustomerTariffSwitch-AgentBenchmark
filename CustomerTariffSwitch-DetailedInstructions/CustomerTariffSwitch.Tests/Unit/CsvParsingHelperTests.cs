using CustomerTariffSwitch.Data.Helpers;

namespace CustomerTariffSwitch.Tests.Unit;

public class CsvParsingHelperTests
{
    [Theory]
    [InlineData("TRUE", true)]
    [InlineData("true", true)]
    [InlineData("False", false)]
    [InlineData("FALSE", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    public void ParseBool_ValidValues_ReturnsCorrectBool(string input, bool expected)
    {
        var result = CsvParsingHelper.ParseBool(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("maybe")]
    [InlineData("yes")]
    [InlineData("")]
    public void ParseBool_InvalidValues_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => CsvParsingHelper.ParseBool(input));
    }

    [Theory]
    [InlineData("Standard", Models.Enums.SlaLevel.Standard)]
    [InlineData("premium", Models.Enums.SlaLevel.Premium)]
    [InlineData("STANDARD", Models.Enums.SlaLevel.Standard)]
    public void ParseEnum_ValidSlaLevel_ReturnsCorrectEnum(string input, Models.Enums.SlaLevel expected)
    {
        var result = CsvParsingHelper.ParseEnum<Models.Enums.SlaLevel>(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseEnum_InvalidValue_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => CsvParsingHelper.ParseEnum<Models.Enums.SlaLevel>("Unknown"));
    }

    [Fact]
    public void ParseDecimal_InvariantCulture_ParsesCorrectly()
    {
        var result = CsvParsingHelper.ParseDecimal("29.90");
        Assert.Equal(29.90m, result);
    }

    [Fact]
    public void ParseDecimal_CommaDecimal_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => CsvParsingHelper.ParseDecimal("29,90"));
    }

    [Fact]
    public void GetRequiredHeaderIndex_ExistingColumn_ReturnsIndex()
    {
        var headers = new[] { "Id", "Name", "Value" };
        var index = CsvParsingHelper.GetRequiredHeaderIndex(headers, "Name", "test.csv");
        Assert.Equal(1, index);
    }

    [Fact]
    public void GetRequiredHeaderIndex_MissingColumn_ThrowsInvalidOperationException()
    {
        var headers = new[] { "Id", "Name" };
        Assert.Throws<InvalidOperationException>(
            () => CsvParsingHelper.GetRequiredHeaderIndex(headers, "Missing", "test.csv"));
    }
}
