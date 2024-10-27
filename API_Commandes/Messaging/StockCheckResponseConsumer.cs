using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System;
using Prometheus;

namespace API_Commandes.Messaging
{
    public interface IStockCheckResponseConsumer
    {
        Task<StockCheckResponse> WaitForStockCheckResponseAsync(int timeoutMilliseconds = 100000);
    }
    public class StockCheckResponseConsumer : IStockCheckResponseConsumer, IDisposable
    {
        private readonly IModel _channel;
        private static readonly Counter _messagesConsumed = Metrics.CreateCounter("stock_check_responses_received", "Total number of Stock Check responses received");

        private TaskCompletionSource<StockCheckResponse> _responseTaskSource; 

        public StockCheckResponseConsumer(IConnection connection)
        {
            _channel = connection.CreateModel();

            // Declare a queue for stock check responses
            _channel.QueueDeclare(queue: "stock_check_response_queue",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            // Create a consumer to listen for messages from the queue
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, args) =>
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var response = JsonSerializer.Deserialize<StockCheckResponse>(message);

                // Complete the task with the stock check response
                _responseTaskSource?.SetResult(response);
                _messagesConsumed.Inc();
            };

            _channel.BasicConsume(queue: "stock_check_response_queue", autoAck: true, consumer: consumer);
        }

        public async Task<StockCheckResponse> WaitForStockCheckResponseAsync(int timeoutMilliseconds = 100000)
        {
            // Create a TaskCompletionSource that will be completed when a stock response is received
            _responseTaskSource = new TaskCompletionSource<StockCheckResponse>();

            // Get the Task from the TaskCompletionSource
            var task = _responseTaskSource.Task;
            var timeoutTask = Task.Delay(timeoutMilliseconds); 

            // Wait for either the stock response task or the timeout task to complete
            if (await Task.WhenAny(task, timeoutTask) == task)
            {
                return await task; 
            }
            else
            {
                throw new TimeoutException("Stock check response timed out.");
            }
        }
        public void Dispose()
        {
            _channel?.Close();
        }
    }
}

