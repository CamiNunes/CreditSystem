using CreditSystem.Application.Interfaces;
using CreditSystem.Application.Services;
using CreditSystem.Infrastructure.Data;
using CreditSystem.Infrastructure.Messaging;
using CreditSystem.Infrastructure.Repositories;
using CreditSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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
builder.Services.AddScoped<ICreditService, CreditService>();
builder.Services.AddScoped<ICreditScoreProvider, MockCreditScoreProvider>();

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
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
        //options.RoutePrefix = string.Empty; // Coloca o Swagger na raiz
        //options.ConfigObject.DisplayRequestDuration = true;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();