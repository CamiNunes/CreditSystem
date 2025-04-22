using CreditSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CreditSystem.Infrastructure.Messaging;

public class RabbitMQService : IMessagingService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private const string ExchangeName = "credit-exchange";

    public RabbitMQService(string hostname, ILogger<RabbitMQService> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory()
        {
            HostName = hostname,
            Port = 5672, // Porta explícita
            UserName = "admin", // Credenciais padrão
            Password = "admin123",
            VirtualHost = "/",
            RequestedConnectionTimeout = TimeSpan.FromSeconds(10) // Timeout reduzido
        };

        try
        {
            _logger.LogInformation("Tentando conectar ao RabbitMQ em {Host}:{Port}", hostname, 5672);
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
            _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Falha na conexão com RabbitMQ");
            throw;
        }
    }

    public Task PublishMessageAsync(string routingKey, object message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem");
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao liberar recursos");
        }
    }
}