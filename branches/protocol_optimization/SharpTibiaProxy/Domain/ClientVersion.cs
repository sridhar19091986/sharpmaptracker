using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTibiaProxy.Domain
{
    public class ClientVersion
    {
        public static readonly ClientVersion Version961 = new ClientVersion { Number = 961, FileVersion = "9.6.1.0", OtbmVersion = 3, OtbMajorVersion = 3, OtbMinorVersion = 41 };
        public static readonly ClientVersion Version963 = new ClientVersion { Number = 963, FileVersion = "9.6.3.0", OtbmVersion = 3, OtbMajorVersion = 3, OtbMinorVersion = 42 };
        public static readonly ClientVersion Current = Version963;

        public int Number { get; private set; }
        public string FileVersion { get; private set; }
        public uint OtbmVersion { get; private set; }
        public uint OtbMajorVersion { get; private set; }
        public uint OtbMinorVersion { get; private set; }

        private ClientVersion() { }

        public static ClientVersion GetFromFileVersion(string fileVersion)
        {
            switch (fileVersion)
            {
                case "9.6.1.0": return Version961;
                case "9.6.3.0": return Version963;
                default: return null;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as ClientVersion;
            return other != null && other.Number == Number;
        }

        public override int GetHashCode()
        {
            return Number ^ 31;
        }
    }
}
