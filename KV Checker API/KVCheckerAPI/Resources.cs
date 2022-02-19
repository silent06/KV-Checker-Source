namespace KVChecker_API
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class Resources
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal Resources()
        {
        }

        internal static byte[] apReq1 =>
            ((byte[])ResourceManager.GetObject("apReq1", resourceCulture));

        internal static byte[] apreq2 =>
            ((byte[])ResourceManager.GetObject("apreq2", resourceCulture));

        internal static byte[] APRESP =>
            ((byte[])ResourceManager.GetObject("APRESP", resourceCulture));

        internal static byte[] authenticator =>
            ((byte[])ResourceManager.GetObject("authenticator", resourceCulture));

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get =>
                resourceCulture;
            set
            {
                resourceCulture = value;
            }
        }

        internal static byte[] macsresp =>
            ((byte[])ResourceManager.GetObject("macsresp", resourceCulture));

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (resourceMan == null)
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("KV_Checker.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }

        internal static byte[] resp =>
            ((byte[])ResourceManager.GetObject("resp", resourceCulture));

        internal static byte[] servicereq =>
            ((byte[])ResourceManager.GetObject("servicereq", resourceCulture));

        internal static byte[] test =>
            ((byte[])ResourceManager.GetObject("test", resourceCulture));

        internal static byte[] TGSREQ =>
            ((byte[])ResourceManager.GetObject("TGSREQ", resourceCulture));

        internal static byte[] tgsres =>
            ((byte[])ResourceManager.GetObject("tgsres", resourceCulture));

        internal static byte[] tgsresp =>
            ((byte[])ResourceManager.GetObject("tgsresp", resourceCulture));

        internal static byte[] XMACSREQ =>
            ((byte[])ResourceManager.GetObject("XMACSREQ", resourceCulture));
    }
}
