namespace CreditSystem.Contracts.DTOs.Authentication
{
    public record AuthResponse(
        string Token,
        DateTime Expiration
    );
}
