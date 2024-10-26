using API_Commandes.Models;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

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
