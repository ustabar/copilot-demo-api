using System.Text.RegularExpressions;
using Contoso.CustomerApi.Models;

namespace Contoso.CustomerApi.Validation;

public sealed record ValidationError(string Field, string Message);

/// <summary>
/// Input validation for customer payloads.
///
/// Everything a caller sends passes through here before it reaches the service layer.
/// The rules are intentionally boring - the interesting part for a demo is what happens
/// when a change bypasses this class rather than extending it.
/// </summary>
public static partial class CustomerValidator
{
    private const int MaxNameLength = 120;
    private const int MaxCountryLength = 2;

    public static IReadOnlyList<ValidationError> Validate(CustomerRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(new ValidationError(nameof(request.Name), "Name is required."));
        }
        else if (request.Name.Length > MaxNameLength)
        {
            errors.Add(new ValidationError(nameof(request.Name), $"Name must be {MaxNameLength} characters or fewer."));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add(new ValidationError(nameof(request.Email), "Email is required."));
        }
        else if (!EmailPattern().IsMatch(request.Email))
        {
            errors.Add(new ValidationError(nameof(request.Email), "Email is not a valid address."));
        }

        if (string.IsNullOrWhiteSpace(request.Country))
        {
            errors.Add(new ValidationError(nameof(request.Country), "Country is required."));
        }
        else if (request.Country.Length != MaxCountryLength)
        {
            errors.Add(new ValidationError(nameof(request.Country), "Country must be a two-letter ISO code."));
        }

        if (!Enum.IsDefined(request.Tier))
        {
            errors.Add(new ValidationError(nameof(request.Tier), "Tier is not a recognised value."));
        }

        return errors;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailPattern();
}
