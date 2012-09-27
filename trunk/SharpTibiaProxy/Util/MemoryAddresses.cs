using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;

namespace SharpTibiaProxy.Util
{
    public class MemoryAddresses
    {
        public readonly long ClientRsa;
        public readonly long ClientServerStart;
        public readonly long ClientServerStep;
        public readonly long ClientServerMax;
        public readonly long ClientServerDistancePort;
        public readonly long ClientSelectedCharacter;

        public readonly long ClientMultiClient;
        public readonly byte ClientMultiClientJMP = 0xEB;
        public readonly byte ClientMultiClientJNZ = 0x75;

        public readonly int ClientStatus = 0x3BCCC4;

        public MemoryAddresses(Client client)
        {
            if (client.Version == ClientVersion.Version963)
            {
                ClientRsa = client.BaseAddress + 0x324EC0;
                ClientServerStart = client.BaseAddress + 0x3B34F8;
                ClientServerStep = 112;
                ClientServerMax = 10;
                ClientServerDistancePort = 100;
                ClientSelectedCharacter = client.BaseAddress + 0x3BCD10;
                ClientMultiClient = client.BaseAddress + 0x12E807;
            }
            else
            {
                throw new Exception("The client version " + client.Version + " is not supported.");
            }
        }
    }
}
