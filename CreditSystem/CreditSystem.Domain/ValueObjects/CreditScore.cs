namespace CreditSystem.Domain.ValueObjects
{
    // Princípio SOLID: Single Responsibility - Representa exclusivamente um score de crédito
    public record CreditScore
    {
        public int Value { get; }

        public CreditScore(int value)
        {
            if (value < 300 || value > 850)
                throw new ArgumentException("Credit score must be between 300 and 850");

            Value = value;
        }

        public bool IsGood() => Value >= 700;
        public bool IsFair() => Value >= 600 && Value < 700;
        public bool IsPoor() => Value < 600;
    }
}
