﻿using CreditSystem.API.Extensions;
using CreditSystem.API.Middlewares;
using CreditSystem.Application.Interfaces;
using CreditSystem.Application.Services;
using CreditSystem.Domain;
using CreditSystem.Infrastructure.Data;
using CreditSystem.Infrastructure.Messaging;
using CreditSystem.Infrastructure.Repositories;
using CreditSystem.Infrastructure.Services;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//builder.Services.AddCustomAuthentication(builder.Configuration);

// Configuração do banco de dados
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("A connection string 'DefaultConnection' must be configured.");
    }
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

// Registro de serviços
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICreditService, CreditService>();
builder.Services.AddScoped<ICreditScoreProvider, MockCreditScoreProvider>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configuração do RabbitMQ
builder.Services.AddSingleton<IMessagingService>(provider =>
    new RabbitMQService(
        hostname: builder.Configuration["RabbitMQ:Hostname"]!,
        logger: provider.GetRequiredService<ILogger<RabbitMQService>>()
    )
);

// Registrar o consumidor como um hosted service
//builder.Services.AddHostedService(provider =>
//    new CreditRequestConsumer(
//        builder.Configuration["RabbitMQ:Hostname"],
//        provider.GetRequiredService<ICreditService>()));
builder.Services.AddHostedService<CreditRequestConsumer>();

// Swagger
builder.Services.AddEndpointsApiExplorer();

// ... outras configurações

//builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services
    .AddKeycloakAuthentication(builder.Configuration, options =>
    {
        options.RequireHttpsMetadata = false; // para ambiente de desenvolvimento
    });

builder.Services.AddKeycloakAuthorization(builder.Configuration);


// Swagger com autenticação
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Credit System API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure CORS
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

/// Configure o Swagger UI para desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();