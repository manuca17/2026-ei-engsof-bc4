namespace BlazorProject.Utils;

public static class CsvHelper
{
    /// <summary>
    /// Escapes a value for use inside a quoted CSV field by doubling any embedded double-quote characters.
    /// </summary>
    public static string EscapeField(string? value) =>
        (value ?? string.Empty).Replace("\"", "\"\"");

    /// <summary>
    /// Produces a safe filename fragment from an arbitrary string.
    /// </summary>
    public static string SafeFileName(string name) =>
        string.Concat(name.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')).ToLower();
}
