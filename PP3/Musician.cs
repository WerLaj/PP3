using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PP3
{
    class Musician
    {
        Send sender;
        Receive receiver;
        bool done;
        int numberOfNeighbors;
        int[,] neighbors;
        int[] position;
        String[] sendingQueue;
        String[] receivingQueue;


        public Musician(int n, int[,] _neighbors, int[] pos)
        {
            sender = new Send();
            receiver = new Receive();
            done = false;
            numberOfNeighbors = n;
            neighbors = _neighbors;
            position = pos;
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
                sendMessageToAllNeighborsAndReceive();
            }
        }

        public void sendMessageToAllNeighborsAndReceive()
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
            Thread.Sleep(3000);
        
            for (int i = 0; i < numberOfNeighbors; i++)
            {
                String rec = receiver.ReceiveMessage(receivingQueue[i]);
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
    }
}
