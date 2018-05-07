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
        bool isPlaying;
        int numberOfNeighbors;
        int[,] neighbors;
        int[] position;
        String[] sendingQueue;
        String[] receivingQueue;


        public Musician(int n, int[,] _neighbors, int[] pos)
        {
            sender = new Send();
            receiver = new Receive();
            isPlaying = false;
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

        public void sendMessageToAllNeighborsAndReceive()
        {
            int value = randomValue();
            Console.WriteLine("Musician " + position[0] + ", " + position[1] + ": " + value);
            int[] receivedValues = new int[numberOfNeighbors];

            for (int i = 0; i < numberOfNeighbors; i++)
            {
                sendingQueue[i] = position[0].ToString() + position[1].ToString() + neighbors[i, 0].ToString() + neighbors[i, 1].ToString();
                receivingQueue[i] = neighbors[i, 0].ToString() + neighbors[i, 1].ToString() + position[0].ToString() + position[1].ToString();

                sender.SendMessage(sendingQueue[i], value.ToString());

                //Thread.Sleep(1000);
            }

            Thread.Sleep(3000);
        
            for (int i = 0; i < numberOfNeighbors; i++)
            {
                String rec = receiver.ReceiveMessage(receivingQueue[i]);
                Console.WriteLine(rec);

                receivedValues[i] = Int32.Parse(rec);
            }

            int max = 0;
            for (int i = 0; i < numberOfNeighbors; i++)
            {
                Console.WriteLine("Musician " + position[0] + ", " + position[1] + " received values: " + receivedValues[i]);
                if (receivedValues[i] > max)
                    max = receivedValues[i];
            }
            Console.WriteLine("Musician " + position[0] + ", " + position[1] + " maxvalue: " + max);
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
