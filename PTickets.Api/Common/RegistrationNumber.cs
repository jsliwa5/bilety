namespace PTickets.Api.Common;

public record RegistrationNumber
{
    public string Value { get; }

    private static readonly System.Text.RegularExpressions.Regex AllowedCharsRegex =
        new System.Text.RegularExpressions.Regex(@"^[A-Z0-9 \-]+$",
            System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    public RegistrationNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Numer rejestracyjny nie może być pusty.", nameof(number));

        var trimmed = number.Trim().ToUpperInvariant();

        if (!AllowedCharsRegex.IsMatch(trimmed))
            throw new ArgumentException("Numer rejestracyjny zawiera niedozwolone znaki.", nameof(number));

        var compact = trimmed.Replace(" ", string.Empty).Replace("-", string.Empty);
        if (compact.Length is < 5 or > 10)
            throw new ArgumentException("Numer rejestracyjny powinien mieć od 5 do 10 znaków.", nameof(number));

        Value = trimmed;
    }
}