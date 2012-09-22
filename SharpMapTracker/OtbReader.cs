﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SharpMapTracker
{
    public class OtbReader
    {
        private enum ItemGroup
        {
            NONE = 0,
            GROUND,
            CONTAINER,
            WEAPON,
            AMMUNITION,
            ARMOR,
            CHARGES,
            TELEPORT,
            MAGICFIELD,
            WRITEABLE,
            KEY,
            SPLASH,
            FLUID,
            DOOR,
            DEPRECATED,
            LAST
        };

        private enum ItemAttr
        {
            ITEM_ATTR_FIRST = 0x10,
            ITEM_ATTR_SERVERID = ITEM_ATTR_FIRST,
            ITEM_ATTR_CLIENTID,
            ITEM_ATTR_NAME,
            ITEM_ATTR_DESCR,			/*deprecated*/
            ITEM_ATTR_SPEED,
            ITEM_ATTR_SLOT,				/*deprecated*/
            ITEM_ATTR_MAXITEMS,			/*deprecated*/
            ITEM_ATTR_WEIGHT,			/*deprecated*/
            ITEM_ATTR_WEAPON,			/*deprecated*/
            ITEM_ATTR_AMU,				/*deprecated*/
            ITEM_ATTR_ARMOR,			/*deprecated*/
            ITEM_ATTR_MAGLEVEL,			/*deprecated*/
            ITEM_ATTR_MAGFIELDTYPE,		/*deprecated*/
            ITEM_ATTR_WRITEABLE,		/*deprecated*/
            ITEM_ATTR_ROTATETO,			/*deprecated*/
            ITEM_ATTR_DECAY,			/*deprecated*/
            ITEM_ATTR_SPRITEHASH,
            ITEM_ATTR_MINIMAPCOLOR,
            ITEM_ATTR_07,
            ITEM_ATTR_08,
            ITEM_ATTR_LIGHT,			/*deprecated*/

            //1-byte aligned
            ITEM_ATTR_DECAY2,			/*deprecated*/
            ITEM_ATTR_WEAPON2,			/*deprecated*/
            ITEM_ATTR_AMU2,				/*deprecated*/
            ITEM_ATTR_ARMOR2,			/*deprecated*/
            ITEM_ATTR_WRITEABLE2,		/*deprecated*/
            ITEM_ATTR_LIGHT2,
            ITEM_ATTR_TOPORDER,
            ITEM_ATTR_WRITEABLE3,		/*deprecated*/
            ITEM_ATTR_WAREID,

            ITEM_ATTR_LAST
        };

        private enum RootAttr
        {
            ROOT_ATTR_VERSION = 0x01
        };

        [FlagsAttribute]
        private enum ItemFlags
        {
            BLOCK_SOLID = 1,
            BLOCK_PROJECTILE = 2,
            BLOCK_PATHFIND = 4,
            HAS_HEIGHT = 8,
            USEABLE = 16,
            PICKUPABLE = 32,
            MOVEABLE = 64,
            STACKABLE = 128,
            FLOORCHANGEDOWN = 256,
            FLOORCHANGENORTH = 512,
            FLOORCHANGEEAST = 1024,
            FLOORCHANGESOUTH = 2048,
            FLOORCHANGEWEST = 4096,
            ALWAYSONTOP = 8192,
            READABLE = 16384,
            ROTABLE = 32768,
            HANGABLE = 65536,
            VERTICAL = 131072,
            HORIZONTAL = 262144,
            CANNOTDECAY = 524288,		/*deprecated*/
            ALLOWDISTREAD = 1048576,
            CORPSE = 2097152,			/*deprecated*/
            CLIENTCHARGES = 4194304,	/*deprecated*/
            LOOKTHROUGH = 8388608,
            ANIMATION = 16777216,
            WALKSTACK = 33554432
        };

        private class OtbLoader : BinaryReader
        {
            private enum SpecialChars
            {
                NODE_START = 0xFE,
                NODE_END = 0xFF,
                ESCAPE_CHAR = 0xFD,
            };

            public OtbLoader(Stream input)
                : base(input)
            {
                //
            }

            public BinaryReader getRootNode()
            {
                return getChildNode();
            }

            public BinaryReader getChildNode()
            {
                advance();
                return getNodeData();
            }

            public BinaryReader getNextNode()
            {
                BaseStream.Seek(currentNodePos, SeekOrigin.Begin);

                byte value = ReadByte();
                if ((SpecialChars)value != SpecialChars.NODE_START)
                {
                    return null;
                }

                value = ReadByte();

                Int32 level = 1;
                while (true)
                {
                    value = ReadByte();
                    if ((SpecialChars)value == SpecialChars.NODE_END)
                    {
                        --level;
                        if (level == 0)
                        {
                            value = ReadByte();
                            if ((SpecialChars)value == SpecialChars.NODE_END)
                            {
                                return null;
                            }
                            else if ((SpecialChars)value != SpecialChars.NODE_START)
                            {
                                return null;
                            }
                            else
                            {
                                currentNodePos = BaseStream.Position - 1;
                                return getNodeData();
                            }
                        }
                    }
                    else if ((SpecialChars)value == SpecialChars.NODE_START)
                    {
                        ++level;
                    }
                    else if ((SpecialChars)value == SpecialChars.ESCAPE_CHAR)
                    {
                        ReadByte();
                    }
                }
            }

            private BinaryReader getNodeData()
            {
                BaseStream.Seek(currentNodePos, SeekOrigin.Begin);

                //read node type
                byte value = ReadByte();

                if ((SpecialChars)value != SpecialChars.NODE_START)
                {
                    return null;
                }

                MemoryStream ms = new MemoryStream(200);

                currentNodeSize = 0;
                while (true)
                {
                    value = ReadByte();
                    if ((SpecialChars)value == SpecialChars.NODE_END || (SpecialChars)value == SpecialChars.NODE_START)
                        break;
                    else if ((SpecialChars)value == SpecialChars.ESCAPE_CHAR)
                    {
                        value = ReadByte();
                    }
                    ++currentNodeSize;
                    ms.WriteByte(value);
                }

                BaseStream.Seek(currentNodePos, SeekOrigin.Begin);
                ms.Position = 0;
                return new BinaryReader(ms);
            }

            private bool advance()
            {
                try
                {
                    Int64 seekPos = 0;
                    if (currentNodePos == 0)
                    {
                        seekPos = 4;
                    }
                    else
                    {
                        seekPos = currentNodePos;
                    }

                    BaseStream.Seek(seekPos, SeekOrigin.Begin);

                    byte value = ReadByte();
                    if ((SpecialChars)value != SpecialChars.NODE_START)
                    {
                        return false;
                    }

                    if (currentNodePos == 0)
                    {
                        currentNodePos = BaseStream.Position - 1;
                        return true;
                    }
                    else
                    {
                        value = ReadByte();

                        while (true)
                        {
                            value = ReadByte();
                            if ((SpecialChars)value == SpecialChars.NODE_END)
                            {
                                return false;
                            }
                            else if ((SpecialChars)value == SpecialChars.NODE_START)
                            {
                                currentNodePos = BaseStream.Position - 1;
                                return true;
                            }
                            else if ((SpecialChars)value == SpecialChars.ESCAPE_CHAR)
                            {
                                ReadByte();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public void createNode(byte type)
            {
                writeByte((byte)SpecialChars.NODE_START, false);
                writeByte(type);
            }

            public void closeNode()
            {
                writeByte((byte)SpecialChars.NODE_END, false);
            }

            public void writeByte(byte value)
            {
                byte[] bytes = new byte[1] { value };
                writeBytes(bytes, true);
            }

            public void writeByte(byte value, bool unescape)
            {
                byte[] bytes = new byte[1] { value };
                writeBytes(bytes, unescape);
            }

            public void writeUInt16(UInt16 value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                writeBytes(bytes, true);
            }

            public void writeUInt16(UInt16 value, bool unescape)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                writeBytes(bytes, unescape);
            }

            public void writeUInt32(UInt32 value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                writeBytes(bytes, true);
            }

            public void writeUInt32(UInt32 value, bool unescape)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                writeBytes(bytes, unescape);
            }

            public void writeProp(ItemAttr attr, BinaryWriter writer)
            {
                writer.BaseStream.Position = 0;
                byte[] bytes = new byte[writer.BaseStream.Length];
                writer.BaseStream.Read(bytes, 0, (int)writer.BaseStream.Length);
                writer.BaseStream.Position = 0;
                writer.BaseStream.SetLength(0);

                writeProp((byte)attr, bytes);
            }

            public void writeProp(RootAttr attr, BinaryWriter writer)
            {
                writer.BaseStream.Position = 0;
                byte[] bytes = new byte[writer.BaseStream.Length];
                writer.BaseStream.Read(bytes, 0, (int)writer.BaseStream.Length);
                writer.BaseStream.Position = 0;
                writer.BaseStream.SetLength(0);

                writeProp((byte)attr, bytes);
            }

            private void writeProp(byte attr, byte[] bytes)
            {
                writeByte((byte)attr);
                writeUInt16((UInt16)bytes.Length);
                writeBytes(bytes, true);
            }

            public void writeBytes(byte[] bytes, bool unescape)
            {
                foreach (byte b in bytes)
                {
                    if (unescape && (b == (byte)SpecialChars.NODE_START || b == (byte)SpecialChars.NODE_END || b == (byte)SpecialChars.ESCAPE_CHAR))
                    {
                        BaseStream.WriteByte((byte)SpecialChars.ESCAPE_CHAR);
                    }

                    BaseStream.WriteByte(b);
                }
            }

            public Int64 currentNodePos = 0;
            public UInt32 currentNodeSize = 0;
        };

        public Otb Open(string filename, bool outputDebug = false)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            Otb otb = new Otb();

            try
            {
                using (OtbLoader reader = new OtbLoader(fileStream))
                {
                    //get root node
                    BinaryReader nodeReader = reader.getRootNode();
                    if (nodeReader == null)
                        throw new Exception("Null root node.");

                    nodeReader.ReadByte(); //first byte of otb is 0
                    nodeReader.ReadUInt32(); //4 bytes flags, unused

                    byte attr = nodeReader.ReadByte();
                    if ((RootAttr)attr == RootAttr.ROOT_ATTR_VERSION)
                    {
                        UInt16 datalen = nodeReader.ReadUInt16();
                        if (datalen != 4 + 4 + 4 + 1 * 128)
                            throw new Exception("Size of version header is invalid.");

                        otb.MajorVersion = nodeReader.ReadUInt32(); //major, file version
                        otb.MinorVersion = nodeReader.ReadUInt32(); //minor, client version
                        otb.BuildNumber = nodeReader.ReadUInt32(); //build number, revision
                        nodeReader.BaseStream.Seek(128, SeekOrigin.Current);
                    }

                    nodeReader = reader.getChildNode();
                    if (nodeReader == null)
                        throw new Exception("Null child node.");

                    do
                    {
                        OtbItem item = new OtbItem();

                        byte itemGroup = nodeReader.ReadByte();
                        if (outputDebug)
                        {
                            Trace.WriteLine(String.Format("Node:ItemGroup {0}", (ItemGroup)itemGroup));
                        }

                        switch ((ItemGroup)itemGroup)
                        {
                            case ItemGroup.NONE: item.Type = OtbItemType.None; break;
                            case ItemGroup.GROUND: item.Type = OtbItemType.Ground; break;
                            case ItemGroup.SPLASH: item.Type = OtbItemType.Splash; break;
                            case ItemGroup.FLUID: item.Type = OtbItemType.FluidContainer; break;
                            case ItemGroup.CONTAINER: item.Type = OtbItemType.Container; break;
                            case ItemGroup.DEPRECATED: item.Type = OtbItemType.Deprecated; break;
                            default: break;
                        }

                        ItemFlags flags = (ItemFlags)nodeReader.ReadUInt32();
                        if (outputDebug)
                        {
                            Trace.WriteLine(String.Format("Node:flags {0}", flags));
                        }

                        item.BlockObject = ((flags & ItemFlags.BLOCK_SOLID) == ItemFlags.BLOCK_SOLID);
                        item.BlockProjectile = ((flags & ItemFlags.BLOCK_PROJECTILE) == ItemFlags.BLOCK_PROJECTILE);
                        item.BlockPathFind = ((flags & ItemFlags.BLOCK_PATHFIND) == ItemFlags.BLOCK_PATHFIND);
                        item.IsPickupable = ((flags & ItemFlags.PICKUPABLE) == ItemFlags.PICKUPABLE);
                        item.IsMoveable = ((flags & ItemFlags.MOVEABLE) == ItemFlags.MOVEABLE);
                        item.IsStackable = ((flags & ItemFlags.STACKABLE) == ItemFlags.STACKABLE);
                        item.AlwaysOnTop = ((flags & ItemFlags.ALWAYSONTOP) == ItemFlags.ALWAYSONTOP);
                        item.IsVertical = ((flags & ItemFlags.VERTICAL) == ItemFlags.VERTICAL);
                        item.IsHorizontal = ((flags & ItemFlags.HORIZONTAL) == ItemFlags.HORIZONTAL);
                        item.IsHangable = ((flags & ItemFlags.HANGABLE) == ItemFlags.HANGABLE);
                        item.IsRotatable = ((flags & ItemFlags.ROTABLE) == ItemFlags.ROTABLE);
                        item.IsReadable = ((flags & ItemFlags.READABLE) == ItemFlags.READABLE);
                        item.HasUseWith = ((flags & ItemFlags.USEABLE) == ItemFlags.USEABLE);
                        item.HasHeight = ((flags & ItemFlags.HAS_HEIGHT) == ItemFlags.HAS_HEIGHT);
                        item.LookThrough = ((flags & ItemFlags.LOOKTHROUGH) == ItemFlags.LOOKTHROUGH);
                        item.AllowDistRead = ((flags & ItemFlags.ALLOWDISTREAD) == ItemFlags.ALLOWDISTREAD);
                        item.IsAnimation = ((flags & ItemFlags.ANIMATION) == ItemFlags.ANIMATION);
                        item.WalkStack = ((flags & ItemFlags.WALKSTACK) == ItemFlags.WALKSTACK);

                        while (nodeReader.PeekChar() != -1)
                        {
                            byte attribute = nodeReader.ReadByte();
                            UInt16 datalen = nodeReader.ReadUInt16();

                            if (outputDebug)
                            {
                                Trace.WriteLine(String.Format("Node[{0}]:attribut {1}, size: {2}",
                                    reader.currentNodePos, ((ItemAttr)attribute), datalen, reader.currentNodePos + nodeReader.BaseStream.Position));
                            }

                            switch ((ItemAttr)attribute)
                            {
                                case ItemAttr.ITEM_ATTR_SERVERID:
                                    if (datalen != sizeof(UInt16))
                                        throw new Exception("Unexpected data length of server id block (Should be 2 bytes)");

                                    item.Id = nodeReader.ReadUInt16();
                                    if (outputDebug)
                                        System.Diagnostics.Debug.WriteLine(String.Format("Node:attribute:data {0}", item.Id));

                                    break;

                                case ItemAttr.ITEM_ATTR_CLIENTID:
                                    if (datalen != sizeof(UInt16))
                                        throw new Exception("Unexpected data length of client id block (Should be 2 bytes)");

                                    item.SpriteId = nodeReader.ReadUInt16();
                                    if (outputDebug)
                                        Trace.WriteLine(String.Format("Node:attribute:data {0}", item.SpriteId));
                                    break;

                                case ItemAttr.ITEM_ATTR_WAREID:
                                    if (datalen != sizeof(UInt16))
                                        throw new Exception("Unexpected data length of ware id block (Should be 2 bytes)");

                                    item.WareId = nodeReader.ReadUInt16();
                                    if (outputDebug)
                                        Trace.WriteLine(String.Format("Node:attribute:data {0}", item.WareId));
                                    break;

                                case ItemAttr.ITEM_ATTR_SPEED:
                                    if (datalen != sizeof(UInt16))
                                        throw new Exception("Unexpected data length of speed block (Should be 2 bytes)");

                                    item.GroundSpeed = nodeReader.ReadUInt16();
                                    if (outputDebug)
                                        Trace.WriteLine(String.Format("Node:attribute:data {0}", item.GroundSpeed));
                                    break;

                                case ItemAttr.ITEM_ATTR_NAME:
                                    item.Name = new string(nodeReader.ReadChars(datalen));
                                    if (outputDebug)
                                        Trace.WriteLine(String.Format("Node:attribute:data {0}", item.Name));
                                    break;

                                case ItemAttr.ITEM_ATTR_SPRITEHASH:
                                    if (datalen != 16)
                                        throw new Exception("Unexpected data length of sprite hash (Should be 16 bytes)");

                                    item.SpriteHash = nodeReader.ReadBytes(16);
                                    if (outputDebug)
                                        Trace.WriteLine(String.Format("Node:attribute:data {0}", item.AlwaysOnTopOrder));
                                    break;

                                case ItemAttr.ITEM_ATTR_MINIMAPCOLOR:
                                    if (datalen != 2)
                                        throw new Exception("Unexpected data length of minimap color (Should be 2 bytes)");

                                    item.MinimapColor = nodeReader.ReadUInt16();
                                    if (outputDebug)
                                        Trace.WriteLine(String.Format("Node:attribute:data {0}", item.MinimapColor));
                                    break;

                                case ItemAttr.ITEM_ATTR_07:
                                    //read/write-able
                                    if (datalen != 2)
                                        throw new Exception("Unexpected data length of attr 07 (Should be 2 bytes)");

                                    item.MaxReadWriteChars = nodeReader.ReadUInt16();
                                    break;

                                case ItemAttr.ITEM_ATTR_08:
                                    //readable
                                    if (datalen != 2)
                                        throw new Exception("Unexpected data length of attr 08 (Should be 2 bytes)");

                                    item.MaxReadChars = nodeReader.ReadUInt16();
                                    break;

                                case ItemAttr.ITEM_ATTR_LIGHT2:
                                    if (datalen != sizeof(UInt16) * 2)
                                        throw new Exception("Unexpected data length of item light (2) block");

                                    item.LightLevel = nodeReader.ReadUInt16();
                                    item.LightColor = nodeReader.ReadUInt16();
                                    if (outputDebug)
                                        Trace.WriteLine(String.Format("Node:attribute:data {0}, {1}", item.LightLevel, item.LightColor));
                                    break;

                                case ItemAttr.ITEM_ATTR_TOPORDER:
                                    if (datalen != sizeof(byte))
                                        throw new Exception("Unexpected data length of item toporder block (Should be 1 byte)");

                                    item.AlwaysOnTopOrder = nodeReader.ReadByte();
                                    if (outputDebug)
                                        Trace.WriteLine(String.Format("Node:attribute:data {0}", item.AlwaysOnTopOrder));
                                    break;

                                default:
                                    //skip unknown attributes
                                    nodeReader.BaseStream.Seek(datalen, SeekOrigin.Current);
                                    if (outputDebug)
                                        Trace.WriteLine(String.Format("Skipped unknown attribute"));
                                    break;
                            }
                        }

                        otb.AddItem(item);

                        nodeReader = reader.getNextNode();
                    } while (nodeReader != null);
                }
            }
            finally
            {
                fileStream.Close();
            }

            return otb;
        }

    }

    public class Otb
    {
        private Dictionary<ushort, OtbItem> clientItemMap = new Dictionary<ushort, OtbItem>();
        private Dictionary<ushort, OtbItem> serverItemMap = new Dictionary<ushort, OtbItem>();

        public uint MajorVersion { get; set; }

        public uint MinorVersion { get; set; }

        public uint BuildNumber { get; set; }

        public void AddItem(OtbItem item)
        {
            serverItemMap[item.Id] = item;

            //if (!clientItemMap.ContainsKey(item.SpriteId))
                clientItemMap[item.SpriteId] = item;
            //else
            //    Trace.TraceWarning("Duplicate client item " + item.Id + " and " + clientItemMap[item.SpriteId].Id); 
        }

        public OtbItem GetItem(ushort id)
        {
            if (serverItemMap.ContainsKey(id))
                return serverItemMap[id];

            return null;
        }

        public OtbItem GetItemBySpriteId(ushort spriteId)
        {
            if (clientItemMap.ContainsKey(spriteId))
                return clientItemMap[spriteId];

            return null;
        }

        //public static bool save(string filename, ref OtbList items)
        //{
        //    FileStream fileStream = new FileStream(filename, FileMode.Create);
        //    try
        //    {
        //        using (OtbLoader writer = new OtbLoader(fileStream))
        //        {
        //            writer.writeUInt32(0, false); //version, always 0

        //            writer.createNode(0); //root node
        //            writer.writeUInt32(0, true); //flags, unused for root node

        //            VersionInfo vi = new VersionInfo();

        //            vi.dwMajorVersion = items.dwMajorVersion;
        //            vi.dwMinorVersion = items.dwMinorVersion;
        //            vi.dwBuildNumber = items.dwBuildNumber;
        //            vi.CSDVersion = String.Format("OTB {0}.{1}.{2}-{3}.{4}", vi.dwMajorVersion, vi.dwMinorVersion, vi.dwBuildNumber, items.clientVersion / 100, items.clientVersion % 100);

        //            MemoryStream ms = new MemoryStream();
        //            BinaryWriter property = new BinaryWriter(ms);
        //            property.Write(vi.dwMajorVersion);
        //            property.Write(vi.dwMinorVersion);
        //            property.Write(vi.dwBuildNumber);
        //            byte[] CSDVersion = System.Text.Encoding.ASCII.GetBytes(vi.CSDVersion);
        //            Array.Resize(ref CSDVersion, 128);
        //            property.Write(CSDVersion);

        //            writer.writeProp(RootAttr.ROOT_ATTR_VERSION, property);

        //            foreach (OtbItem item in items)
        //            {
        //                List<ItemAttr> saveAttributeList = new List<ItemAttr>();

        //                saveAttributeList.Add(ItemAttr.ITEM_ATTR_SERVERID);

        //                if (item.type == ItemType.Deprecated)
        //                {
        //                    //no other attributes should be saved for this type
        //                }
        //                else
        //                {
        //                    saveAttributeList.Add(ItemAttr.ITEM_ATTR_CLIENTID);
        //                    saveAttributeList.Add(ItemAttr.ITEM_ATTR_SPRITEHASH);

        //                    if (item.minimapColor != 0)
        //                    {
        //                        saveAttributeList.Add(ItemAttr.ITEM_ATTR_MINIMAPCOLOR);
        //                    }

        //                    if (item.maxReadWriteChars != 0)
        //                    {
        //                        saveAttributeList.Add(ItemAttr.ITEM_ATTR_07);
        //                    }

        //                    if (item.maxReadChars != 0)
        //                    {
        //                        saveAttributeList.Add(ItemAttr.ITEM_ATTR_08);
        //                    }

        //                    if (item.lightLevel != 0 || item.lightColor != 0)
        //                    {
        //                        saveAttributeList.Add(ItemAttr.ITEM_ATTR_LIGHT2);
        //                    }

        //                    if (item.type == ItemType.Ground)
        //                    {
        //                        saveAttributeList.Add(ItemAttr.ITEM_ATTR_SPEED);
        //                    }

        //                    if (item.alwaysOnTop)
        //                    {
        //                        saveAttributeList.Add(ItemAttr.ITEM_ATTR_TOPORDER);
        //                    }

        //                    if (item.wareId != 0)
        //                    {
        //                        saveAttributeList.Add(ItemAttr.ITEM_ATTR_WAREID);
        //                    }

        //                    if (!string.IsNullOrEmpty(item.name))
        //                    {
        //                        saveAttributeList.Add(ItemAttr.ITEM_ATTR_NAME);
        //                    }
        //                }

        //                switch (item.type)
        //                {
        //                    case ItemType.Container: writer.createNode((byte)ItemGroup.CONTAINER); break;
        //                    case ItemType.Fluid: writer.createNode((byte)ItemGroup.FLUID); break;
        //                    case ItemType.Ground: writer.createNode((byte)ItemGroup.GROUND); break;
        //                    case ItemType.Splash: writer.createNode((byte)ItemGroup.SPLASH); break;
        //                    case ItemType.Deprecated: writer.createNode((byte)ItemGroup.DEPRECATED); break;
        //                    default: writer.createNode((byte)ItemGroup.NONE); break;
        //                }

        //                UInt32 flags = 0;
        //                if (item.blockObject)
        //                    flags |= (UInt32)ItemFlags.BLOCK_SOLID;

        //                if (item.blockProjectile)
        //                    flags |= (UInt32)ItemFlags.BLOCK_PROJECTILE;

        //                if (item.blockPathFind)
        //                    flags |= (UInt32)ItemFlags.BLOCK_PATHFIND;

        //                if (item.hasHeight)
        //                    flags |= (UInt32)ItemFlags.HAS_HEIGHT;

        //                if (item.hasUseWith)
        //                    flags |= (UInt32)ItemFlags.USEABLE;

        //                if (item.isPickupable)
        //                    flags |= (UInt32)ItemFlags.PICKUPABLE;

        //                if (item.isMoveable)
        //                    flags |= (UInt32)ItemFlags.MOVEABLE;

        //                if (item.isStackable)
        //                    flags |= (UInt32)ItemFlags.STACKABLE;

        //                if (item.alwaysOnTop)
        //                    flags |= (UInt32)ItemFlags.ALWAYSONTOP;

        //                if (item.isReadable)
        //                    flags |= (UInt32)ItemFlags.READABLE;

        //                if (item.isRotatable)
        //                    flags |= (UInt32)ItemFlags.ROTABLE;

        //                if (item.isHangable)
        //                    flags |= (UInt32)ItemFlags.HANGABLE;

        //                if (item.isVertical)
        //                    flags |= (UInt32)ItemFlags.VERTICAL;

        //                if (item.isHorizontal)
        //                    flags |= (UInt32)ItemFlags.HORIZONTAL;

        //                if (item.lookThrough)
        //                    flags |= (UInt32)ItemFlags.LOOKTHROUGH;

        //                if (item.allowDistRead)
        //                    flags |= (UInt32)ItemFlags.ALLOWDISTREAD;

        //                if (item.isAnimation)
        //                    flags |= (UInt32)ItemFlags.ANIMATION;

        //                if (item.walkStack)
        //                    flags |= (UInt32)ItemFlags.WALKSTACK;

        //                writer.writeUInt32(flags, true);

        //                foreach (ItemAttr attribute in saveAttributeList)
        //                {
        //                    switch (attribute)
        //                    {
        //                        case ItemAttr.ITEM_ATTR_SERVERID:
        //                            {
        //                                property.Write((UInt16)item.id);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_SERVERID, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_WAREID:
        //                            {
        //                                property.Write((UInt16)item.wareId);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_WAREID, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_CLIENTID:
        //                            {
        //                                property.Write((UInt16)item.spriteId);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_CLIENTID, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_SPEED:
        //                            {
        //                                property.Write((UInt16)item.groundSpeed);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_SPEED, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_NAME:
        //                            {
        //                                for (UInt16 i = 0; i < item.name.Length; ++i)
        //                                {
        //                                    property.Write((char)item.name[i]);
        //                                }

        //                                writer.writeProp(ItemAttr.ITEM_ATTR_NAME, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_SPRITEHASH:
        //                            {
        //                                property.Write(item.spriteHash);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_SPRITEHASH, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_MINIMAPCOLOR:
        //                            {
        //                                property.Write((UInt16)item.minimapColor);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_MINIMAPCOLOR, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_07:
        //                            {
        //                                property.Write((UInt16)item.maxReadWriteChars);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_07, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_08:
        //                            {
        //                                property.Write((UInt16)item.maxReadChars);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_08, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_LIGHT2:
        //                            {
        //                                property.Write((UInt16)item.lightLevel);
        //                                property.Write((UInt16)item.lightColor);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_LIGHT2, property);
        //                                break;
        //                            }

        //                        case ItemAttr.ITEM_ATTR_TOPORDER:
        //                            {
        //                                property.Write((byte)item.alwaysOnTopOrder);
        //                                writer.writeProp(ItemAttr.ITEM_ATTR_TOPORDER, property);
        //                                break;
        //                            }
        //                    }
        //                }

        //                writer.closeNode();
        //            }

        //            writer.closeNode();
        //        }
        //    }
        //    finally
        //    {
        //        fileStream.Close();
        //    }

        //    return true;
        //}
    }

}
