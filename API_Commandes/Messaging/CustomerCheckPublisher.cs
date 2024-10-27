
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Prometheus;

namespace API_Commandes.Messaging
{
    public interface ICustomerCheckPublisher
    {
        void PublishCustomerCheckRequest(int clientId);
    }

    public class CustomerCheckPublisher : ICustomerCheckPublisher, IDisposable
    {
        private readonly IModel _channel;
        private readonly ILogger<CustomerCheckPublisher> _logger;
        private static readonly Counter _messagesPublished = Metrics.CreateCounter("customer_check_messages_published", "Total number of Customer Check messages published");

        public CustomerCheckPublisher(IConnection connection, ILogger<CustomerCheckPublisher> logger)
        {
            _channel = connection.CreateModel();
            _logger = logger;

            _channel.QueueDeclare(queue: "customer_check_request",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _logger.LogInformation("CustomerCheckPublisher initialized and queue declared.");
        }

        public void PublishCustomerCheckRequest(int clientId)
        {
            var customerRequest = new CustomerCheckRequest
            {
                ClientId = clientId
            };
            _logger.LogInformation($"Publishing customer check request for client ID: {clientId}.");

            var requestBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(customerRequest));

            _channel.BasicPublish(exchange: "",
                                  routingKey: "customer_check_request",
                                  basicProperties: null,
                                  body: requestBody);

            _messagesPublished.Inc();
            _logger.LogInformation($"Customer check request for client ID: {clientId} published.");

        }

        public uint GetMessageCount()
        {
            return _channel.MessageCount("customer_check_request");
        }
        public void Dispose()
        {
            _channel?.Close();
        }
    }
}
