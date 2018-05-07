using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;

namespace PP3
{
    class Send
    {
        public void SendMessage(String q, String m)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: q, durable: false, exclusive: false, autoDelete: false, arguments: null);

                string message = m;
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: q, basicProperties: null, body: body);
                //Console.WriteLine(" [{0}] Sent {1}", q, message);

                /*channel.QueueDeclare(queue: "hello2", durable: false, exclusive: false, autoDelete: false, arguments: null);

                string message2 = "Hello World---------------------2!";
                var body2 = Encoding.UTF8.GetBytes(message2);

                channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body2);
                Console.WriteLine(" [x] Sent {0}", message2);*/
            }
            //Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadLine();
            //Thread.Sleep(1000);
        }
    }
}
