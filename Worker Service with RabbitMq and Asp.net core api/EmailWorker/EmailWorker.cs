using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Worker
{
    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
    public class EmailWorker : BackgroundService
    {
        private readonly ILogger<EmailWorker> _logger;
        private IConnection? _connection;
        private IModel? _channel;

        public EmailWorker(ILogger<EmailWorker> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost", DispatchConsumersAsync = true };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare("email_queue", durable: true, exclusive: false, autoDelete: false, null);

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null) return;

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var email = JsonSerializer.Deserialize<EmailMessage>(body);
                _logger.LogInformation("Sending email to {To}: {Subject}", email?.To, email?.Subject);
                await Task.Delay(1000, stoppingToken); // simulate send
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume("email_queue", autoAck: false, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _connection?.Close();
            return base.StopAsync(cancellationToken);
        }
    }

}
