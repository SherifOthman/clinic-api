using System.Text.RegularExpressions;

namespace ClinicManagement.Application.Utils;
public static class StringUtils
{
    public static bool IsEmail(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
    }
    public static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            return input;
        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }
}
