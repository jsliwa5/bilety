namespace PTickets.Shared.ValueObjects;

using System.Text.RegularExpressions;

public sealed record RegistrationNumber
{
    public string Value { get; }

    private static readonly Regex AllowedCharsRegex =
        new(@"^[A-Z0-9 \-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public RegistrationNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Registration number cannot be empty.", nameof(number));

        var trimmed = number.Trim().ToUpperInvariant();

        if (!AllowedCharsRegex.IsMatch(trimmed))
            throw new ArgumentException("Registration number contains invalid characters.", nameof(number));

        var compact = trimmed.Replace(" ", string.Empty).Replace("-", string.Empty);
        if (compact.Length is < 2 or > 10)
            throw new ArgumentException("Registration number must be between 2 and 10 characters.", nameof(number));

        Value = trimmed;
    }

    public static implicit operator string(RegistrationNumber registrationNumber) => registrationNumber.Value;

    public override string ToString() => Value;
}
