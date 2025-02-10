namespace HexagonalArchitecture.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
            
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money FromDecimal(decimal amount, string currency) => new(amount, currency);

    public Money Add(Money other)
    {
        if (other.Currency != Currency)
            throw new ArgumentException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (other.Currency != Currency)
            throw new ArgumentException("Cannot subtract money with different currencies");

        return new Money(Amount - other.Amount, Currency);
    }
}
