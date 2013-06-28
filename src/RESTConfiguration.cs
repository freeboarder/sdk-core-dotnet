using System;
using System.Collections.Generic;
using System.Text;
using PayPal.Manager;
using PayPal.Exception;
using System.Management;
using System.Runtime.InteropServices;

namespace PayPal
{
    public class RESTConfiguration
    {
#if NET_2_0
        /// <summary>
        /// string Authorization Token
        /// </summary>
        private string AuthorizeToken;

        /// <summary>
        ///  Gets and sets the Authorization Token
        /// </summary>
        public string AuthorizationToken
        {
            get
            {
                return this.AuthorizeToken;
            }
            set
            {
                this.AuthorizeToken = value;
            }
        }
#else
        /// <summary>
        ///  Gets and sets the Authorization Token
        /// </summary>
        public string AuthorizationToken
        {
            get;
            set;
        }
#endif

        private string ReqID;

        /// <summary>
        /// Idempotency Request ID
        /// </summary>
        public string RequestID
        {
            private get
            {
                return ReqID;
            }
            set
            {
                ReqID = value;
            }
        }

        /// <summary>
        /// Dynamic configuration map
        /// </summary>
        private Dictionary<string, string> Config;

        /// <summary>
        /// Optional headers map
        /// </summary>
        private Dictionary<string, string> headersMap;

        public RESTConfiguration(Dictionary<string, string> config)
        {
            this.Config = ConfigManager.GetConfigWithDefaults(config);
        }

        public RESTConfiguration(Dictionary<string, string> config, Dictionary<string, string> headersMap)
        {
            this.Config = ConfigManager.GetConfigWithDefaults(config);
            this.headersMap = (headersMap == null) ? new Dictionary<string, string>() : headersMap;
        }

        public Dictionary<string, string> GetHeaders()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(AuthorizationToken))
            {
                headers.Add("Authorization", AuthorizationToken);
            }
            else if (!string.IsNullOrEmpty(GetClientID()) && !string.IsNullOrEmpty(GetClientSecret()))
            {
                headers.Add("Authorization", "Basic " + EncodeToBase64(GetClientID(), GetClientSecret()));
            }
            headers.Add("User-Agent", FormUserAgentHeader());
            if (!string.IsNullOrEmpty(RequestID))
            {
                headers.Add("PayPal-Request-Id", RequestID);
            }
            return headers;
        }

        private string GetClientID()
        {
            return this.Config.ContainsKey(BaseConstants.ClientID) ? this.Config[BaseConstants.ClientID] : null;
        }

        private string GetClientSecret()
        {
            return this.Config.ContainsKey(BaseConstants.ClientSecret) ? this.Config[BaseConstants.ClientSecret] : null;
        }

        private string EncodeToBase64(string clientID, string clientSecret)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(clientID + ":" + clientSecret);
                string base64ClientID = Convert.ToBase64String(bytes);
                return base64ClientID;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new PayPalException(ex.Message, ex);
            }
            catch (ArgumentException ex)
            {
                throw new PayPalException(ex.Message, ex);
            }
            catch (NotSupportedException ex)
            {
                throw new PayPalException(ex.Message, ex);
            }
            catch (System.Exception ex)
            {
                throw new PayPalException(ex.Message, ex);
            }
        }

        public static string FormUserAgentHeader()
        {
            string header = null;
            StringBuilder stringBuilder = new StringBuilder("PayPalSDK/"
                    + PayPalResource.SDKID + " " + PayPalResource.SDKVersion
                    + " ");
            string dotNETVersion = DotNetVersionHeader;
            stringBuilder.Append(";").Append(dotNETVersion);
            string osVersion = GetOSHeader();
            if (osVersion.Length > 0)
            {
                stringBuilder.Append(";").Append(osVersion);
            }
            header = stringBuilder.ToString();
            return header;
        }

        private static string OperatingSystemFriendlyName
        {
            get
            {
                string result = string.Empty;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
                foreach (ManagementObject os in searcher.Get())
                {
                    result = os["Caption"].ToString();
                }
                return result;
            }
        }

#if NET_3_5
        private static bool Is64BitProcess
        {
            get { return IntPtr.Size == 8; }
        }

        private static bool Is64BitOperatingSystem
        {
            get
            {
                if (Is64BitProcess)
                {
                    return true;
                }
                bool isWow64;
                return ModuleContainsFunction("kernel32.dll", "IsWow64Process") && IsWow64Process(GetCurrentProcess(), out isWow64) && isWow64;
            }
        }

        private static bool ModuleContainsFunction(string moduleName, string methodName)
        {
            IntPtr hModule = GetModuleHandle(moduleName);
            if (hModule != IntPtr.Zero)
            {
                return GetProcAddress(hModule, methodName) != IntPtr.Zero;
            }
            return false;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern static IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        extern static IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        extern static IntPtr GetProcAddress(IntPtr hModule, string methodName);
#endif
        private static string GetOSHeader()
        {
            string osHeader = string.Empty;

#if NET_3_5
            if (Is64BitOperatingSystem)
            {
                osHeader += "bit=" + 64 + ";";
            }
            else 
            {
                osHeader += "bit=" + 32 + ";";
            }
#elif NET_4_0
            if (Environment.Is64BitOperatingSystem)
            {
                osHeader += "bit=" + 64 + ";";
            }
            else 
            {
                osHeader += "bit=" + 32 + ";";
            }
#endif
            osHeader += "os=" + OperatingSystemFriendlyName + " " + Environment.OSVersion.Version + ";";
            return osHeader;
        }

        private static string DotNetVersionHeader
        {
            get
            {
                string DotNetVersionHeader = "lang=" + "DOTNET;" + "v=" + Environment.Version.ToString().Trim();
                return DotNetVersionHeader;
            }
        }
    }
}
