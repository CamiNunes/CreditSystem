using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace CreditSystem.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddCustomAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Configuração dos eventos (OnChallenge/OnForbidden)
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    var problem = new ProblemDetails
                    {
                        Status = 401,
                        Title = "Não autenticado",
                        Detail = "Token inválido ou ausente"
                    };
                    context.Response.ContentType = "application/problem+json";
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(problem);
                },
                OnForbidden = async context =>
                {
                    var problem = new ProblemDetails
                    {
                        Status = 403,
                        Title = "Acesso proibido",
                        Detail = "Você não tem permissão para este recurso"
                    };
                    context.Response.ContentType = "application/problem+json";
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(problem);
                }
            };

            // Configuração do TokenValidationParameters
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:ValidIssuer"],
                ValidAudience = configuration["JwtSettings:ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!))
            };
        });

        return services;
    }
}