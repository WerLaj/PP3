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
        int roundNumberOfNeighbors;
        int[,] neighbors;
        int[,] roundNeighbors;
        int[] position;
        List<string> sendingQueue;
        List<string> receivingQueue;
        List<string> roundSendingQueue;
        List<string> winners;
        int randomValue;
        int max = 0;
        ConnectionFactory factory;
        IConnection connection;
        IModel inChannel;
        IModel outChannel;
        int stateN = 0, stageW = 0, stageL = 0, round = 0;
        bool winner = false, loser = false, done = false;

        public Musician(int n, int[,] neigh, int[] pos, List<string> send, List<string> recv, int v, ConnectionFactory f, IConnection c, IModel chI, IModel chO)
        {
            numberOfNeighbors = n;
            roundNumberOfNeighbors = n;
            neighbors = neigh;
            roundNeighbors = neigh;
            position = pos;
            sendingQueue = send;
            receivingQueue = recv;
            roundSendingQueue = send;
            randomValue = v;
            factory = f;
            connection = c;
            inChannel = chI;
            outChannel = chO;
            winners = new List<string>();
            Console.WriteLine("[{0}, {1}] : {2}; neigh = {3}", position[0], position[1], randomValue, numberOfNeighbors);
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
            //var factory = new ConnectionFactory() { HostName = "localhost" };
            //using (var connection = factory.CreateConnection())
            //using (var channel = connection.CreateModel())
            {
                lock (outChannel)
                {
                    outChannel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
                }

                var body = Encoding.UTF8.GetBytes(message);
                Console.WriteLine(" [{0}] Sent {1}", queue, message); 
                lock (outChannel)
                {
                    outChannel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
                }
               
            }
        }

        public void receive(string queue)
        {
            //Console.WriteLine(" Receiver");
            //var factory = new ConnectionFactory() { HostName = "localhost" };
            //using (var connection = factory.CreateConnection())
            //using (var channel = connection.CreateModel())
            {
                lock (inChannel)
                {
                    inChannel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
                }
                var consumer = new EventingBasicConsumer(inChannel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [{0}] Received {1}", queue, message);
                    handleIncomingMessages(message, queue);
                    
                };
                lock (inChannel)
                {
                    inChannel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
                }
                //Thread.Sleep(100);
            }
        }

        public void handleIncomingMessages(string m, string q)
        {
            if (done == false)
            {
                if (m == Message.notwinner)
                {
                    stageW++;
                    //Console.WriteLine("[{0}, {1}] {2}, {3} is NOT winner", position[0], position[1], q.Substring(0, 1), q.Substring(1, 1));
                }
                else if (m == Message.winner)
                {
                    if (loser == true) { }//!!!!!!!!!!!!!!!!!!!!!!!!!-
                    stageW++;
                    Console.WriteLine("[{0}, {1}] {2}, {3} is winner. I'm loser", position[0], position[1], q.Substring(0, 1), q.Substring(1, 1));
                    winners.Add(q.Substring(0, 2));
                    loser = true;
                }
                else if (m == Message.notloser)
                {
                    stageL++;
                    //Console.WriteLine("[{0}, {1}] {2}, {3} is NOT loser", position[0], position[1], q.Substring(0, 1), q.Substring(1, 1));
                }
                else if (m == Message.loser)
                {
                    stageL++;
                    Console.WriteLine("[{0}, {1}] {2}, {3} is loser", position[0], position[1], q.Substring(0, 1), q.Substring(1, 1));
                    if (loser == false)
                    {
                        //roundSendingQueue.Remove(q);
                        removeOneNeighbor(roundNeighbors, Int32.Parse(q.Substring(0, 1)), Int32.Parse(q.Substring(1, 1)));
                        roundNumberOfNeighbors--;
                    }
                }
                else if (m.Contains(Message.number))
                {
                    int n = Int32.Parse(m.Substring((Message.number).Length));
                    stateN++;
                    if (n > max)
                        max = n;
                }
                else if (m == Message.finished)
                {
                    roundSendingQueue.Remove(q + position[0] + position[1]);
                    removeOneNeighbor(roundNeighbors, Int32.Parse(q.Substring(0, 1)), Int32.Parse(q.Substring(1, 1)));
                    sendingQueue.Remove(q + position[0] + position[1]);
                    removeOneNeighbor(neighbors, Int32.Parse(q.Substring(0, 1)), Int32.Parse(q.Substring(1, 1)));
                    numberOfNeighbors--;
                    roundNumberOfNeighbors--;
                    winners.Remove(q.Substring(0, 2));

                    if (winners.Count == 0)
                    {
                        if (roundNumberOfNeighbors == 0)
                            winner = true;
                        else
                            foreach (string s in roundSendingQueue)
                            {
                                send(s, Message.number + randomValue.ToString());
                            }
                    }
                    loser = false;
                }

                /*if(loser == false && winner == false)
                {
                    if (roundNumberOfNeighbors == 0)
                        winner = true;
                    else
                        foreach (string s in roundSendingQueue)
                        {
                            send(s, Message.number + randomValue.ToString());
                        }
                } */

                if (stateN == roundNumberOfNeighbors && loser == false)
                {
                    if (max <= randomValue)
                    {
                        Console.WriteLine("[{0}, {1}] I'm winner", position[0], position[1]);
                        foreach (string s in sendingQueue)
                        {
                            send(s, Message.winner);
                            winner = true;
                        }
                    }
                    else
                    {
                        //Console.WriteLine("{0}, {1} I'm NOT winner", position[0], position[1]);
                        foreach (string s in roundSendingQueue)
                        {
                            send(s, Message.notwinner);
                        }
                    }
                    stateN = 0;
                    max = 0;
                    round++;
                }

                if (stageW == roundNumberOfNeighbors && round == 1)
                {
                    if (loser == true)
                    {
                        foreach (string s in roundSendingQueue)
                        {
                            send(s, Message.loser);
                        }
                    }
                    else
                    {
                        foreach (string s in roundSendingQueue)
                        {
                            send(s, Message.notloser);
                        }
                    }
                    round++;
                    stageW = 0;
                }
                
                if(stageL == roundNumberOfNeighbors && round == 2)
                {
                    if(loser == false)
                    {
                        if (roundNumberOfNeighbors == 0)
                            winner = true;
                        else
                            foreach (string s in roundSendingQueue)      
                            {
                                send(s, Message.number + randomValue.ToString());
                            }
                    }
                    stageL = 0;
                    round = 0;
                }

                if (winner == true && winners.Count == 0)
                {
                    Console.WriteLine("[{0}, {1}] ----------- PLAYING -----------", position[0], position[1]);
                    Thread.Sleep(5000);
                    Console.WriteLine("[{0}, {1}] ----------- FINISHED ----------", position[0], position[1]);
                    foreach (string s in roundSendingQueue)
                    {
                        send(s, Message.finished);
                    }
                    done = true;
                }
            }
        }

        public int[,] removeOneNeighbor(int[,] neigh, int pos1, int pos2)
        {
            int len = (int)neigh.Length/2;
            int[,] newNeigh = new int[len, 2];
            int k = 0;
            for(int i = 0; i < len; i++)
            {
                if(neigh[i,0] != pos1 || neigh[i, 1] != pos2)
                {
                    newNeigh[k, 0] = neigh[i, 0];
                    newNeigh[k, 1] = neigh[i, 1];
                    k++;
                }
            }
            return newNeigh;
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
