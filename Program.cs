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

            int objectId;
            int nameId;
            short posX;
            short posY;

            bool IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] shortBuffer = new byte[2];
            byte[] intBuffer = new byte[4];

            while (listening)
            {
                try
                {
                    Console.WriteLine("Trying to receive packets.");

                    byte[] bytes = receiveClient.Receive(ref receiveEndPoint);
                    MemoryStream stream = new MemoryStream(bytes);
                    reader = new BinaryReader(stream);

                    for(int i = 0; i < 4; i++) {
                        intBuffer[i] = reader.ReadByte();
                    }
                    if(IsLittleEndian) {
                        Array.Reverse(intBuffer);
                    }
                    packetId = BitConverter.ToInt32(intBuffer, 0);

                    if(packetId > lastPacketId) {
                        lastPacketId = packetId;

                        for(int i = 0; i < 2; i++) {
                            shortBuffer[i] = reader.ReadByte();
                        }
                        if(IsLittleEndian) {
                            Array.Reverse(shortBuffer);
                        }
                        objectAmount = BitConverter.ToInt16(shortBuffer, 0);

                        Console.WriteLine($"{receiveEndPoint}>> pakcet length: {bytes.Length}");
                        Console.WriteLine($"{receiveEndPoint}>> packetID:      {packetId}");
                        Console.WriteLine($"{receiveEndPoint}>> object amount: {objectAmount}");

                        for(int i = 0; i < objectAmount; i++) {
                            // Object ID
                            for(int j = 0; j < 4; j++) {
                                intBuffer[j] = reader.ReadByte();
                            }
                            if(IsLittleEndian) {
                                Array.Reverse(intBuffer);    
                            }
                            objectId = BitConverter.ToInt32(intBuffer, 0);
                            // Name ID
                            for(int j = 0; j < 4; j++) {
                                intBuffer[j] = reader.ReadByte();
                            }
                            if(IsLittleEndian) {
                                Array.Reverse(intBuffer);    
                            }
                            nameId = BitConverter.ToInt32(intBuffer, 0);
                            // Position X and Y
                            for(int j = 0; j < 2; j++) {
                                shortBuffer[j] = reader.ReadByte();
                            }
                            if(IsLittleEndian) {
                                Array.Reverse(shortBuffer);
                            }
                            posX = BitConverter.ToInt16(shortBuffer, 0);

                            for(int j = 0; j < 2; j++) {
                                shortBuffer[j] = reader.ReadByte();
                            }
                            if(IsLittleEndian) {
                                Array.Reverse(shortBuffer);
                            }
                            posY = BitConverter.ToInt16(shortBuffer, 0);

                            Console.WriteLine($"{receiveEndPoint}>> objectID: {objectId}");
                            Console.WriteLine($"{receiveEndPoint}>> nameID:   {nameId}");
                            Console.WriteLine($"{receiveEndPoint}>> posX:     {posX}");
                            Console.WriteLine($"{receiveEndPoint}>> posY:     {posY}\n");
                        }

                    } else {
                        Console.WriteLine("Old packet dropped.");
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
