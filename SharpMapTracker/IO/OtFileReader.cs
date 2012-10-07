using System.IO;

namespace SharpMapTracker.IO
{
    public class OtFileReader
    {
        protected byte[] _buffer;
        protected FileStream _fileStream;
        protected BinaryReader _reader;
        protected OtFileNode _root;

        public OtFileNode GetRootNode()
        {
            return _root;
        }

        public bool Open(string fileName)
        {
            _fileStream = File.Open(fileName, FileMode.Open);
            _reader = new BinaryReader(_fileStream);

            var version = _reader.ReadUInt32();

            if(version > 0)
            {
                _reader.Close();
                return false;
            }

            if(SafeSeek(4))
            {
                _root = new OtFileNode {Start = 4};

                if (_reader.ReadByte() == OtFileNode.NODE_START)
                    return ParseNode(_root);
            }

            return false;
        }

        public void Close()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
            }
        }

        private bool ParseNode(OtFileNode node)
        {
            var currentNode = node;
            int val;

            while (true)
            {
                // read node type
                val = _fileStream.ReadByte();
                if (val != -1)
                {
                    
                    currentNode.Type = val;
                    var setPropSize = false;

                    while (true)
                    {
                        // search child and next node
                        val = _fileStream.ReadByte();

                        
                        if (val == OtFileNode.NODE_START)
                        {
                            var childNode = new OtFileNode {Start = _fileStream.Position};
                            setPropSize = true;

                            currentNode.PropsSize = _fileStream.Position - currentNode.Start - 2;
                            currentNode.Child = childNode;

                            if (!ParseNode(childNode))
                                return false;
                        }
                        else if (val == OtFileNode.NODE_END)
                        {
                            if (!setPropSize)
                                currentNode.PropsSize = _fileStream.Position - currentNode.Start - 2;

                            val = _fileStream.ReadByte();

                            if (val != -1)
                            {
                                if (val == OtFileNode.NODE_START)
                                {
                                    // start next node
                                    var nextNode = new OtFileNode {Start = _fileStream.Position};
                                    currentNode.Next = nextNode;
                                    currentNode = nextNode;
                                    break;
                                }

                                if (val == OtFileNode.NODE_END)
                                {
                                    // up 1 level and move 1 position back
                                    // safeTell(pos) && safeSeek(pos)
                                    _fileStream.Seek(-1, SeekOrigin.Current);
                                    return true;
                                }
                                
                                // bad format
                                return false;

                            }

                            // end of file?
                            return true;
                        }
                        else if (val == OtFileNode.ESCAPE)
                        {
                            _fileStream.ReadByte();
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private byte[] GetProps(OtFileNode node, out long size)
        {
            if (_buffer == null || _buffer.Length < node.PropsSize)
                _buffer = new byte[node.PropsSize];

            _fileStream.Seek(node.Start + 1, SeekOrigin.Begin);
            _fileStream.Read(_buffer, 0, (int)node.PropsSize);

            uint j = 0;
            var escaped = false;

            for (uint i = 0; i < node.PropsSize; ++i, ++j)
            {
                if (_buffer[i] == OtFileNode.ESCAPE)
                {
                    ++i;
                    _buffer[j] = _buffer[i];
                    escaped = true;
                }
                else if (escaped)
                {
                    _buffer[j] = _buffer[i];
                }
            }
            size = j;
            return _buffer;
        }

        public bool GetProps(OtFileNode node, out OtPropertyReader props)
        {
            long size;
            var buff = GetProps(node, out size);
            
            if (buff == null)
            {
                props = null;
                return false;
            }

            props = new OtPropertyReader(new MemoryStream(buff, 0, (int)size));
            return true;
        }

        protected bool SafeSeek(long pos)
        {
            if (_fileStream == null || _fileStream.Length < pos)
                return false;

            return _fileStream.Seek(pos, SeekOrigin.Begin) == pos;
        }
    }
}