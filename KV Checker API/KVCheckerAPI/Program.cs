using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

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
