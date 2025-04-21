using CreditSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CreditSystem.Infrastructure.Messaging;

// Princípio SOLID: Interface Segregation - Implementa apenas IMessagingService
public class RabbitMQService : IMessagingService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "credit-exchange";

    public RabbitMQService(string hostname)
    {
        var factory = new ConnectionFactory() { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Fanout);
    }

    public async Task PublishMessageAsync(string routingKey, object message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}