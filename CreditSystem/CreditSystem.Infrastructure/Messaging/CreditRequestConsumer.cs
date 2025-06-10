using CreditSystem.Application.Interfaces;
using CreditSystem.Contracts.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CreditSystem.Infrastructure.Messaging;

public class CreditRequestConsumer : BackgroundService, IDisposable
{
    private readonly ILogger<CreditRequestConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private const string ExchangeName = "credit-exchange";
    private const string QueueName = "credit-requests-queue";

    public CreditRequestConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<CreditRequestConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //await InitializeRabbitMQ();

        //var consumer = new AsyncEventingBasicConsumer(_channel);

        //consumer.Received += async (model, ea) =>
        //{
        //    try
        //    {
        //        using var scope = _scopeFactory.CreateScope();
        //        var creditService = scope.ServiceProvider.GetRequiredService<ICreditService>();

        //        var body = ea.Body.ToArray();
        //        var message = Encoding.UTF8.GetString(body);
        //        var request = JsonSerializer.Deserialize<CreditRequestMessage>(message);

        //        if (request != null)
        //        {
        //            _logger.LogInformation("Processing credit request: {RequestId}", request.RequestId);
        //            await creditService.EvaluateCreditRequestAsync(request.RequestId);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing message");
        //    }
        //};

        //_channel.BasicConsume(
        //    queue: QueueName,
        //    autoAck: true,
        //    consumer: consumer);

        //_logger.LogInformation("Credit Request Consumer started");
    }

    private async Task InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:Hostname"],
            Port = int.Parse(_configuration["RabbitMQ:Port"]),
            UserName = _configuration["RabbitMQ:Username"],
            Password = _configuration["RabbitMQ:Password"],
            VirtualHost = _configuration["RabbitMQ:VirtualHost"],
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            RequestedHeartbeat = TimeSpan.FromSeconds(30),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        try
        {
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
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public override void Dispose()
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
            _logger.LogError(ex, "Error cleaning up RabbitMQ resources");
        }

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}