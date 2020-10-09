using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

namespace vscProjects
{
    class Program
    {
        int listeningPort = 55555;
        bool listening;
        Thread listeningThread;
        UdpClient receiveClient;
        IPEndPoint receiveEndPoint;

        int lastPacketId = 0;

        public Program() 
        {
            listening = true;

            receiveClient = new UdpClient(listeningPort);
            receiveEndPoint = new IPEndPoint(IPAddress.Any, listeningPort);
            Console.WriteLine("Listening on: " + receiveEndPoint.ToString());

            listeningThread = new Thread(new ThreadStart(FetchUpdates));
            listeningThread.Start();    
        }

        public void FetchUpdates()
        {
            BinaryReader reader;
            int packetId;
            short objectAmount = -1;

            byte[] idBuffer = new byte[4];
            byte[] amountBuffer = new byte[2];

            while (listening)
            {
                try
                {
                    Console.WriteLine("Trying to receive packets.");

                    byte[] bytes = receiveClient.Receive(ref receiveEndPoint);
                    MemoryStream stream = new MemoryStream(bytes);
                    reader = new BinaryReader(stream);

                    for(int i = 0; i < 4; i++) {
                        idBuffer[i] = reader.ReadByte();
                    }
                    if(BitConverter.IsLittleEndian) {
                        Array.Reverse(idBuffer);
                    }
                    packetId = BitConverter.ToInt32(idBuffer, 0);

                    if(packetId > lastPacketId) {
                        lastPacketId = packetId;

                        for(int i = 0; i < 2; i++) {
                            amountBuffer[i] = reader.ReadByte();
                        }
                        if(BitConverter.IsLittleEndian) {
                            Array.Reverse(amountBuffer);
                        }
                        objectAmount = BitConverter.ToInt16(amountBuffer, 0);

                        for(int i = 0; i < objectAmount; i++) {
                            Console.WriteLine("Object " + i + " created!");
                        }

                    } else {
                        Console.WriteLine("Old packet dropped.");
                    }

                    Console.WriteLine($"{receiveEndPoint}>> {bytes.Length}");
                    Console.WriteLine($"{receiveEndPoint}>> {packetId}");
                    if(objectAmount > 0) {
                        Console.WriteLine($"{receiveEndPoint}>> {objectAmount}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("Stopped.");
        }
   
        static void Main(string[] args)
        {
            Program program = new Program();
        }
    }
}
