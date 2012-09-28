using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpMapTracker
{
    public class FileLoader : BinaryReader
    {
        private enum SpecialChars
        {
            NODE_START = 0xFE,
            NODE_END = 0xFF,
            ESCAPE_CHAR = 0xFD,
        };

        public FileLoader(Stream input)
            : base(input)
        {
            //
        }

        public BinaryReader GetRootNode()
        {
            return GetChildNode();
        }

        public BinaryReader GetChildNode()
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
            return new FileLoader(ms);
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

        public override string ReadString()
        {
            var len = ReadUInt16();
            return new string(ReadChars(len));
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

        public void writeProp(byte attr, BinaryWriter writer)
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

}
