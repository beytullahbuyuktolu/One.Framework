namespace SharedKernel.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string topic, CancellationToken cancellationToken = default) where T : class;
    Task SubscribeAsync<T>(string topic, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;
}
