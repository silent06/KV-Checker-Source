using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Core;

namespace KVChecker_API
{
    // Token: 0x02000004 RID: 4
    internal class KVChecker
    {
        // Token: 0x0600002E RID: 46 RVA: 0x0000677C File Offset: 0x0000497C
        private static byte[] ComputeClientName(byte[] ConsoleId)
        {
            byte[] result;
            try
            {
                long num = 0L;
                for (int i = 0; i < 5; i++)
                {
                    num |= (long)((ulong)ConsoleId[i]);
                    num <<= 8;
                }
                long num2 = num >> 8;
                int num3 = (int)num2 & 15;
                string text = string.Format("XE.{0}{1}@xbox.com", (num2 >> 4).ToString(), num3.ToString());
                bool flag = text.Length != 24;
                if (flag)
                {
                    int length = text.Length;
                    for (int j = 0; j < 24 - (text.Length - 1); j++)
                    {
                        text = text.Insert(3, "0");
                    }
                }
                result = Encoding.ASCII.GetBytes(text);
            }
            catch
            {
                result = ConsoleId;
            }
            return result;
        }



        // Token: 0x0600002F RID: 47 RVA: 0x0000685C File Offset: 0x00004A5C
        private static byte[] ComputeKdcNoonce(byte[] Key, int keyLen)
        {
            byte[] buffer = new byte[]
            {
                115,
                105,
                103,
                110,
                97,
                116,
                117,
                114,
                101,
                107,
                101,
                121,
                0
            };
            byte[] key = new HMACMD5(Key).ComputeHash(buffer, 0, 13);
            byte[] inputBuffer = new byte[4];
            byte[] array = new byte[4];
            array[0] = 2;
            array[1] = 4;
            byte[] inputBuffer2 = array;
            MD5 md = new MD5CryptoServiceProvider();
            md.TransformBlock(inputBuffer2, 0, 4, null, 0);
            md.TransformFinalBlock(inputBuffer, 0, 4);
            byte[] hash = md.Hash;
            return new HMACMD5(key).ComputeHash(hash);
        }

        // Token: 0x06000030 RID: 48 RVA: 0x000068E0 File Offset: 0x00004AE0
        private static byte[] GenerateTimeStamp()
        {
            byte[] array = Misc.HexStringToBytes("301aa011180f32303132313231323139303533305aa10502030b3543");
            Encoding ascii = Encoding.ASCII;
            string s = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssZ");
            Array.Copy(ascii.GetBytes(s), 0, array, 6, 15);
            return array;
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00006934 File Offset: 0x00004B34
        public void getStatus(byte[] str2)
        {
            try
            {
                DateTime now = DateTime.Now;
                //KVChecker.currentFile = str2;
                byte[] xmacsLogonKey = KVChecker.GetXmacsLogonKey(str2);
                bool flag = xmacsLogonKey == null;
                if (flag)
                {
                    xmacsLogonKey = KVChecker.GetXmacsLogonKey(str2);
                    bool flag2 = xmacsLogonKey == null;
                    if (flag2)
                    {
                        this.banned = 2;
                        return;
                    }
                }
                EndianIO endianIO = new EndianIO(str2, EndianStyle.BigEndian);
                endianIO.Reader.BaseStream.Position = 2506L;
                byte[] consoleId = endianIO.Reader.ReadBytes(5);
                endianIO.Reader.BaseStream.Position = 2504L;
                byte[] sourceArray = SHA1.Create().ComputeHash(endianIO.Reader.ReadBytes(168));
                byte[] array = File.ReadAllBytes(KVChecker.AppDataDir + "apReq1.bin");
                byte[] sourceArray2 = KVChecker.ComputeClientName(consoleId);
                Array.Copy(sourceArray2, 0, array, 258, 24);
                Array.Copy(sourceArray, 0, array, 36, 20);
                byte[] array2 = KVChecker.GenerateTimeStamp();
                Array.Copy(KVChecker.RC4HMACEncrypt(xmacsLogonKey, 16, array2, array2.Length, 1), 0, array, 176, 52);
                UdpClient udpClient = new UdpClient();                
                udpClient.Connect("XEAS.XBOXLIVE.COM", 88);
                Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> Connecting to XEAS.XBOXLIVE.COM >> ", ConsoleColor.Green);
                udpClient.Send(array, array.Length);
                IPEndPoint ipendPoint = new IPEndPoint(0L, 0);
                int num = 0;
                byte[] array3;
                for (;;)
                {
                    try
                    {
                        Thread.Sleep(10);
                        bool flag3 = udpClient.Available > 0;
                        if (flag3)
                        {
                            array3 = udpClient.Receive(ref ipendPoint);
                            break;
                        }
                        bool flag4 = num >= 5;
                        if (flag4)
                        {
                            this.banned = 2;
                            return;
                        }
                        Thread.Sleep(50);
                        udpClient.Send(array, array.Length);
                        num++;
                    }
                    catch
                    {
                        this.banned = 2;
                        return;
                    }
                }
                udpClient.Close();
                byte[] array4 = new byte[16];
                Array.Copy(array3, array3.Length - 16, array4, 0, 16);
                byte[] array5 = File.ReadAllBytes(KVChecker.AppDataDir + "apReq2.bin");
                Array.Copy(sourceArray2, 0, array5, 286, 24);
                Array.Copy(sourceArray, 0, array5, 36, 20);
                byte[] array6 = KVChecker.GenerateTimeStamp();
                Array.Copy(KVChecker.RC4HMACEncrypt(xmacsLogonKey, 16, array6, array6.Length, 1), 0, array5, 204, 52);
                Array.Copy(array4, 0, array5, 68, 16);
                UdpClient udpClient2 = new UdpClient();
                udpClient2.Connect("XEAS.XBOXLIVE.COM", 88);
                Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> Connecting to XEAS.XBOXLIVE.COM >> ", ConsoleColor.Green);
                udpClient2.Send(array5, array5.Length);
                num = 0;
                byte[] array7;
                for (;;)
                {
                    try
                    {
                        Thread.Sleep(10);
                        bool flag5 = udpClient2.Available > 0;
                        if (flag5)
                        {
                            array7 = udpClient2.Receive(ref ipendPoint);
                            break;
                        }
                        bool flag6 = num >= 5;
                        if (flag6)
                        {
                            this.banned = 2;
                            return;
                        }
                        Thread.Sleep(50);
                        udpClient2.Send(array5, array5.Length);
                        num++;
                    }
                    catch
                    {
                        this.banned = 2;
                        return;
                    }
                }
                udpClient2.Close();
                File.WriteAllBytes(KVChecker.AppDataDir + "APRESP.bin", array7);
                byte[] array8 = new byte[210];
                Array.Copy(array7, array7.Length - 210, array8, 0, 210);
                byte[] array9 = KVChecker.RC4HMACDecrypt(xmacsLogonKey, 16, array8, 210, 8);
                byte[] array10 = new byte[16];
                File.WriteAllBytes(KVChecker.AppDataDir + "test.bin", array9);
                Array.Copy(array9, 27, array10, 0, 16);
                byte[] array11 = new byte[345];
                Array.Copy(array7, 168, array11, 0, 345);
                byte[] array12 = File.ReadAllBytes(KVChecker.AppDataDir + "TGSREQ.bin");
                Array.Copy(array11, 0, array12, 437, 345);
                byte[] array13 = File.ReadAllBytes(KVChecker.AppDataDir + "authenticator.bin");
                Array.Copy(sourceArray2, 0, array13, 40, 15);
                Encoding ascii = Encoding.ASCII;
                string s = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssZ");
                Array.Copy(ascii.GetBytes(s), 0, array13, 109, 15);
                Array.Copy(MD5.Create().ComputeHash(array12, 954, 75), 0, array13, 82, 16);
                Array.Copy(KVChecker.RC4HMACEncrypt(array10, 16, array13, array13.Length, 7), 0, array12, 799, 153);
                byte[] key = KVChecker.ComputeKdcNoonce(array10, 16);
                byte[] array14 = File.ReadAllBytes(KVChecker.AppDataDir + "servicereq.bin");
                Array.Copy(KVChecker.RC4HMACEncrypt(key, 16, array14, array14.Length, 1201), 0, array12, 55, 150);
                byte[] array15 = new byte[66];
                Array.Copy(array5, 116, array15, 0, 66);
                Array.Copy(KVChecker.GetTitleAuthData(array10, 16, array15), 0, array12, 221, 82);
                UdpClient udpClient3 = new UdpClient();
                udpClient3.Connect("XETGS.XBOXLIVE.COM", 88);
                Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> Connecting to XETGS.XBOXLIVE.COM >> ", ConsoleColor.Green);
                udpClient3.Send(array12, array12.Length);
                ipendPoint = new IPEndPoint(0L, 0);
                num = 0;
                byte[] array16;
                for (;;)
                {
                    try
                    {
                        Thread.Sleep(10);
                        bool flag7 = udpClient3.Available > 0;
                        if (flag7)
                        {
                            array16 = udpClient3.Receive(ref ipendPoint);
                            break;
                        }
                        bool flag8 = num >= 5;
                        if (flag8)
                        {
                            this.banned = 2;
                            return;
                        }
                        Thread.Sleep(50);
                        udpClient3.Send(array12, array12.Length);
                        num++;
                    }
                    catch
                    {
                        this.banned = 2;
                        return;
                    }
                }
                File.WriteAllBytes(KVChecker.AppDataDir + "tgsres.bin", array16);
                byte[] array17 = new byte[84];
                Array.Copy(array16, 50, array17, 0, 84);
                byte[] value = KVChecker.RC4HMACDecrypt(key, 16, array17, 84, 1202);
                byte[] array18 = new byte[208];
                Array.Copy(array16, 58, array18, 0, 208);
                byte[] bytes = KVChecker.RC4HMACDecrypt(key, 16, array18, 208, 1202);
                File.WriteAllBytes(KVChecker.AppDataDir + "resp.bin", bytes);
                uint num2 = BitConverter.ToUInt32(value, 8);
                bool flag9 = (ulong)num2 == 18446744071563450637UL | num2 == 2148866317u;
                if (flag9)
                {
                    this.banned = 1;
                    Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> KV banned >> ", ConsoleColor.Green);
                }
                else
                {
                    this.banned = 0;
                    Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> KV Unbanned >> ", ConsoleColor.Green);
                }
                endianIO.Close();
                Thread.Sleep(200);
            }
            catch(Exception ex)
            {
                this.banned = 2;
                Tools.AppendText(ex.Message, ConsoleColor.Red);
            }

        }
 
        // Token: 0x06000032 RID: 50 RVA: 0x0000700C File Offset: 0x0000520C
        private static byte[] GetTitleAuthData(byte[] Key, int keyLen, byte[] titleData)
        {
            byte[] sourceArray = new HMACSHA1(KVChecker.ComputeKdcNoonce(Key, 16)).ComputeHash(titleData, 0, 66);
            byte[] array = new byte[82];
            Array.Copy(sourceArray, array, 16);
            Array.Copy(titleData, 0, array, 16, 66);
            return array;
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00007058 File Offset: 0x00005258
        private static byte[] GetXmacsLogonKey(byte[] fileName)
        {
            RSACryptoServiceProvider rsacryptoServiceProvider = KVChecker.LoadXmacsKey();
            byte[] array = new byte[16];
            new Random(Environment.TickCount).NextBytes(array);
            byte[] array2 = rsacryptoServiceProvider.Encrypt(array, true);
            Array.Reverse(array2);
            byte[] array3 = File.ReadAllBytes(KVChecker.AppDataDir + "XMACSREQ.bin");
            Array.Copy(array2, 0, array3, 44, 256);
            EndianIO endianIO = new EndianIO(fileName, EndianStyle.BigEndian)
            {
                Position = 176L
            };
            byte[] array4 = endianIO.Reader.ReadBytes(12);
            endianIO.Position = 2504L;
            byte[] sourceArray = endianIO.Reader.ReadBytes(424);
            endianIO.Position = 668L;
            byte[] exponent = endianIO.Reader.ReadBytes(4);
            endianIO.Position = 680L;
            byte[] keyParams = endianIO.Reader.ReadBytes(448);
            endianIO.Position = 2506L;
            byte[] consoleId = endianIO.Reader.ReadBytes(5);
            endianIO.Close();
            byte[] sourceArray2 = KVChecker.ComputeClientName(consoleId);
            RSACryptoServiceProvider key = KVChecker.LoadConsolePrivateKey(exponent, keyParams);
            byte[] bytes = BitConverter.GetBytes(DateTime.UtcNow.ToFileTime());
            Array.Reverse(bytes);
            byte[] array5 = KVChecker.GenerateTimeStamp();
            byte[] sourceArray3 = KVChecker.RC4HMACEncrypt(array, 16, array5, array5.Length, 1);
            byte[] inputBuffer = SHA1.Create().ComputeHash(array);
            SHA1CryptoServiceProvider sha1CryptoServiceProvider = new SHA1CryptoServiceProvider();
            sha1CryptoServiceProvider.TransformBlock(bytes, 0, 8, null, 0);
            byte[] array6 = new byte[16];
            try
            {
                sha1CryptoServiceProvider.TransformBlock(array4, 0, 12, null, 0);
                sha1CryptoServiceProvider.TransformFinalBlock(inputBuffer, 0, 20);
                byte[] hash = sha1CryptoServiceProvider.Hash;
                RSAPKCS1SignatureFormatter rsapkcs1SignatureFormatter = new RSAPKCS1SignatureFormatter(key);
                rsapkcs1SignatureFormatter.SetHashAlgorithm("SHA1");
                byte[] array7 = rsapkcs1SignatureFormatter.CreateSignature(hash);
                Array.Reverse(array7);
                Array.Copy(bytes, 0, array3, 300, 8);
                Array.Copy(array4, 0, array3, 308, 12);
                Array.Copy(array7, 0, array3, 320, 128);
                Array.Copy(sourceArray, 0, array3, 448, 424);
                Array.Copy(sourceArray3, 0, array3, 992, 52);
                Array.Copy(sourceArray2, 0, array3, 1072, 15);
                UdpClient udpClient = new UdpClient();
                udpClient.Connect("XEAS.XBOXLIVE.COM", 88);
                Tools.AppendText($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} >> Connecting to XEAS.XBOXLIVE.COM >> ", ConsoleColor.Green);
                udpClient.Send(array3, array3.Length);
                IPEndPoint ipendPoint = new IPEndPoint(0L, 0);
                int num = 0;
                for (;;)
                {
                    Thread.Sleep(10);
                    bool flag = udpClient.Available <= 0;
                    if (!flag)
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    num++;
                    if (num == 10)
                    {
                        goto Block_4;
                    }
                }
                byte[] sourceArray4 = udpClient.Receive(ref ipendPoint);
                byte[] array8 = new byte[108];
                Array.Copy(sourceArray4, 53, array8, 0, 108);
                byte[] sourceArray5 = KVChecker.RC4HMACDecrypt(KVChecker.ComputeKdcNoonce(array, 16), 16, array8, 108, 1203);
                Array.Copy(sourceArray5, 76, array6, 0, 16);
                return array6;
            Block_4:
                return null;
            }
            catch(Exception ex)
            {
                Tools.AppendText(ex.Message, ConsoleColor.Red);
            }
            return array6;
        }

        // Token: 0x06000034 RID: 52 RVA: 0x00007388 File Offset: 0x00005588
        private static RSACryptoServiceProvider LoadConsolePrivateKey(byte[] Exponent, byte[] KeyParams)
        {
            RSACryptoServiceProvider rsacryptoServiceProvider = new RSACryptoServiceProvider();
            try
            {
                EndianIO endianIO = new EndianIO(KeyParams, EndianStyle.BigEndian);
                byte[] modulus = KVChecker.Reverse8(endianIO.Reader.ReadBytes(128));
                byte[] p = KVChecker.Reverse8(endianIO.Reader.ReadBytes(64));
                byte[] q = KVChecker.Reverse8(endianIO.Reader.ReadBytes(64));
                byte[] dp = KVChecker.Reverse8(endianIO.Reader.ReadBytes(64));
                byte[] dq = KVChecker.Reverse8(endianIO.Reader.ReadBytes(64));
                byte[] inverseQ = KVChecker.Reverse8(endianIO.Reader.ReadBytes(64));
                RSAParameters rsaparameters = new RSAParameters
                {
                    Exponent = Exponent,
                    Modulus = modulus,
                    P = p,
                    Q = q,
                    DP = dp,
                    DQ = dq,
                    InverseQ = inverseQ,
                    D = new byte[128]
                };
                new Random(Environment.TickCount).NextBytes(rsaparameters.D);
                rsacryptoServiceProvider.ImportParameters(rsaparameters);
            }
            catch (Exception ex)
            {
                Tools.AppendText(ex.Message, ConsoleColor.Red);
                Thread.CurrentThread.Abort();
            }
            return rsacryptoServiceProvider;
        }

        // Token: 0x06000035 RID: 53 RVA: 0x000074E0 File Offset: 0x000056E0
        private static RSACryptoServiceProvider LoadXmacsKey()
        {
            byte[] RSAKey = File.ReadAllBytes("Keys/XMACS_pub.bin");
            EndianIO endianIO = new EndianIO(RSAKey, EndianStyle.BigEndian)
            {
                Position = 4L
            };
            byte[] exponent = endianIO.Reader.ReadBytes(4);
            endianIO.Position = 16L;
            byte[] modulus = KVChecker.Reverse8(endianIO.Reader.ReadBytes(256));
            RSAParameters parameters = new RSAParameters
            {
                Exponent = exponent,
                Modulus = modulus,              
            };
            RSACryptoServiceProvider rsacryptoServiceProvider = new RSACryptoServiceProvider();
            rsacryptoServiceProvider.ImportParameters(parameters);
            return rsacryptoServiceProvider;
        }

        // Token: 0x06000036 RID: 54 RVA: 0x00007564 File Offset: 0x00005764
        private static void LogonFromTicketCache()
        {
            //AppDomain.CurrentDomain.BaseDirectory + "KVs\\";
            string str = AppDomain.CurrentDomain.BaseDirectory + "\\files";
            EndianIO endianIO = new EndianIO(str + "kerb_ticket.bin", EndianStyle.BigEndian);
            endianIO.Reader.BaseStream.Position = 212L;
            byte[] key = endianIO.Reader.ReadBytes(16);
            endianIO.Reader.BaseStream.Position = 318L;
            byte[] sourceArray = endianIO.Reader.ReadBytes(345);
            byte[] array = File.ReadAllBytes(str + "TGSREQ.bin");
            Array.Copy(sourceArray, 0, array, 437, 345);
            byte[] array2 = File.ReadAllBytes(str + "authenticator.bin");
            MD5.Create().ComputeHash(array, 954, 75);
            Encoding ascii = Encoding.ASCII;
            string s = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssZ");
            Array.Copy(ascii.GetBytes(s), 0, array2, 109, 15);
            Array.Copy(KVChecker.RC4HMACEncrypt(key, 16, array2, array2.Length, 7), 0, array, 799, 153);
            byte[] key2 = KVChecker.ComputeKdcNoonce(key, 16);
            byte[] data = File.ReadAllBytes(str + "servicereq.bin");
            Array.Copy(KVChecker.RC4HMACEncrypt(key2, 16, data, 126, 1201), 0, array, 55, 150);
            byte[] sourceArray2 = File.ReadAllBytes(str + "apreq2.bin");
            byte[] array3 = new byte[66];
            Array.Copy(sourceArray2, 116, array3, 0, 66);
            Array.Copy(KVChecker.GetTitleAuthData(key, 16, array3), 0, array, 221, 82);
            UdpClient udpClient = new UdpClient(88);
            udpClient.Connect("XETGS.XBOXLIVE.COM", 88);
            udpClient.Send(array, array.Length);
            IPEndPoint ipendPoint = new IPEndPoint(0L, 0);
            byte[] array4 = udpClient.Receive(ref ipendPoint);
            File.WriteAllBytes(str + "tgsresp.bin", array4);
            udpClient.Close();
            byte[] array5 = new byte[84];
            Array.Copy(array4, 50, array5, 0, 84);
            BitConverter.ToUInt32(KVChecker.RC4HMACDecrypt(key2, 16, array5, 84, 1202), 8);
        }

        // Token: 0x06000037 RID: 55 RVA: 0x000077B0 File Offset: 0x000059B0
        private static byte[] RC4HMACDecrypt(byte[] key, int keyLen, byte[] data, int dataLen, int Idk)
        {
            HMACMD5 hmacmd = new HMACMD5(key);
            byte[] bytes = BitConverter.GetBytes(Idk);
            byte[] key2 = hmacmd.ComputeHash(bytes, 0, 4);
            byte[] array = new byte[16];
            Array.Copy(data, array, 16);
            byte[] array2 = new byte[data.Length - 16];
            Array.Copy(data, 16, array2, 0, data.Length - 16);
            hmacmd.Key = key2;
            Security.RC4(ref array2, hmacmd.ComputeHash(array));
            return array2;
        }

        // Token: 0x06000038 RID: 56 RVA: 0x00007828 File Offset: 0x00005A28
        private static byte[] RC4HMACEncrypt(byte[] key, int keyLen, byte[] data, int dataLen, int Idk)
        {
            HMACMD5 hmacmd = new HMACMD5(key);
            byte[] bytes = BitConverter.GetBytes(Idk);
            byte[] key2 = hmacmd.ComputeHash(bytes, 0, 4);
            byte[] sourceArray = Misc.HexStringToBytes("9b6bfacb5c488190");
            byte[] array = new byte[data.Length + 8];
            Array.Copy(sourceArray, array, 8);
            Array.Copy(data, 0, array, 8, data.Length);
            hmacmd.Key = key2;
            byte[] array2 = hmacmd.ComputeHash(array);
            Security.RC4(ref array, hmacmd.ComputeHash(array2));
            byte[] array3 = new byte[dataLen + 24];
            Array.Copy(array2, 0, array3, 0, 16);
            Array.Copy(array, 0, array3, 16, array.Length);
            return array3;
        }

        // Token: 0x06000039 RID: 57 RVA: 0x000078D4 File Offset: 0x00005AD4
        public int returnStatus()
        {
            return this.banned;
        }

        // Token: 0x0600003A RID: 58 RVA: 0x000078EC File Offset: 0x00005AEC
        private static byte[] Reverse8(byte[] input)
        {
            byte[] array = new byte[input.Length];
            int num = input.Length - 8;
            int num2 = 0;
            for (int i = 0; i < input.Length / 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    array[num2 + j] = input[num + j];
                }
                num -= 8;
                num2 += 8;
            }
            return array;
        }

        // Token: 0x04000047 RID: 71
        private static string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/var/www/html/kvchecker/Keys/";

        // Token: 0x04000048 RID: 72
        private int banned;

        // Token: 0x04000049 RID: 73
        //private static string currentFile;
    }
}
