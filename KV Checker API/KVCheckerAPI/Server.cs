using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Linq;
using System.Windows.Forms;



namespace KVChecker_API
{

    class Server
    {


        private static void HandleRequest(TcpClient Client, NetworkStream Stream, string RemoteIP)
        {

            try
            {
                File.WriteAllText("KV.log", String.Empty);
                byte[] KVFile = File.ReadAllBytes("bin\\kv.bin");
                Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> KV Request >> ", ConsoleColor.Green);
                KVChecker kvchecker = new KVChecker();
                kvchecker.getStatus(KVFile);
                int num = kvchecker.returnStatus(); 
                if (num == 0)
                {
                    Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> KV Unbanned! >> ", ConsoleColor.Green);
                }
                else if(num == 1)
                {
                    Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> KV Banned! >> ", ConsoleColor.Green);
                }
                else if (num == 2)
                {

                    Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> KV Banned! >> ", ConsoleColor.Green);
                }
                File.Delete("bin\\kv.bin");
                Client.Client.Close();
                return;
            }
            catch (Exception)
            {
                //Tools.AppendText(ex.Message, ConsoleColor.Red);
                Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> Failed to Read KV.bin >> ", ConsoleColor.Red);
                Client.Client.Close();
                return;
            }
        }

        private static void AcceptClient(TcpListener Listener)
        {
            try
            {

                while (true)
                {
                    while (!Listener.Pending())
                    {
                        Thread.Sleep(1);
                    }
                    TcpClient Client = Listener.AcceptTcpClient();
                    NetworkStream Stream = Client.GetStream();
                    string RemoteIP = Client.Client.RemoteEndPoint.ToString().Split(':')[0x00];

                    HandleRequest(Client, Stream, RemoteIP);
                }
            }
            catch
            {
                Console.WriteLine("API - Unable to listen for incoming requests\n");
                Listener.Server.Close();
            }
        }
        static int ServerPort;
        public enum CONFIG_VALUES : int
        {
            PORT,
        }
        public static void Initialize()
        {
            if (!File.Exists("bin/config.ini"))
            {
                File.WriteAllText("bin/config.ini", "" + 5000);
            }
            string[] config = File.ReadAllLines("bin/config.ini");
            ServerPort = int.Parse(config[(int)CONFIG_VALUES.PORT]);
            try
            {
                TcpListener Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
                Listener.Start();
                Console.WriteLine("API started & listening on Port: {0} ", ServerPort);
                new Thread(new ThreadStart(() => AcceptClient(Listener))).Start();
            }
            catch
            {
                Console.WriteLine("API - Failed to open socket\n");
                return;
            }
        }
    }
}
