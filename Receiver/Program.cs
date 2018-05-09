using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Messages;

namespace Receiver
{
    public class Receive
    {
        public static string queue;

        public Receive(string q)
        {
            queue = q;
        }

        public static void Main()
        {
            Console.WriteLine(" Receiver");
            String message = "";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    message = Encoding.UTF8.GetString(body);
                    handleIncomingMessages(message, queue);
                    Console.WriteLine(" [{0}] Received {1}", queue, message);
                };
                channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
                Thread.Sleep(100);
            }
        }

        public static void handleIncomingMessages(string m, string channel)
        {
            if (m == Message.isPlaying)
            {
                Console.WriteLine("---Someone is playing");
            }
            else if (m == Message.winner)
            {
                Console.WriteLine("---Someone is winner");
            }
            else if (m == Message.loser)
            {
                Console.WriteLine("---Someone is loser");
            }
        }
    }
}
