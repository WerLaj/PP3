using RabbitMQ.Client;
using System;
using System.Text;

namespace Sender
{
    public class Send
    {
        public static string queue;
        public static string message;

        public Send(string q, string m)
        {
            queue = q;
            message = m;
        }

        public static void Main()
        {
            Console.WriteLine(" Sender");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                string m = message;
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
                Console.WriteLine(" [{0}] Sent {1}", queue, m);
            }            
        }
    }
}
