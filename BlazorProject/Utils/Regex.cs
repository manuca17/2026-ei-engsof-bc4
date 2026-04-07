using System.Text.RegularExpressions;

namespace BlazorProject.Utils;

public static partial class Regex
{
    // Portuguese phone numbers:
    // - Mobile: starts with 9 (91, 92, 93, 96) followed by 8 digits
    // - Landline: starts with 2 followed by 8 digits
    // - Optionally prefixed by +351 or 00351, with optional spaces/dashes
    [GeneratedRegex(@"^(\+351|00351)?[\s-]?(2\d|9[1236])\d[\s-]?\d{3}[\s-]?\d{3}$")]
    private static partial System.Text.RegularExpressions.Regex PhoneRegex();

    // Portuguese postal codes: XXXX-XXX (4 digits, dash, 3 digits)
    [GeneratedRegex(@"^\d{4}-\d{3}$")]
    private static partial System.Text.RegularExpressions.Regex PostalCodeRegex();

    // Standard email validation
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial System.Text.RegularExpressions.Regex EmailRegex();

    // Doctor's card number: exactly 9 digits
    [GeneratedRegex(@"^\d{9}$")]
    private static partial System.Text.RegularExpressions.Regex NumCarteiraRegex();

    /// <summary>
    /// Validates a Portuguese phone number.
    /// Accepts mobile (9x) and landline (2x) numbers, with optional +351/00351 prefix.
    /// </summary>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
        return PhoneRegex().IsMatch(phoneNumber.Trim());
    }

    /// <summary>
    /// Validates a Portuguese postal code in the format XXXX-XXX.
    /// </summary>
    public static bool IsValidPostalCode(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode)) return false;
        return PostalCodeRegex().IsMatch(postalCode.Trim());
    }

    /// <summary>
    /// Validates an email address.
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return EmailRegex().IsMatch(email.Trim());
    }

    /// <summary>
    /// Validates a doctor's card number (NumCarteira).
    /// Must be exactly 9 digits.
    /// </summary>
    public static bool IsValidNumCarteira(string? numCarteira)
    {
        if (string.IsNullOrWhiteSpace(numCarteira)) return false;
        return NumCarteiraRegex().IsMatch(numCarteira.Trim());
    }
}
