namespace CreditSystem.Application.Interfaces;

public interface IMessagingService
{
    Task PublishMessageAsync(string routingKey, object message);
}