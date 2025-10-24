using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClinicManagement.Application.Utils;
public static class CheckEmail
{
    public static bool IsEmail(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
    }
}
