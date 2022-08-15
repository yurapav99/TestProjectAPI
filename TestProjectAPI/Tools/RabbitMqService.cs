using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace TestProjectAPI.Tools
{
    public class RabbitMqService : IRabbitMqService
    {

        private  ConnectionFactory _factory { get; set; }

        public RabbitMqService() {
            _factory = new ConnectionFactory() { HostName = "localhost" };
        }

        public void SendMessage(string message)
        {
           
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "MyQueue",
                               durable: false,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                               routingKey: "MyQueue",
                               basicProperties: null,
                               body: body);
            }
        }
    }
}
