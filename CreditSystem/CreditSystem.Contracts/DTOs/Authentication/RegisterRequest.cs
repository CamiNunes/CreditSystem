namespace CreditSystem.Contracts.DTOs.Authentication
{
    public record RegisterRequest(
        string Email,
        string Password
    );
}
