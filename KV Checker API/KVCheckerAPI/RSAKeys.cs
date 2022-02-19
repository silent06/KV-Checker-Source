using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace KVChecker_API
{
    // Token: 0x02000006 RID: 6
    [CompilerGenerated]
    [DebuggerNonUserCode]
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    internal class RSAKeys
    {
        // Token: 0x17000001 RID: 1
        // (get) Token: 0x0600003D RID: 61 RVA: 0x00007A18 File Offset: 0x00005C18
        // (set) Token: 0x0600003E RID: 62 RVA: 0x00007A2F File Offset: 0x00005C2F
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return RSAKeys.resourceCulture;
            }
            set
            {
                RSAKeys.resourceCulture = value;
            }
        }

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x0600003F RID: 63 RVA: 0x00007A38 File Offset: 0x00005C38
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                bool flag = RSAKeys.resourceMan == null;
                if (flag)
                {
                    RSAKeys.resourceMan = new ResourceManager("KVChecker_API.RSAKeys", typeof(RSAKeys).Assembly);
                }
                return RSAKeys.resourceMan;
            }
        }

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000040 RID: 64 RVA: 0x00007A7C File Offset: 0x00005C7C
        internal static byte[] XMACS_RSA_PUB2048
        {
            get
            {
                return (byte[])RSAKeys.ResourceManager.GetObject("XMACS_RSA_PUB2048", RSAKeys.resourceCulture);
            }
        }

        // Token: 0x06000041 RID: 65 RVA: 0x0000676F File Offset: 0x0000496F
        internal RSAKeys()
        {
        }

        // Token: 0x0400004A RID: 74
        private static ResourceManager resourceMan;

        // Token: 0x0400004B RID: 75
        private static CultureInfo resourceCulture;
    }
}
