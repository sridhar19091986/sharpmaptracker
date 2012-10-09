using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTibiaCommons.Domain;
using System.Threading;
using System.Diagnostics;
using SharpTibiaProxy.Network;

namespace SharpMapTracker.Share
{
    public class MapShare
    {
        private Queue<object> queue;
        private Connection connection;
        private OutMessage message;
        private Thread thread;

        public MapShare()
        {
            queue = new Queue<object>();
            message = new OutMessage();
            connection = new Connection(Constants.MAP_SHARE_HOST, Constants.MAP_SHARE_PORT);
        }

        public void Start()
        {
            lock (queue)
            {
                if (thread != null)
                    return;

                queue.Clear();
                thread = new Thread(Run);
                thread.Start();
            }
        }

        public void Stop()
        {
            lock (queue)
            {
                queue.Clear();
                thread = null;
                Monitor.Pulse(queue);
            }
        }
        public void AddTile(OtTile tile)
        {
            Add(tile);
        }

        public void AddCreature(OtCreature creature)
        {
            Add(creature);
        }

        private void Add(object obj)
        {
            lock (queue)
            {
                if (thread == null)
                    return;

                queue.Enqueue(obj);
                Monitor.Pulse(queue);
            }
        }

        private void Run()
        {
            while (thread != null)
            {
                try
                {
                    if (!connection.IsConnected)
                    {
                        connection.Connect();
                    }

                    object obj = null;

                    lock (queue)
                    {
                        if (queue.Count == 0)
                            Monitor.Wait(queue);
                        else
                            obj = queue.Dequeue();
                    }

                    if (obj != null)
                    {
                        if (obj is OtTile)
                            SendTile((OtTile)obj);
                        else if (obj is OtCreature)
                            SendCreature((OtCreature)obj);
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("[Error] Map share can not complete. Details: " + e.Message);
                }
            }

            try
            {
                if (connection.IsConnected)
                    connection.Disconnect();
            }
            catch (Exception e)
            {
                Trace.WriteLine("[Error] Can't close the connection. Details: " + e.Message);
            }
        }

        private void SendTile(OtTile tile)
        {
            message.Size = 0;
            message.ReadPosition = 0;
            message.WritePosition = 2;

            message.WriteByte(0x01);

            if (tile.Ground != null)
                message.WriteUShort(tile.Ground.Type.Id);
            else
                message.WriteUShort(0);

            message.WriteByte((byte)tile.ItemCount);

            foreach (var item in tile.Items)
            {
                message.WriteUShort(item.Type.Id);
                if (item.GetAttribute(OtItemAttribute.COUNT) != null)
                    message.WriteByte((byte)item.GetAttribute(OtItemAttribute.COUNT));
            }

            message.WriteHead();
            connection.Send(message);
        }

        private void SendCreature(OtCreature creature)
        {
            message.Size = 0;
            message.ReadPosition = 0;
            message.WritePosition = 2;

            message.WriteByte(0x02);

            message.WriteUInt(creature.Id);
            message.WriteByte((byte)creature.Type);
            message.WriteString(creature.Name);
            message.WriteLocation(creature.Location);

            message.WriteHead();
            connection.Send(message);
        }
    }
}
