using System.IO;

namespace KVChecker_API
{
    class Program
    {

        static void Main(string[] args)
        {

            //Console.BufferWidth = 70;
            //Console.SetWindowSize(60, 25);
            if (!File.Exists("KV.log"))
            {
                File.WriteAllText("KV.log", "");
            }
            Server.Initialize();
        }
    }
}
