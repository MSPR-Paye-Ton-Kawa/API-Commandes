using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using API_Commandes.Models;

public class StockRequestPublisher
{
    private readonly HttpClient _httpClient;

    public StockRequestPublisher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> PublishStockCheckRequest(IEnumerable<OrderItem> orderItems)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "stock_check_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var stockCheckTasks = new List<Task<bool>>();

        foreach (var item in orderItems)
        {
            var stockCheckRequest = new StockCheckRequest
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            };

            var message = JsonSerializer.Serialize(stockCheckRequest);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: "stock_check_queue", basicProperties: null, body: body);

            // Vérifier la disponibilité du stock via l'API Produit
            stockCheckTasks.Add(CheckStockAvailability(item.ProductId, item.Quantity));
        }

        var stockResults = await Task.WhenAll(stockCheckTasks);
        return stockResults.All(result => result);
    }

    private async Task<bool> CheckStockAvailability(int productId, int quantity)
    {
        var response = await _httpClient.GetAsync($"http://localhost:5003/api/products/{productId}/stock?quantity={quantity}");
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StockCheckResponse>(jsonString);
            return result?.IsAvailable ?? false; 
        }
        return false; 
    }

    public class StockCheckRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class StockCheckResponse
    {
        public int ProductId { get; set; }
        public bool IsAvailable { get; set; }
    }
}
