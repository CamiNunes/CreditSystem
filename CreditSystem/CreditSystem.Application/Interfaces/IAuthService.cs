using CreditSystem.Contracts.DTOs.Authentication;

namespace CreditSystem.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> AuthenticateAsync(LoginRequest request);
    }
}
