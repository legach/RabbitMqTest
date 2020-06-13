using System;
using System.Text;
using Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus
{
    public class EventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public EventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var connectionFactory = new ConnectionFactory()
            {
                HostName = "127.0.0.1",
                Port = 5672, //!important
                UserName = "my_guest",
                Password = "my_pass",
                DispatchConsumersAsync = true,
            };

            _connection = connectionFactory.CreateConnection();
        }

        public IModel Channel => _channel ??= _connection.CreateModel();

        public void Publish(IEvent @event, string exchange)
        {
            CreateExchange(exchange);
            var serializedEvent = JsonConvert.SerializeObject(@event);
            var bytesEvent = Encoding.UTF8.GetBytes(serializedEvent);
            Channel.BasicPublish(exchange, string.Empty, body:bytesEvent);
        }

        public void Subscribe<THandler, TEvent>(string subscriber, string exchange) where THandler : IEventHandler<TEvent> where TEvent : IEvent
        {
            BindQueue(exchange, subscriber);
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.Received += async (o, args) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
                    var message = Encoding.UTF8.GetString(args.Body.Span);
                    var @event = JsonConvert.DeserializeObject<TEvent>(message);

                    await handler.HandleAsync(@event);

                    Channel.BasicAck(args.DeliveryTag, multiple:false);
                }
            };

            Channel.BasicConsume(subscriber, autoAck: false, consumer);
        }

        private void CreateExchange(string name)
        {
            Channel.ExchangeDeclare(name, ExchangeType.Fanout, durable:true); //Fanout - to catch msg to all, Direct - to one of many
        }

        private void BindQueue(string exchange, string subscriber)
        {
            CreateExchange(exchange);
            var queue = Channel.QueueDeclare(subscriber, durable:true, exclusive:false, autoDelete:false);
            Channel.QueueBind(queue.QueueName, exchange, string.Empty);
        }

    }
}