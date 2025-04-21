using CreditSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CreditSystem.Infrastructure.Messaging;

public class CreditRequestConsumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ICreditService _creditService;
    private readonly string _queueName = "credit-requests-queue";

    public CreditRequestConsumer(string hostname, ICreditService creditService)
    {
        _creditService = creditService ?? throw new ArgumentNullException(nameof(creditService));

        var factory = new ConnectionFactory() { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: "credit-exchange", type: ExchangeType.Fanout);
        _channel.QueueDeclare(queue: _queueName,
                            durable: true,  // Alterado para true para persistência
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);
        _channel.QueueBind(queue: _queueName,
                         exchange: "credit-exchange",
                         routingKey: "credit-requests");

        var consumer = new EventingBasicConsumer(_channel);
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
                // Em produção, considere usar ILogger em vez de Console
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public record CreditRequestMessage(int RequestId, string ApplicantEmail, decimal Amount);