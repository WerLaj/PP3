using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace PP3
{
    class Receive
    {
        public String ReceiveMessage(String q)
        {
            String message = "";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: q, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [{0}] Received {1}", q, message);
                };
                channel.BasicConsume(queue: q, autoAck: true, consumer: consumer);
                //channel.BasicConsume(queue: "hello2", autoAck: true, consumer: consumer);
                //Console.WriteLine(" Press [enter] to exit.");
                //Console.ReadLine();
                //Thread.Sleep(500);
            }
            return message;
        }
    }
}
