using System.Text.RegularExpressions;

namespace SwU.WebApp.Services;

public class ValidationService
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CardNumberRegex = new(
        @"^[A-Z0-9]{2,5}-\d{2,3}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex UsernameRegex = new(
        @"^[a-zA-Z0-9_]{3,20}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SetCodeRegex = new(
        @"^[A-Z0-9]{2,10}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public bool IsValidEmail(string? email)
    {
        return !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());
    }

    public bool IsValidCardNumber(string? cardNumber)
    {
        return !string.IsNullOrWhiteSpace(cardNumber) && CardNumberRegex.IsMatch(cardNumber.Trim());
    }

    public bool IsValidUsername(string? username)
    {
        return !string.IsNullOrWhiteSpace(username) && UsernameRegex.IsMatch(username.Trim());
    }

    public bool IsValidSetCode(string? code)
    {
        return !string.IsNullOrWhiteSpace(code) && SetCodeRegex.IsMatch(code.Trim());
    }

    public bool IsValidPassword(string? password)
    {
        return password != null && password.Length >= 6 && password.Length <= 100;
    }

    public bool IsValidComment(string? comment)
    {
        string text = comment?.Trim() ?? string.Empty;
        return text.Length >= 3 && text.Length <= 500;
    }

    public bool IsValidCommentTitle(string? title)
    {
        string text = title?.Trim() ?? string.Empty;
        return text.Length >= 3 && text.Length <= 120;
    }

    public bool IsValidRating(int rating)
    {
        return rating >= 1 && rating <= 5;
    }

    public bool IsValidCost(int cost)
    {
        return cost >= 0 && cost <= 20;
    }

    public bool IsValidPower(int? power)
    {
        return !power.HasValue || (power.Value >= 0 && power.Value <= 30);
    }

    public bool IsValidHp(int? hp)
    {
        return !hp.HasValue || (hp.Value >= 0 && hp.Value <= 50);
    }
}
