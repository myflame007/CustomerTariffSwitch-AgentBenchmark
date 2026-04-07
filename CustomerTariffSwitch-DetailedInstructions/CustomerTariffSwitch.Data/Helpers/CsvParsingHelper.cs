using System.Globalization;
using CustomerTariffSwitch.Models.Enums;

namespace CustomerTariffSwitch.Data.Helpers;

public static class CsvParsingHelper
{
    private const char Delimiter = ';';

    public static string[] SplitLine(string line)
    {
        return line.Split(Delimiter);
    }

    public static bool ParseBool(string value)
    {
        var trimmed = value.Trim();

        if (string.Equals(trimmed, "TRUE", StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(trimmed, "FALSE", StringComparison.OrdinalIgnoreCase))
            return false;

        if (string.Equals(trimmed, "1", StringComparison.Ordinal))
            return true;

        if (string.Equals(trimmed, "0", StringComparison.Ordinal))
            return false;

        throw new FormatException($"Cannot parse '{value}' as boolean. Expected TRUE/FALSE/1/0.");
    }

    public static T ParseEnum<T>(string value) where T : struct, Enum
    {
        if (Enum.TryParse<T>(value.Trim(), ignoreCase: true, out var result))
            return result;

        throw new FormatException($"Cannot parse '{value}' as {typeof(T).Name}. Valid values: {string.Join(", ", Enum.GetNames<T>())}.");
    }

    public static decimal ParseDecimal(string value)
    {
        return decimal.Parse(value.Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
    }

    public static DateTimeOffset ParseDateTimeOffset(string value)
    {
        return DateTimeOffset.Parse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    public static int GetRequiredHeaderIndex(string[] headers, string columnName, string fileName)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            if (string.Equals(headers[i].Trim(), columnName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        throw new InvalidOperationException(
            $"Required column '{columnName}' not found in '{fileName}'. Available columns: {string.Join(", ", headers)}");
    }
}
