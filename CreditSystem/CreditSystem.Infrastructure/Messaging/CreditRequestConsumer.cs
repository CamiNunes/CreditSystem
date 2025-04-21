using CreditSystem.Application.Interfaces;
using CreditSystem.Contracts.Messages;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CreditSystem.Infrastructure.Messaging;

// Alterar para herdar de BackgroundService e implementar IDisposable
public class CreditRequestConsumer : BackgroundService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ICreditService _creditService;
    private const string ExchangeName = "credit-exchange";
    private const string QueueName = "credit-requests-queue";

    public CreditRequestConsumer(string hostname, ICreditService creditService)
    {
        _creditService = creditService ?? throw new ArgumentNullException(nameof(creditService));

        var factory = new ConnectionFactory()
        {
            HostName = hostname,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true);

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: string.Empty);
    }

    // Implementar o método ExecuteAsync obrigatório do BackgroundService
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<CreditRequestMessage>(message);

                if (request != null)
                {
                    await _creditService.EvaluateCreditRequestAsync(request.RequestId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] Processamento da mensagem: {ex.Message}");
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}