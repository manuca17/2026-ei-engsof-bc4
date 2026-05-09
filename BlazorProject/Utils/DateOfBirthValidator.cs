namespace BlazorProject.Utils;

public static class DateOfBirthValidator
{
    public const int MaximumAge = 120;

    public static DateOnly MinimumDate => DateOnly.FromDateTime(DateTime.Today.AddYears(-MaximumAge));

    public static DateOnly MaximumDate => DateOnly.FromDateTime(DateTime.Today);

    public static string MinimumDateInputValue => MinimumDate.ToString("yyyy-MM-dd");

    public static string MaximumDateInputValue => MaximumDate.ToString("yyyy-MM-dd");

    public static bool TryValidate(DateOnly? dateOfBirth, out string errorMessage)
    {
        if (!dateOfBirth.HasValue)
        {
            errorMessage = "A data de nascimento e obrigatoria.";
            return false;
        }

        if (dateOfBirth.Value > MaximumDate)
        {
            errorMessage = "A data de nascimento nao pode ser no futuro.";
            return false;
        }

        if (dateOfBirth.Value < MinimumDate)
        {
            errorMessage = $"A idade nao pode ser superior a {MaximumAge} anos.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
