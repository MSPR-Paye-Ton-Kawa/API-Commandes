﻿using API_Commandes.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;


namespace API_Commandes.Messaging
{
    public class StockCheckPublisher : IDisposable
     {
        private readonly IModel _channel;
        private readonly ILogger<StockCheckPublisher> _logger; 

        public StockCheckPublisher(IConnection connection, ILogger<StockCheckPublisher> logger)
        {
            _channel = connection.CreateModel();
            _logger = logger; 

            // Declare the queue for stock check
            _channel.QueueDeclare(queue: "stock_check_request",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _logger.LogInformation("StockCheckPublisher initialized and queue declared.");
        }

        public void PublishStockCheckRequest(Order order)
        {
            var stockRequest = new StockCheckRequest
            {
                // Generation of an unic ID
                RequestId = Guid.NewGuid().ToString(),  
                Items = order.OrderItems.Select(item => new StockCheckItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };
            _logger.LogInformation($"Publishing stock check request {stockRequest.RequestId} with {stockRequest.Items.Count} items.");

            var requestBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(stockRequest));

            // Publication of the request
            _channel.BasicPublish(exchange: "",
                                  routingKey: "stock_check_request",
                                  basicProperties: null,
                                  body: requestBody);
            _logger.LogInformation($"Stock check request {stockRequest.RequestId} published");
        }
        public void Dispose()
        {
            _channel?.Close();
        }
    }
}