using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace PP3
{
    class Program
    {
        public static Musician[] musicians;
        public static Thread[] musiciansThreads;

        public static void Main(string[] args)
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
                for (int j = 0; j < numberOfMusicians; j++)
                {
                    if (positions[i, 0] != positions[j, 0] || positions[i, 1] != positions[j, 1])
                    {
                        double dist = Math.Sqrt(Math.Pow(positions[i, 0] - positions[j, 0], 2) + Math.Pow(positions[i, 1] - positions[j, 1], 2));
                        if (dist <= 3)
                        {
                            neighbors[k, 0] = positions[j, 0];
                            neighbors[k, 1] = positions[j, 1];
                            k++;
                        }
                    }
                }
                int[] pos = { positions[i, 0], positions[i, 1] };
                musicians[i] = new Musician(k, neighbors, pos);
            }

            for (i = 0; i < numberOfMusicians; i++)
            {
                
                musiciansThreads[i] = new Thread(musicians[i].dowork);
                musiciansThreads[i].Start();
            }

            /*Send s = new Send();
            s.SendMessage("hello", "Hello world!!!!!!!!");
            Receive r = new Receive();
            r.ReceiveMessage("hello");*/

            Console.ReadKey();
        }
    }
}
