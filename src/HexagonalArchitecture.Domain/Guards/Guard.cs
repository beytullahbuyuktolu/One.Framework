using System.Runtime.CompilerServices;
using HexagonalArchitecture.Domain.Exceptions;

namespace HexagonalArchitecture.Domain.Guards;

/// <summary>
/// Guard clause utility methods
/// </summary>
public static class Guard
{
    public static void Against<TException>(bool condition, string message) where TException : Exception
    {
        if (condition)
        {
            throw (TException)Activator.CreateInstance(typeof(TException), message)!;
        }
    }

    public static void NotNull<T>(T value, [CallerArgumentExpression("value")] string? parameterName = null)
        where T : class
    {
        if (value is null)
        {
            throw new DomainException($"{parameterName} cannot be null");
        }
    }

    public static void NotEmpty<T>(IEnumerable<T> value, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        NotNull(value, parameterName);
        if (!value.Any())
        {
            throw new DomainException($"{parameterName} cannot be empty");
        }
    }

    public static void NotNullOrEmpty(string value, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new DomainException($"{parameterName} cannot be null or empty");
        }
    }

    public static void NotNullOrWhiteSpace(string value, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} cannot be null or whitespace");
        }
    }

    public static void NotDefault<T>(T value, [CallerArgumentExpression("value")] string? parameterName = null)
        where T : struct
    {
        if (value.Equals(default(T)))
        {
            throw new DomainException($"{parameterName} cannot be default value");
        }
    }

    public static void NotNegative(int value, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        if (value < 0)
        {
            throw new DomainException($"{parameterName} cannot be negative");
        }
    }

    public static void NotNegative(decimal value, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        if (value < 0)
        {
            throw new DomainException($"{parameterName} cannot be negative");
        }
    }

    public static void NotNegative(double value, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        if (value < 0)
        {
            throw new DomainException($"{parameterName} cannot be negative");
        }
    }

    public static void Range(int value, int min, int max, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        if (value < min || value > max)
        {
            throw new DomainException($"{parameterName} must be between {min} and {max}");
        }
    }

    public static void MaxLength(string value, int maxLength, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        if (value?.Length > maxLength)
        {
            throw new DomainException($"{parameterName} cannot be longer than {maxLength} characters");
        }
    }

    public static void Email(string value, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            if (addr.Address != value)
            {
                throw new DomainException($"{parameterName} is not a valid email address");
            }
        }
        catch
        {
            throw new DomainException($"{parameterName} is not a valid email address");
        }
    }
}
