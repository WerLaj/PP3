using Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PP3
{
    class Musician
    {
        int numberOfNeighbors;
        int[,] neighbors;
        int[] position;
        List<string> sendingQueue;
        List<string> receivingQueue;
        int randomValue;
        static int max;

        public Musician(int n, int[,] neigh, int[] pos, List<string> send, List<string> recv, int v)
        {
            numberOfNeighbors = n;
            neighbors = neigh;
            position = pos;
            sendingQueue = send;
            receivingQueue = recv;
            randomValue = v;
            max = 0;
        }

        public void dowork()
        {
            foreach (string r in receivingQueue)
            {
                receive(r);
            }
            foreach (string s in sendingQueue)
            {
                send(s, Message.number + randomValue.ToString());
            }
        }

        public void send(string queue, string message)
        {
            //Console.WriteLine(" Sender");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                string m = message;
                var body = Encoding.UTF8.GetBytes(m);

                channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
                Console.WriteLine(" [{0}] Sent {1}", queue, m);
            }
        }

        public void receive(string queue)
        {
            //Console.WriteLine(" Receiver");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    handleIncomingMessages(message, queue);
                    Console.WriteLine(" [{0}] Received {1}", queue, message);
                };
                channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
                //Thread.Sleep(100);
            }
        }

        public static void handleIncomingMessages(string m, string q)
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
            else if (m.Contains(Message.number))
            {
                int n = Int32.Parse(m.Substring((Message.number).Length));
                if (n > max)
                    max = n;
            }
        }
    }
}

/*Send sender;
        Receive receiver;
        bool done;
        bool isPlaying;
        int numberOfNeighbors;
        int[,] neighbors;
        int[] position;
        String[] sendingQueue;
        String[] receivingQueue;
        int diameter;
        bool winner;
        bool looser;

        public Musician(int n, int[,] _neighbors, int[] pos, int _diameter)
        {
            sender = new Send();
            receiver = new Receive();
            done = false;
            isPlaying = false;
            numberOfNeighbors = n;
            neighbors = _neighbors;
            position = pos;
            diameter = _diameter;
            winner = false;
            looser = false;
            sendingQueue = new String[numberOfNeighbors];
            receivingQueue = new String[numberOfNeighbors];
            Console.WriteLine("Musician: " + position[0] + "," + position[1]);
            for (int i = 0; i < numberOfNeighbors; i++)
            {
                Console.WriteLine(neighbors[i, 0] + ", " + neighbors[i, 1]);
            }
        }

        public void dowork()
        {
            //while(done == false)
            {
                //sendMessageToAllNeighborsAndReceive();
                lubyMIS();
            }
        }

        public void lubyMIS()
        {
            int value = randomValue();
            int max = 0;
            Console.WriteLine("Musician " + position[0] + ", " + position[1] + ": " + value);
            int[] receivedValues = new int[numberOfNeighbors];
            List<int> toremove = new List<int>();

            for (int i = 0; i < numberOfNeighbors; i++)
            {
                sendingQueue[i] = position[0].ToString() + position[1].ToString() + neighbors[i, 0].ToString() + neighbors[i, 1].ToString();
                receivingQueue[i] = neighbors[i, 0].ToString() + neighbors[i, 1].ToString() + position[0].ToString() + position[1].ToString();
            }

            sendMessagesToNeightbours(value);
            //Thread.Sleep(1000);

            for (int i = 0; i < numberOfNeighbors; i++)
            {
                String rec = "";
                while (rec == "")
                    rec = receiver.ReceiveMessage(receivingQueue[i]);
                receivedValues[i] = Int32.Parse(rec);
            }

            Thread.Sleep(1000);

            for (int i = 0; i < numberOfNeighbors; i++)
            {
                //Console.WriteLine("Musician " + position[0] + ", " + position[1] + " received values: " + receivedValues[i]);
                if (receivedValues[i] > max)
                    max = receivedValues[i];
            }

            Console.WriteLine("Musician " + position[0] + ", " + position[1] + " maxvalue: " + max);

            if (max <= value)
            {
                winner = true;
                looser = false;
                Console.WriteLine("----Musician " + position[0] + ", " + position[1] + " is a winner ");
                for (int i = 0; i < numberOfNeighbors; i++)
                {
                    sender.SendMessage(sendingQueue[i], "_winner");
                }
            }
            else
            {
                for (int i = 0; i < numberOfNeighbors; i++)
                {
                    sender.SendMessage(sendingQueue[i], "_");
                }
                winner = false;
                looser = false;
                for (int i = 0; i < numberOfNeighbors; i++)
                {
                    String rec = "";
                    while (!rec.Contains("_"))
                        rec = receiver.ReceiveMessage(receivingQueue[i]);
                    if(rec == "_winner")
                    {
                        winner = false;
                        looser = true;
                        toremove.Add(i);
                    }
                }
                if (looser == true)
                {
                    for (int j = 0; j < numberOfNeighbors; j++)
                    {
                        if(!toremove.Contains(j))
                            sender.SendMessage(sendingQueue[j], "+loser");
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfNeighbors; i++)
                    {
                        sender.SendMessage(sendingQueue[i], "+");
                    }
                    for (int i = 0; i < numberOfNeighbors; i++)
                    {
                        String rec = "";
                        while (!rec.Contains("+"))
                            rec = receiver.ReceiveMessage(receivingQueue[i]);
                        if (rec == "+loser")
                        {
                            toremove.Add(i);
                        }
                    }
                    /////reduce neighbors
                }
            }

            if (winner == true)
                isPlaying = true;
            else
                isPlaying = false;



            //Console.WriteLine("Musician " + position[0] + ", " + position[1] + " maxvalue: " + max);
        }

        public void sendMessageToAllNeighborsAndReceive()
        {
            /*if(max <= value)
            {
                isPlaying = true;
                Console.WriteLine("----Musician " + position[0] + ", " + position[1] + " PLAYS THE CONCERT ");
                Thread.Sleep(2000);
                for (int i = 0; i < numberOfNeighbors; i++)
                {
                    sender.SendMessage(sendingQueue[i], "done");
                }
                done = true;
            }
            else
            {
                Console.WriteLine("----Musician " + position[0] + ", " + position[1] + " waits ");
                Thread.Sleep(2500);
                int k = 0;
                int[,] temp = new int[numberOfNeighbors, 2];
                for (int i = 0; i < numberOfNeighbors; i++)
                {
                    String rec = receiver.ReceiveMessage(receivingQueue[i]);
                    if(rec == "")
                    {
                        temp[k, 0] = neighbors[i, 0];
                        temp[k, 1] = neighbors[i, 1];
                        k++;                 
                    }
                }
                numberOfNeighbors = k;
                Console.WriteLine("----Musician " + position[0] + ", " + position[1] + " has: " + numberOfNeighbors + " neighbours ");
            }
}

public void sendMessagesToNeightbours(int value)
{
    for (int i = 0; i < numberOfNeighbors; i++)
    {
        sender.SendMessage(sendingQueue[i], value.ToString());
    }
}

public int randomValue()
{
    int rand = 0;
    Random rn = new Random();
    rand = rn.Next(1000);

    return rand;
}

/*public void sendMessageToAllNeighborsAndReceive()
{
    int value = randomValue();
    int max = 0;
    Console.WriteLine("Musician " + position[0] + ", " + position[1] + ": " + value);
    int[] receivedValues = new int[numberOfNeighbors];

    for (int i = 0; i < numberOfNeighbors; i++)
    {
        sendingQueue[i] = position[0].ToString() + position[1].ToString() + neighbors[i, 0].ToString() + neighbors[i, 1].ToString();
        receivingQueue[i] = neighbors[i, 0].ToString() + neighbors[i, 1].ToString() + position[0].ToString() + position[1].ToString();
    }

    sendMessagesToNeightbours(value);
    //Thread.Sleep(3000);

    for (int i = 0; i < numberOfNeighbors; i++)
    {
        String rec = "";
        while(rec == "")
            rec = receiver.ReceiveMessage(receivingQueue[i]);
        receivedValues[i] = Int32.Parse(rec);
    }

    for (int i = 0; i < numberOfNeighbors; i++)
    {
        //Console.WriteLine("Musician " + position[0] + ", " + position[1] + " received values: " + receivedValues[i]);
        if (receivedValues[i] > max)
            max = receivedValues[i];
    }
    Console.WriteLine("Musician " + position[0] + ", " + position[1] + " maxvalue: " + max);

    if(max <= value)
    {
        isPlaying = true;
        Console.WriteLine("----Musician " + position[0] + ", " + position[1] + " PLAYS THE CONCERT ");
        Thread.Sleep(2000);
        for (int i = 0; i < numberOfNeighbors; i++)
        {
            sender.SendMessage(sendingQueue[i], "done");
        }
        done = true;
    }
    else
    {
        Console.WriteLine("----Musician " + position[0] + ", " + position[1] + " waits ");
        Thread.Sleep(2500);
        int k = 0;
        int[,] temp = new int[numberOfNeighbors, 2];
        for (int i = 0; i < numberOfNeighbors; i++)
        {
            String rec = receiver.ReceiveMessage(receivingQueue[i]);
            if(rec == "")
            {
                temp[k, 0] = neighbors[i, 0];
                temp[k, 1] = neighbors[i, 1];
                k++;                 
            }
        }
        numberOfNeighbors = k;
        Console.WriteLine("----Musician " + position[0] + ", " + position[1] + " has: " + numberOfNeighbors + " neighbours ");
    }
}

public void sendMessagesToNeightbours(int value)
{
    for (int i = 0; i < numberOfNeighbors; i++)
    {
        sender.SendMessage(sendingQueue[i], value.ToString());
    }
}*/
