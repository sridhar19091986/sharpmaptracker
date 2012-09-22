using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharpTibiaProxy.Memory;
using SharpTibiaProxy.Domain;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SharpTibiaProxy.Network
{
    public enum Protocol
    {
        None, Login, World
    }

    public class Proxy
    {
        private Client client;

        private int loginClientPort;
        private int worldClientPort;

        private Socket loginClientSocket;
        private Socket worldClientSocket;

        private Socket clientSocket;
        private Socket serverSocket;

        private InMessage clientInMessage;
        private OutMessage clientOutMessage;

        private InMessage serverInMessage;
        private OutMessage servertOutMessage;

        private bool accepting;

        private LoginServer[] loginServers;

        private Protocol protocol = Protocol.None;

        private uint[] xteaKey;
        private CharacterLoginInfo[] charList;

        public Proxy(Client client)
        {
            this.client = client;
            client.Rsa = Constants.RSAKey.OpenTibiaM;

            loginClientPort = GetFreePort();
            worldClientPort = GetFreePort(loginClientPort + 1);

            if (client.LoginServers[0].Server == "localhost")
                loginServers = Client.DefaultServers;
            else
                loginServers = client.LoginServers;


            client.LoginServers = new LoginServer[] { new LoginServer("localhost", loginClientPort) };

            clientInMessage = new InMessage();
            clientOutMessage = new OutMessage();

            serverInMessage = new InMessage();
            servertOutMessage = new OutMessage();

            StartListen();
        }

        private void StartListen()
        {
            try
            {
                lock (this)
                {
                    if (accepting)
                        return;

                    protocol = Protocol.None;

                    loginClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    loginClientSocket.Bind(new IPEndPoint(IPAddress.Any, loginClientPort));
                    loginClientSocket.Listen(1);
                    loginClientSocket.BeginAccept(ClientBeginAcceptCallback, Protocol.Login);

                    worldClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    worldClientSocket.Bind(new IPEndPoint(IPAddress.Any, worldClientPort));
                    worldClientSocket.Listen(1);
                    worldClientSocket.BeginAccept(ClientBeginAcceptCallback, Protocol.World);

                    accepting = true;
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Error] Proxy [StartListen]: " + ex.Message);
            }
        }

        private void ClientBeginAcceptCallback(IAsyncResult ar)
        {
            try
            {
                lock (this)
                {
                    if (!accepting)
                        return;

                    accepting = false;

                    Protocol protocol = (Protocol)ar.AsyncState;
                    clientSocket = null;

                    if (protocol == Protocol.Login)
                        clientSocket = loginClientSocket.EndAccept(ar);
                    else
                    {
                        clientSocket = worldClientSocket.EndAccept(ar);

                        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        CharacterLoginInfo selectedChar = charList[client.SelectedChar];
                        serverSocket.Connect(new IPEndPoint(selectedChar.WorldIP, selectedChar.WorldPort));

                        serverInMessage.Reset();
                        serverSocket.BeginReceive(serverInMessage.Buffer, 0, 2, SocketFlags.None, ServerReceiveCallback, null);
                    }

                    loginClientSocket.Close();
                    worldClientSocket.Close();

                    clientInMessage.Reset();
                    clientSocket.BeginReceive(clientInMessage.Buffer, 0, 2, SocketFlags.None, ClientReceiveCallback, null);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Error] Proxy [ClientBeginAcceptCallback]: " + ex.Message);
            }
        }

        private void ServerReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int count = serverSocket.EndReceive(ar);

                if (count <= 0)
                    throw new Exception("Connection lost.");

                serverInMessage.Size = serverInMessage.ReadHead() + 2;
                int read = 2;

                while (read < serverInMessage.Size)
                {
                    count = serverSocket.Receive(serverInMessage.Buffer, read, serverInMessage.Size - read, SocketFlags.None);

                    if (count <= 0)
                        throw new Exception("Connection lost.");

                    read += count;
                }

                ParseServerMessage();

                serverInMessage.Reset();
                serverSocket.BeginReceive(serverInMessage.Buffer, 0, 2, SocketFlags.None, ServerReceiveCallback, null);
            }
            catch (Exception)
            {
                //Trace.WriteLine("[Error] Proxy [ServerReceiveCallback]: " + ex.Message);
                Restart();
            }
        }

        private void ParseServerMessage()
        {
            switch (protocol)
            {
                case Protocol.None:
                    clientSocket.Send(serverInMessage.Buffer, 0, serverInMessage.Size, SocketFlags.None);
                    break;
                case Protocol.Login:
                    ParseServerLoginMessage();
                    break;
                case Protocol.World:
                    ParseServerWorldMessage();
                    break;
            }
        }

        private void ParseServerWorldMessage()
        {
            clientOutMessage.Reset();
            Array.Copy(serverInMessage.Buffer, clientOutMessage.Buffer, serverInMessage.Size);
            clientOutMessage.Size = serverInMessage.Size;

            serverInMessage.ReadPosition = 2;

            if (Adler.Generate(serverInMessage) != serverInMessage.ReadChecksum())
                throw new Exception("Wrong checksum.");

            Xtea.Decrypt(serverInMessage, xteaKey);
            serverInMessage.Size = serverInMessage.ReadInternalHead() + 8;
            serverInMessage.ReadPosition = 8;

            client.ProtocolWorld.ParseMessage(serverInMessage);

            clientSocket.Send(clientOutMessage.Buffer, 0, clientOutMessage.Size, SocketFlags.None);
        }

        private void ParseServerLoginMessage()
        {
            serverInMessage.ReadPosition = 2;

            if (Adler.Generate(serverInMessage) != serverInMessage.ReadChecksum())
                throw new Exception("Wrong checksum.");

            Xtea.Decrypt(serverInMessage, xteaKey);
            serverInMessage.Size = serverInMessage.ReadInternalHead() + 8;
            serverInMessage.ReadPosition = 8;

            clientOutMessage.Reset();
            Array.Copy(serverInMessage.Buffer, clientOutMessage.Buffer, serverInMessage.Size);
            clientOutMessage.Size = serverInMessage.Size;

            while (serverInMessage.ReadPosition < serverInMessage.Size)
            {
                byte cmd = serverInMessage.ReadByte();

                switch (cmd)
                {
                    case 0x0A: //Error message
                        var msg = serverInMessage.ReadString();
                        break;
                    case 0x0B: //For your information
                        serverInMessage.ReadString();
                        break;
                    case 0x14: //MOTD
                        serverInMessage.ReadString();
                        break;
                    case 0x1E: //Patching exe/dat/spr messages
                    case 0x1F:
                    case 0x20:
                        //DisconnectClient(0x0A, "A new client is avalible, please download it first!");
                        break;
                    case 0x28: //Select other login server
                        //selectedLoginServer = random.Next(0, loginServers.Length - 1);
                        break;
                    case 0x64: //character list
                        int nChar = (int)serverInMessage.ReadByte();
                        charList = new CharacterLoginInfo[nChar];

                        for (int i = 0; i < nChar; i++)
                        {
                            charList[i].CharName = serverInMessage.ReadString();
                            charList[i].WorldName = serverInMessage.ReadString();
                            clientOutMessage.WriteAt(new byte[] { 127, 0, 0, 1 }, serverInMessage.ReadPosition);
                            charList[i].WorldIP = serverInMessage.ReadUInt();
                            clientOutMessage.WriteAt(BitConverter.GetBytes((ushort)worldClientPort), serverInMessage.ReadPosition);
                            charList[i].WorldPort = serverInMessage.ReadUShort();
                        }
                        break;
                    default:
                        break;
                }
            }

            clientOutMessage.WriteInternalHead();
            Xtea.Encrypt(clientOutMessage, xteaKey);
            Adler.Generate(clientOutMessage, true);
            clientOutMessage.WriteHead();

            clientSocket.Send(clientOutMessage.Buffer, 0, clientOutMessage.Size, SocketFlags.None);
        }

        private void ClientReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int count = clientSocket.EndReceive(ar);

                if (count <= 0)
                    throw new Exception("Connection lost.");

                clientInMessage.Size = clientInMessage.ReadHead() + 2;
                int read = 2;

                while (read < clientInMessage.Size)
                {
                    count = clientSocket.Receive(clientInMessage.Buffer, read, clientInMessage.Size - read, SocketFlags.None);

                    if (count <= 0)
                        throw new Exception("Connection lost.");

                    read += count;
                }

                ParseClientMessage();

                clientInMessage.Reset();
                clientSocket.BeginReceive(clientInMessage.Buffer, 0, 2, SocketFlags.None, ClientReceiveCallback, null);
            }
            catch (Exception ex)
            {
                //Trace.WriteLine("[Error] Proxy [ClientReceiveCallback]: " + ex.Message);
                Restart();
            }
        }

        private void ParseClientMessage()
        {
            switch (protocol)
            {
                case Protocol.None:
                    ParseFirstClientMessage();
                    break;
                case Protocol.Login:
                    throw new Exception("Invalid client message.");
                case Protocol.World:
                    ParseClientWorldMessage();
                    break;
            }
        }

        private void ParseFirstClientMessage()
        {
            clientInMessage.ReadPosition = 2;
            clientInMessage.Encrypted = false;

            if (Adler.Generate(clientInMessage) != clientInMessage.ReadUInt())
                throw new Exception("Wrong checksum.");

            byte protocolId = clientInMessage.ReadByte();

            if (protocolId == 0x01) //Login
            {
                protocol = Protocol.Login;
                clientInMessage.ReadUShort();
                ushort clientVersion = clientInMessage.ReadUShort();

                clientInMessage.ReadUInt();
                clientInMessage.ReadUInt();
                clientInMessage.ReadUInt();

                Rsa.OpenTibiaDecrypt(clientInMessage);

                Array.Copy(clientInMessage.Buffer, servertOutMessage.Buffer, clientInMessage.Size);
                servertOutMessage.Size = clientInMessage.Size;
                servertOutMessage.WritePosition = clientInMessage.ReadPosition - 1; //the first byte is zero

                xteaKey = new uint[4];
                xteaKey[0] = clientInMessage.ReadUInt();
                xteaKey[1] = clientInMessage.ReadUInt();
                xteaKey[2] = clientInMessage.ReadUInt();
                xteaKey[3] = clientInMessage.ReadUInt();

                var acc = clientInMessage.ReadString(); //account name
                var pass = clientInMessage.ReadString(); //password

                Rsa.RealTibiaEncrypt(servertOutMessage);
                Adler.Generate(servertOutMessage, true);
                servertOutMessage.WriteHead();

                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(loginServers[0].Server, loginServers[0].Port);

                serverSocket.Send(servertOutMessage.Buffer, 0, servertOutMessage.Size, SocketFlags.None);

                serverInMessage.Reset();
                serverSocket.BeginReceive(serverInMessage.Buffer, 0, 2, SocketFlags.None, ServerReceiveCallback, null);
            }
            else if (protocolId == 0x0A) //Game
            {
                protocol = Protocol.World;

                clientInMessage.ReadUShort();
                ushort clientVersion = clientInMessage.ReadUShort();

                Rsa.OpenTibiaDecrypt(clientInMessage);

                Array.Copy(clientInMessage.Buffer, servertOutMessage.Buffer, clientInMessage.Size);
                servertOutMessage.Size = clientInMessage.Size;
                servertOutMessage.WritePosition = clientInMessage.ReadPosition - 1; //the first byte is zero

                xteaKey = new uint[4];
                xteaKey[0] = clientInMessage.ReadUInt();
                xteaKey[1] = clientInMessage.ReadUInt();
                xteaKey[2] = clientInMessage.ReadUInt();
                xteaKey[3] = clientInMessage.ReadUInt();

                clientInMessage.ReadByte();

                var accountName = clientInMessage.ReadString();
                var characterName = clientInMessage.ReadString();
                var password = clientInMessage.ReadString();

                Rsa.RealTibiaEncrypt(servertOutMessage);
                Adler.Generate(servertOutMessage, true);
                servertOutMessage.WriteHead();

                serverSocket.Send(servertOutMessage.Buffer, 0, servertOutMessage.Size, SocketFlags.None);
            }
            else
            {
                throw new Exception("Invalid protocol " + protocolId.ToString("X2"));
            }
        }

        private void ParseClientWorldMessage()
        {
            serverSocket.Send(clientInMessage.Buffer, 0, clientInMessage.Size, SocketFlags.None);
        }

        private void Restart()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                try
                {
                    clientSocket.Close();
                    clientSocket = null;
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Proxy [Restart]: " + ex.Message);
                }
            }

            if (serverSocket != null && serverSocket.Connected)
            {
                try
                {
                    serverSocket.Close();
                    serverSocket = null;
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Proxy [Restart]: " + ex.Message);
                }
            }

            clientInMessage.Reset();
            clientOutMessage.Reset();
            serverInMessage.Reset();
            servertOutMessage.Reset();

            StartListen();
        }

        private static int GetFreePort()
        {
            return GetFreePort(7979);
        }

        private static int GetFreePort(int start)
        {
            while (!CheckPort(start))
                start++;
            return start;
        }

        private static bool CheckPort(int port)
        {
            try
            {
                TcpListener tcpScan = new TcpListener(IPAddress.Any, port);
                tcpScan.Start();
                tcpScan.Stop();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
