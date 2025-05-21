// Infrastructure/Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CreditSystem.Application.Interfaces;
using CreditSystem.Contracts.DTOs.Authentication;
using CreditSystem.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CreditSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        IConfiguration configuration,
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Verificar se o usuário já existe
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Usuário já cadastrado");
        }

        // Criar novo usuário
        var newUser = new User
        {
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        // Gerar hash da senha
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

        await _userRepository.CreateAsync(newUser);

        return await GenerateJwtToken(newUser);
    }

    public async Task<AuthResponse> AuthenticateAsync(LoginRequest request)
    {
        // Buscar usuário
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        // Verificar senha
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result != PasswordVerificationResult.Success)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        return await GenerateJwtToken(user);
    }

    private async Task<AuthResponse> GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("CreatedAt", user.CreatedAt.ToString("O"))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(claims),
    Expires = DateTime.UtcNow.AddMinutes(
        _configuration.GetSection("JwtSettings:ExpiryInMinutes").Get<int>()),
            Issuer = _configuration["JwtSettings:ValidIssuer"],
            Audience = _configuration["JwtSettings:ValidAudience"],
            SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature)
};

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new AuthResponse(
            Token: tokenHandler.WriteToken(token),
            Expiration: tokenDescriptor.Expires.Value
        );
    }
}