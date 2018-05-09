using System;
using System.IO;
using System.Linq;
using System.Threading;
using Messages;
using System.Collections.Generic;

namespace PP3
{
    class Program
    {
        public static Musician[] musicians;
        public static Thread[] musiciansThreads;

        public static void Main()
        {
            var lines = File.ReadLines("positions.txt");
            int i = 0;
            int numberOfMusicians = Int32.Parse(lines.First());
            int[,] positions = new int[numberOfMusicians,2];
            musicians = new Musician[numberOfMusicians];
            musiciansThreads = new Thread[numberOfMusicians];
            foreach (var l in lines)
            {
                if (i == 0)
                {
                    i++;
                }
                else
                {
                    positions[i - 1, 0] = Int32.Parse(l.ToString().Substring(0,1));
                    positions[i - 1, 1] = Int32.Parse(l.ToString().Substring(2, 1));
                    i++;
                }
            }
            Console.WriteLine("number of musicians: " + numberOfMusicians);

            for (i = 0; i < numberOfMusicians; i++)
            {
                int[,] neighbors = new int[numberOfMusicians, 2];
                int k = 0;
                int value = randomValue();
                List<string> sendingQueue = new List<string>();
                List<string> receivingQueue = new List<string>();
                int[] pos = { positions[i, 0], positions[i, 1] };
                //Console.WriteLine(pos[0] + ", " + pos[1]);

                for (int j = 0; j < numberOfMusicians; j++)
                {
                    if (pos[0] != positions[j, 0] || pos[1] != positions[j, 1])
                    {
                        double dist = Math.Sqrt(Math.Pow(pos[0] - positions[j, 0], 2) + Math.Pow(pos[1] - positions[j, 1], 2));
                        if (dist <= 3)
                        {
                            neighbors[k, 0] = positions[j, 0];
                            neighbors[k, 1] = positions[j, 1];
                            sendingQueue.Add(pos[0].ToString() + pos[1].ToString() + neighbors[k, 0].ToString() + neighbors[k, 1].ToString());
                            receivingQueue.Add(neighbors[k, 0].ToString() + neighbors[k, 1].ToString() + pos[0].ToString() + pos[1].ToString());
                            //Console.WriteLine(sendingQueue[k]);
                            //Console.WriteLine(receivingQueue[k]);
                            k++;
                        }
                    }
                }
                musicians[i] = new Musician(k, neighbors, pos, sendingQueue, receivingQueue, value);
            }

            for (i = 0; i < numberOfMusicians; i++)
            {
                
                musiciansThreads[i] = new Thread(musicians[i].dowork);
                musiciansThreads[i].Start();
            }

            Console.ReadKey();

            /*Receive r = new Receive("123");
            Receive.Main();

            Send s = new Send("123", "-loser");
            Send.Main();
            //s.send();
            //s = new Send("123", "winner");
            Send.Main();
            //s.send();
            s = new Send("123", "-");
            Send.Main();
            //s.send();

            Receive.Main();

            //r.recv();

            Console.ReadKey();*/
        }

        public static int randomValue()
        {
            int rand = 0;
            Random rn = new Random();
            rand = rn.Next(1000);

            return rand;
        }
    }
}
