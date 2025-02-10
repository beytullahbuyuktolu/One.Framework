using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SharedKernel.Messaging.RabbitMQ;

public class RabbitMQMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessageBus> _logger;
    private readonly Dictionary<string, IModel> _consumerChannels;

    public RabbitMQMessageBus(RabbitMQSettings settings, ILogger<RabbitMQMessageBus> logger)
    {
        _logger = logger;
        _consumerChannels = new Dictionary<string, IModel>();

        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            UserName = settings.UserName,
            Password = settings.Password,
            Port = settings.Port
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public async Task PublishAsync<T>(T message, string topic, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _channel.ExchangeDeclare(topic, ExchangeType.Fanout, durable: true);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: topic,
                routingKey: string.Empty,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Message published to topic {Topic}: {Message}", topic, json);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to topic {Topic}", topic);
            throw;
        }
    }

    public Task SubscribeAsync<T>(string topic, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            if (_consumerChannels.ContainsKey(topic))
            {
                throw new InvalidOperationException($"Already subscribed to topic {topic}");
            }

            var channel = _connection.CreateModel();
            _consumerChannels.Add(topic, channel);

            channel.ExchangeDeclare(topic, ExchangeType.Fanout, durable: true);
            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, topic, string.Empty);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json);

                    if (message != null)
                    {
                        await handler(message);
                        channel.BasicAck(ea.DeliveryTag, false);
                        _logger.LogInformation("Message processed from topic {Topic}: {Message}", topic, json);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(queue: queueName,
                               autoAck: false,
                               consumer: consumer);

            _logger.LogInformation("Subscribed to topic {Topic}", topic);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to topic {Topic}", topic);
            throw;
        }
    }

    public void Dispose()
    {
        foreach (var channel in _consumerChannels.Values)
        {
            channel.Dispose();
        }
        _channel.Dispose();
        _connection.Dispose();
    }
}
