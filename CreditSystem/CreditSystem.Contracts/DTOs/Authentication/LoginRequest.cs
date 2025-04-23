namespace CreditSystem.Contracts.DTOs.Authentication
{
    public record LoginRequest(
        string Email,
        string Password
    );
}
