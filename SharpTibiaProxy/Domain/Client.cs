using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using SharpTibiaProxy.Network;
using SharpTibiaProxy.Util;

namespace SharpTibiaProxy.Domain
{
    public class Client
    {
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

        private Proxy proxy;
        private MemoryAddresses memoryAddresses;
        private string cachedVersion;

        private IntPtr processHandle;

        public bool IsOpenTibiaServer { get; set; }

        public uint PlayerId { get; set; }
        public Location PlayerLocation { get; set; }
        public bool PlayerCanReportBugs { get; set; }

        public Process Process { get; private set; }
        public Items Items { get; private set; }
        public Map Map { get; private set; }
        public BattleList BattleList { get; private set; }
        public ProtocolWorld ProtocolWorld { get; private set; }

        public MemoryAddresses MemoryAddresses
        {
            get
            {
                if (memoryAddresses == null)
                    memoryAddresses = new MemoryAddresses(Version);

                return memoryAddresses;
            }
        }


        public Client(Process process)
        {
            this.Process = process;
            this.Process.EnableRaisingEvents = true;
            this.Process.Exited += new EventHandler(Process_Exited);

            this.Process.WaitForInputIdle();

            memoryAddresses = new MemoryAddresses(Version);

            processHandle = WinApi.OpenProcess(WinApi.PROCESS_ALL_ACCESS, 0, (uint)process.Id);
            Initialize(Path.Combine(Path.GetDirectoryName(process.MainModule.FileName), "Tibia.dat"));
        }

        ~Client()
        {
            WinApi.CloseHandle(processHandle);
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


        private void Process_Exited(object sender, EventArgs e)
        {
            try
            {
                DisableProxy();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Error] " + ex.Message);
            }
        }

        public string Rsa
        {
            get { return Memory.ReadString(processHandle, Process.MainModule.BaseAddress.ToInt64() + MemoryAddresses.ClientRsa, 309); }
            set { Memory.WriteRSA(processHandle, Process.MainModule.BaseAddress.ToInt64() + MemoryAddresses.ClientRsa, value); }
        }

        public LoginServer[] LoginServers
        {
            get
            {
                LoginServer[] servers = new LoginServer[MemoryAddresses.ClientServerMax];
                long address = Process.MainModule.BaseAddress.ToInt64() + MemoryAddresses.ClientServerStart;

                for (int i = 0; i < MemoryAddresses.ClientServerMax; i++)
                {
                    servers[i] = new LoginServer(
                        Memory.ReadString(processHandle, address),
                        (short)Memory.ReadInt32(processHandle, address + MemoryAddresses.ClientServerDistancePort)
                    );
                    address += MemoryAddresses.ClientServerStep;
                }
                return servers;
            }
            set
            {
                long address = Process.MainModule.BaseAddress.ToInt64() + MemoryAddresses.ClientServerStart;
                for (int i = 0; i < MemoryAddresses.ClientServerMax; i++)
                {
                    Memory.WriteString(processHandle, address, value[i % value.Length].Server);
                    Memory.WriteInt32(processHandle, address + MemoryAddresses.ClientServerDistancePort, value[i % value.Length].Port);
                    address += MemoryAddresses.ClientServerStep;
                }
            }
        }

        public int SelectedChar
        {
            get { return Memory.ReadInt32(processHandle, Process.MainModule.BaseAddress.ToInt64() + MemoryAddresses.ClientSelectedCharacter); }
            set { Memory.WriteInt32(processHandle, Process.MainModule.BaseAddress.ToInt64() + MemoryAddresses.ClientSelectedCharacter, value); }
        }

        public bool ProxyEnabled { get { return proxy != null; } }

        public bool HasExited { get { return Process != null && Process.HasExited; } }

        public void EnableProxy()
        {
            if (Process == null)
                throw new Exception("Can not enable proxy on a clientless instance.");

            if (proxy != null)
                return;

            proxy = new Proxy(this);
            proxy.Enable();
        }

        public void DisableProxy()
        {
            if (proxy == null)
                return;

            proxy.Disable();
            proxy = null;
        }

        public Constants.LoginStatus Status
        {
            get { return (Constants.LoginStatus)Memory.ReadByte(processHandle, Process.MainModule.BaseAddress.ToInt64() + MemoryAddresses.ClientStatus); }
        }

        public bool LoggedIn
        {
            get { return Status == Constants.LoginStatus.LoggedIn; }
        }

        public string Version
        {
            get
            {
                if (cachedVersion == null)
                {
                    cachedVersion = Process.MainModule.FileVersionInfo.FileVersion;
                }
                return cachedVersion;
            }
        }

        #region Open Client
        /// <summary>
        /// Open a client at the default path
        /// </summary>
        /// <returns></returns>
        public static Client Open()
        {
            return Open(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Tibia\tibia.exe"));
        }

        /// <summary>
        /// Open a client from the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Client Open(string path)
        {
            ProcessStartInfo psi = new ProcessStartInfo(path);
            psi.UseShellExecute = true; // to avoid opening currently running Tibia's
            psi.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
            return Open(psi);
        }

        /// <summary>
        /// Open a client from the specified path with arguments
        /// </summary>
        /// <param name="path"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static Client Open(string path, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo(path, arguments);
            psi.UseShellExecute = true; // to avoid opening currently running Tibia's
            psi.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
            return Open(psi);
        }

        /// <summary>
        /// Opens a client given a process start info object.
        /// </summary>
        public static Client Open(ProcessStartInfo psi)
        {
            Process p = Process.Start(psi);
            return new Client(p);
        }


        public static Client OpenMC()
        {
            return OpenMC(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Tibia\tibia.exe"), "");
        }

        /// <summary>
        /// Opens a client with dynamic multi-clienting support
        /// </summary>
        public static Client OpenMC(string path, string arguments)
        {
            Util.WinApi.PROCESS_INFORMATION pi = new WinApi.PROCESS_INFORMATION();
            Util.WinApi.STARTUPINFO si = new WinApi.STARTUPINFO();

            if (arguments == null)
                arguments = "";

            Util.WinApi.CreateProcess(path, " " + arguments, IntPtr.Zero, IntPtr.Zero,
                false, Util.WinApi.CREATE_SUSPENDED, IntPtr.Zero,
                System.IO.Path.GetDirectoryName(path), ref si, out pi);

            IntPtr handle = Util.WinApi.OpenProcess(Util.WinApi.PROCESS_ALL_ACCESS, 0, pi.dwProcessId);
            Process p = Process.GetProcessById(Convert.ToInt32(pi.dwProcessId));

            IntPtr baseAddress = WinApi.GetBaseAddress(handle);

            var addresses = new MemoryAddresses(Constants.Versions.Version963);

            Util.Memory.WriteByte(handle, baseAddress.ToInt64() + addresses.ClientMultiClient,
                addresses.ClientMultiClientJMP);

            Util.WinApi.ResumeThread(pi.hThread);
            p.WaitForInputIdle();

            Util.Memory.WriteByte(handle, baseAddress.ToInt64() + addresses.ClientMultiClient,
                addresses.ClientMultiClientJNZ);

            Util.WinApi.CloseHandle(handle);
            Util.WinApi.CloseHandle(pi.hProcess);
            Util.WinApi.CloseHandle(pi.hThread);

            return new Client(p);
        }

        #endregion

        #region Client Processes
        /// <summary>
        /// Get a list of all the open clients. Class method.
        /// </summary>
        /// <returns></returns>
        public static List<Client> GetClients()
        {

            return GetClients(null);
        }

        /// <summary>
        /// Get a list of all the open clients of certain version. Class method.
        /// </summary>
        /// <returns></returns>
        public static List<Client> GetClients(string version)
        {
            return GetClients(version, false);
        }

        /// <summary>
        /// Get a list of all the open clients of certain version. Class method.
        /// </summary>
        /// <returns></returns>
        public static List<Client> GetClients(string version, bool offline)
        {
            List<Client> clients = new List<Client>();

            foreach (Process process in Process.GetProcesses())
            {
                StringBuilder classname = new StringBuilder();
                Util.WinApi.GetClassName(process.MainWindowHandle, classname, 12);

                if (classname.ToString().Equals("TibiaClient", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (version == null || process.MainModule.FileVersionInfo.FileVersion == version)
                    {
                        var client = new Client(process);
                        if (!offline || !client.LoggedIn)
                            clients.Add(client);
                    }
                }
            }
            return clients;
        }

        public void Close()
        {
            if (Process != null && !Process.HasExited)
                Process.Kill();
        }

        #endregion

        public override string ToString()
        {
            string s = "[" + Version + "] ";
            if (!LoggedIn)
                s += "Not logged in.";
            else
                s += "Logged in.";

            return s;
        }
    }
}
