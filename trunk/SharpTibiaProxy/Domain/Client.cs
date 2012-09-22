using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using SharpTibiaProxy.Memory;
using SharpTibiaProxy.Network;

namespace SharpTibiaProxy.Domain
{
    public class Client
    {
        private const int LOGIN_RSA = 0x324EC0;
        private const int LOGIN_STEP_SERVER = 112;
        private const int LOGIN_DISTANCE_PORT = 100;
        private const int LOGIN_MAX_SERVERS = 10;
        private const int LOGIN_SERVER_START = 0x3B34F8;
        private const int LOGIN_SELECTED_CHAR = 0x3BCD10;

        public static readonly LoginServer[] DefaultServers = 
        {
            new LoginServer("login01.tibia.com"),
            new LoginServer("login02.tibia.com"),
            new LoginServer("login03.tibia.com"),
            new LoginServer("login04.tibia.com"),
            new LoginServer("login05.tibia.com"),
            new LoginServer("tibia01.cipsoft.com"),
            new LoginServer("tibia02.cipsoft.com"),
            new LoginServer("tibia03.cipsoft.com"),
            new LoginServer("tibia04.cipsoft.com"),
            new LoginServer("tibia05.cipsoft.com")
        };


        private Process process;
        private Proxy proxy; 

        private IntPtr handle;

        public uint PlayerId { get; set; }
        public Location PlayerLocation { get; set; }
        public bool PlayerCanReportBugs { get; set; }

        public Items Items { get; private set; }
        public Map Map { get; private set; }
        public BattleList BattleList { get; private set; }
        public ProtocolWorld ProtocolWorld { get; private set; }

        public Client(Process process)
        {
            this.process = process;
            this.process.EnableRaisingEvents = true;
            this.process.Exited += new EventHandler(Process_Exited);

            handle = WinApi.OpenProcess(WinApi.PROCESS_ALL_ACCESS, 0, (uint)process.Id);
            Initialize(Path.Combine(Path.GetDirectoryName(process.MainModule.FileName), "Tibia.dat"));
        }

        public Client(string datFilename)
        {
            Initialize(datFilename);
        }

        private void Initialize(string datFilename)
        {
            Items = new Items();
            Items.Load(datFilename);

            Map = new Map(this);
            BattleList = new BattleList(this);
            ProtocolWorld = new ProtocolWorld(this);
        }


        void Process_Exited(object sender, EventArgs e)
        {
        }

        public string Rsa
        {
            get { return Memory.Memory.ReadString(handle, process.MainModule.BaseAddress.ToInt64() + LOGIN_RSA, 309); }
            set { Memory.Memory.WriteRSA(handle, process.MainModule.BaseAddress.ToInt64() + LOGIN_RSA, value); }
        }

        public LoginServer[] LoginServers
        {
            get
            {
                LoginServer[] servers = new LoginServer[LOGIN_MAX_SERVERS];
                long address = process.MainModule.BaseAddress.ToInt64() + LOGIN_SERVER_START;

                for (int i = 0; i < LOGIN_MAX_SERVERS; i++)
                {
                    servers[i] = new LoginServer(
                        Memory.Memory.ReadString(handle, address),
                        (short)Memory.Memory.ReadInt32(handle, address + LOGIN_DISTANCE_PORT)
                    );
                    address += LOGIN_STEP_SERVER;
                }
                return servers;
            }
            set
            {
                long address = process.MainModule.BaseAddress.ToInt64() + LOGIN_SERVER_START;
                if (value.Length == 1)
                {
                    string server = value[0].Server + (char)0;
                    for (int i = 0; i < LOGIN_MAX_SERVERS; i++)
                    {
                        Memory.Memory.WriteString(handle, address, value[0].Server);
                        Memory.Memory.WriteInt32(handle, address + LOGIN_DISTANCE_PORT, value[0].Port);
                        address += LOGIN_STEP_SERVER;
                    }
                }
                else if (value.Length > 1 && value.Length <= LOGIN_MAX_SERVERS)
                {
                    string server = string.Empty;
                    for (int i = 0; i < value.Length; i++)
                    {
                        server = value[i].Server + (char)0;
                        Memory.Memory.WriteString(handle, address, server);
                        Memory.Memory.WriteInt32(handle, address + LOGIN_DISTANCE_PORT, value[0].Port);
                        address += LOGIN_STEP_SERVER;
                    }
                }
            }
        }

        public int SelectedChar
        {
            get { return Memory.Memory.ReadInt32(handle, process.MainModule.BaseAddress.ToInt64() + LOGIN_SELECTED_CHAR); }
            set { Memory.Memory.WriteInt32(handle, process.MainModule.BaseAddress.ToInt64() + LOGIN_SELECTED_CHAR, value); }
        }

        public void EnableProxy()
        {
            if (process == null)
                throw new Exception("Can not enable proxy on a clientless instance.");

            if (proxy != null)
                return;

            proxy = new Proxy(this);
        }
    }
}
