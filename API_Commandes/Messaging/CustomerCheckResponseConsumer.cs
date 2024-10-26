using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;
using System;

namespace API_Commandes.Messaging
{
    public interface ICustomerCheckResponseConsumer
    {
        Task<CustomerCheckResponse> WaitForCustomerCheckResponseAsync(int timeoutMilliseconds = 100000);
    }

    public class CustomerCheckResponseConsumer : ICustomerCheckResponseConsumer, IDisposable
    {
        private readonly IModel _channel;
        private TaskCompletionSource<CustomerCheckResponse> _responseTaskSource;

        public CustomerCheckResponseConsumer(IConnection connection)
        {
            _channel = connection.CreateModel();

            _channel.QueueDeclare(queue: "customer_check_response_queue",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, args) =>
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var response = JsonSerializer.Deserialize<CustomerCheckResponse>(message);

                _responseTaskSource?.SetResult(response);
            };

            _channel.BasicConsume(queue: "customer_check_response_queue", autoAck: true, consumer: consumer);
        }

        public async Task<CustomerCheckResponse> WaitForCustomerCheckResponseAsync(int timeoutMilliseconds = 100000)
        {
            _responseTaskSource = new TaskCompletionSource<CustomerCheckResponse>();

            var task = _responseTaskSource.Task;
            var timeoutTask = Task.Delay(timeoutMilliseconds);

            if (await Task.WhenAny(task, timeoutTask) == task)
            {
                return await task;
            }
            else
            {
                throw new TimeoutException("Customer check response timed out.");
            }
        }

        public void Dispose()
        {
            _channel?.Close();
        }
    }
}
