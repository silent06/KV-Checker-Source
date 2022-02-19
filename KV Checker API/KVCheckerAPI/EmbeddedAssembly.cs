using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

public class EmbeddedAssembly
{
    private static Dictionary<string, Assembly> dic = null;

    public static Assembly Get(string assemblyFullName)
    {
        if ((dic != null) && (dic.Count != 0))
        {
            if (dic.ContainsKey(assemblyFullName))
            {
                return dic[assemblyFullName];
            }
            return null;
        }
        return null;
    }

    public static void Load(string embeddedResource, string fileName)
    {
        if (dic == null)
        {
            dic = new Dictionary<string, Assembly>();
        }
        byte[] buffer = null;
        Assembly assembly = null;
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResource))
        {
            if (stream == null)
            {
                throw new Exception(embeddedResource + " is not found in Embedded Resources.");
            }
            buffer = new byte[(int) stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            try
            {
                assembly = Assembly.Load(buffer);
                dic.Add(assembly.FullName, assembly);
                return;
            }
            catch
            {
            }
        }
        bool flag = false;
        string path = "";
        using (SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider())
        {
            string str2 = BitConverter.ToString(provider.ComputeHash(buffer)).Replace("-", string.Empty);
            path = Path.GetTempPath() + fileName;
            if (File.Exists(path))
            {
                byte[] buffer2 = File.ReadAllBytes(path);
                string str3 = BitConverter.ToString(provider.ComputeHash(buffer2)).Replace("-", string.Empty);
                if (str2 == str3)
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                }
            }
            else
            {
                flag = false;
            }
        }
        if (!flag)
        {
            File.WriteAllBytes(path, buffer);
        }
        assembly = Assembly.LoadFile(path);
        dic.Add(assembly.FullName, assembly);
    }
}

