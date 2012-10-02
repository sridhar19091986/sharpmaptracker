using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTibiaProxy.Domain
{
    public class ClientVersion
    {
        public static readonly ClientVersion Version961 = new ClientVersion { Id = 961, FileVersion = "9.6.1.0", OtbmVersion = 3, OtbVersion = 41 };
        public static readonly ClientVersion Version963 = new ClientVersion { Id = 963, FileVersion = "9.6.3.0", OtbmVersion = 3, OtbVersion = 42 };

        public int Id { get; private set; }
        public string FileVersion { get; private set; }
        public int OtbmVersion { get; private set; }
        public int OtbVersion { get; private set; }
        
        private ClientVersion() {}

        public static ClientVersion GetFromFileVersion(string fileVersion)
        {
            switch (fileVersion)
            {
                case "9.6.1.0": return Version961;
                case "9.6.3.0": return Version963;
                default: return null;
            }
        }
    }
}
