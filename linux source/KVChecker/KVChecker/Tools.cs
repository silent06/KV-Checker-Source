using System;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;
namespace KVChecker_API
{
    class Tools
    {
        public static string ArrayToString(byte[] Input)
        {
            return BitConverter.ToString(Input).Replace("-", "");
        }

        public static byte[] NetGetBytes(NetworkStream Stream, int OutputSize)
        {
            byte[] Output = new byte[OutputSize];
            Stream.Read(Output, 0x00, OutputSize);
            return Output;
        }

        public static bool NetGetBool(NetworkStream Stream)
        {
            return (Stream.ReadByte() > 0x00);
        }

        public static bool IsArrayNull(byte[] Input, int InputSize)
        {
            for (int i = 0x00; i < InputSize; i++)
            {
                if (Input[i] != 0x00)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsArraySame(byte[] Input1, byte[] Input2, int InputSize)
        {
            for (int i = 0x00; i < InputSize; i++)
            {
                if (Input2[i] != Input1[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static void ReverseArray(ref byte[] Array, uint Size)
        {
            for (int i = 0x00; i < Size / 0x02; i++)
            {
                byte High = Array[i];
                byte Low = Array[(Size - 0x01) - i];
                Array[i] = Low;
                Array[(Size - 0x01) - i] = High;
            }
        }
        public static string BytesToHexString(byte[] Buffer)
        {
            try
            {
                string str = "";
                for (int i = 0; i < Buffer.Length; i++)
                {
                    str = str + Buffer[i].ToString("X2");
                }
                return str;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return "";
        }


        public static void SHA1MGF(byte[] Input, uint InputOffset, uint InputSize, ref byte[] Output, uint OutputOffset, uint OutputSize)
        {
            if (OutputSize == 0x00)
            {
                return;
            }

            uint Counter = 0x00;
            while (OutputSize > 0x00)
            {
                SHA1 SHA = new SHA1Managed();
                SHA.TransformBlock(Input, (int)InputOffset, (int)InputSize, null, 0x00);
                SHA.TransformFinalBlock(BitConverter.GetBytes(Counter++).Reverse().ToArray(), 0x00, 0x04);

                uint Size = 0x14;
                if (OutputSize < 0x14)
                {
                    Size = (OutputSize & 0xFFFFFFFF);
                }

                for (int i = 0x00; i < Size; i++)
                {
                    Output[i + OutputOffset] = (byte)(Output[i + OutputOffset] ^ SHA.Hash[i]);
                }

                OutputSize = OutputSize - Size;
                OutputOffset = OutputOffset + Size;
            }
        }

        public static void SHA1Hash(byte[] Input, int InputOffset, int InputSize, ref byte[] Output, int OutputOffset, int OutputSize)
        {
            SHA1 SHA = new SHA1Managed();
            Buffer.BlockCopy(SHA.ComputeHash(Input.Skip(InputOffset).Take(InputSize).ToArray()), 0x00, Output, OutputOffset, OutputSize);
        }


        public static void AppendText(string str, ConsoleColor color)
        {
            StreamWriter log;
            try
            {
                string time = string.Format("{0:hh:mm:ss tt}", DateTime.Now.ToUniversalTime().ToLocalTime());
                Console.ForegroundColor = color;
                Console.WriteLine(string.Concat(new object[] { "[", time, "]", " ", str, "" }));
                if (!File.Exists("KV.log")) { log = new StreamWriter("KV.log"); } else { log = File.AppendText("KV.log"); }
                log.WriteLine(string.Concat(new object[] { "[", time, "]", " ", str, "" })); log.Close();
            }
            catch
            {
                string time = string.Format("{0:hh:mm:ss tt}", DateTime.Now.ToUniversalTime().ToLocalTime());
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine(string.Concat(new object[] { "[", time, "]", "An Error Has Occured" }));
                 if (!File.Exists("KV.log")){log = new StreamWriter("KV.log");}else{log = File.AppendText("KV.log");}
                 log.WriteLine(string.Concat(new object[] { "[", time, "]", "An Error Has Occured" }));log.Close();
            }
        }

    }
}
