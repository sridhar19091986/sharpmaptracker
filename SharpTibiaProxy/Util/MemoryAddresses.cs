using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTibiaProxy.Util
{
    public class MemoryAddresses
    {
        public readonly int ClientRsa;
        public readonly int ClientServerStart;
        public readonly int ClientServerStep;
        public readonly int ClientServerMax;
        public readonly int ClientServerDistancePort;
        public readonly int ClientSelectedCharacter;

        public readonly int ClientMultiClient;
        public readonly byte ClientMultiClientJMP = 0xEB;
        public readonly byte ClientMultiClientJNZ = 0x75;

        public readonly int ClientStatus = 0x3BCCC4;

        public MemoryAddresses(string version)
        {
            if (Constants.Versions.Version963.Equals(version))
            {
                ClientRsa = 0x324EC0;
                ClientServerStart = 0x3B34F8;
                ClientServerStep = 112;
                ClientServerMax = 10;
                ClientServerDistancePort = 100;
                ClientSelectedCharacter = 0x3BCD10;
                ClientMultiClient = 0x12E807;
            }
            else
            {
                throw new Exception("The client version " + version + " is not supported.");
            }
        }
    }
}
