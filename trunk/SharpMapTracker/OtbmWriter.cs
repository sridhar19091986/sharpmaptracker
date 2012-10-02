using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using SharpTibiaProxy.Domain;
using System.Xml.Linq;
using System.Linq;

namespace SharpMapTracker
{
    public class OtbmWriter
    {
        private const byte NodeStart = 0xFE;
        private const byte NodeEnd = 0xFF;
        private const byte Escape = 0xFD;

        #region Vars/Properties
        private string fileName;
        private FileStream fileStream;
        public bool CanWrite { get; private set; }
        #endregion

        #region Constructor
        private OtbmWriter(string fileName)
        {
            this.fileName = fileName;
            CanWrite = OpenFile();
        }
        #endregion

        #region Open/Close
        private bool OpenFile()
        {
            try
            {
                fileStream = File.OpenWrite(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Close()
        {
            fileStream.Close();
        }
        #endregion

        #region Write
        private void WriteHeader(ClientVersion version)
        {
            // Version, unescaped
            WriteUInt32(0, false);

            // Root node
            WriteNodeStart(OtMap.OtMapNodeTypes.ROOTV1);

            // Header information
            // Version
            WriteUInt32(version.OtbmVersion);
            // Width
            WriteUInt16(0xFCFC);
            // Height
            WriteUInt16(0xFCFC);

            // Major version items
            WriteUInt32(version.OtbMajorVersion);
            // Minor version items
            WriteUInt32(version.OtbMinorVersion);
        }

        private void WriteMapStart(string houseFileName, string spawnFileName)
        {
            WriteNodeStart(OtMap.OtMapNodeTypes.MAP_DATA);

            WriteByte((byte)OtMap.OtMapAttrTypes.DESCRIPTION);
            WriteString("Created with SharpMapTracker v" + Constants.MAP_TRACKER_VERSION);

            WriteByte((byte)OtMap.OtMapAttrTypes.EXT_HOUSE_FILE);
            WriteString(houseFileName);

            WriteByte((byte)OtMap.OtMapAttrTypes.EXT_SPAWN_FILE);
            WriteString(spawnFileName);
        }

        private void WriteNodeStart(OtMap.OtMapNodeTypes type)
        {
            WriteByte(NodeStart, false);
            WriteByte((byte)type);
        }

        private void WriteNodeEnd()
        {
            WriteByte(NodeEnd, false);
        }

        private void WriteBytes(byte[] data, bool unescape)
        {
            foreach (byte b in data)
            {
                if (unescape && (b == NodeStart || b == NodeEnd || b == Escape))
                {
                    fileStream.WriteByte(Escape);
                }

                fileStream.WriteByte(b);
            }
        }

        private void WriteBytes(byte[] data)
        {
            WriteBytes(data, true);
        }

        private void WriteByte(byte value)
        {
            WriteByte(value, true);
        }

        private void WriteByte(byte value, bool unescape)
        {
            WriteBytes(new byte[] { value }, unescape);
        }

        private void WriteUInt16(UInt16 value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        private void WriteUInt32(UInt32 value)
        {
            WriteUInt32(value, true);
        }

        private void WriteUInt32(UInt32 value, bool unescape)
        {
            WriteBytes(BitConverter.GetBytes(value), unescape);
        }

        private void WriteString(string text)
        {
            WriteUInt16((UInt16)text.Length);
            WriteBytes(Encoding.ASCII.GetBytes(text));
        }

        private void WriteTileAreaCoords(Location loc)
        {
            WriteUInt16((UInt16)(loc.X & 0xFF00));
            WriteUInt16((UInt16)(loc.Y & 0xFF00));
            WriteByte((byte)loc.Z);
        }

        private void WriteTileCoords(Location loc)
        {
            WriteByte((byte)loc.X);
            WriteByte((byte)loc.Y);
        }
        #endregion

        #region Static Methods
        public static void WriteMapToFile(String filename, OtMap map, ClientVersion version)
        {
            var dir = Path.GetDirectoryName(filename);
            var baseFileName = Path.GetFileNameWithoutExtension(filename);
            string otbmFileName = baseFileName + ".otbm";
            string houseFileName = baseFileName + "-house.xml";
            string spawnFileName = baseFileName + "-spawn.xml";
            WriteOtbm(Path.Combine(dir, otbmFileName), houseFileName, spawnFileName, version, map.Tiles);
            WriteHouses(Path.Combine(dir, baseFileName + "-house.xml"));
            WriteCreatures(Path.Combine(dir, baseFileName + "-spawn.xml"), map.Tiles);
        }

        private static void WriteCreatures(string spawnFileName, IEnumerable<OtMapTile> mapTiles)
        {
            XElement spawns = new XElement("spawns");

            foreach (var tile in mapTiles.Where(x => x.Creature != null))
            {
                XElement spawn = new XElement("spawn");
                spawn.Add(new XAttribute("centerx", (tile.Location.X - 1).ToString()));
                spawn.Add(new XAttribute("centery", tile.Location.Y.ToString()));
                spawn.Add(new XAttribute("centerz", tile.Location.Z.ToString()));
                spawn.Add(new XAttribute("radius", "1"));

                XElement creatureSpawn = new XElement(tile.Creature.Type == CreatureType.NPC ? "npc" : "monster");
                creatureSpawn.Add(new XAttribute("name", tile.Creature.Name));
                creatureSpawn.Add(new XAttribute("x", "1"));
                creatureSpawn.Add(new XAttribute("y", "0"));
                creatureSpawn.Add(new XAttribute("z", tile.Location.Z.ToString()));
                creatureSpawn.Add(new XAttribute("spawntime", "60"));

                spawn.Add(creatureSpawn);
                spawns.Add(spawn);
            }

            var xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Encoding = ASCIIEncoding.UTF8;
            xmlWriterSettings.Indent = true;
            using (var xmlWriter = XmlWriter.Create(spawnFileName, xmlWriterSettings))
            {
                spawns.Save(xmlWriter);
            }
        }

        private static void WriteHouses(string housesFileName)
        {
            XElement houses = new XElement("houses");

            var xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Encoding = ASCIIEncoding.UTF8;
            xmlWriterSettings.Indent = true;
            using (var xmlWriter = XmlWriter.Create(housesFileName, xmlWriterSettings))
            {
                houses.Save(xmlWriter);
            }
        }

        private static void WriteOtbm(string otbmFileName, string houseFileName, string spawnFileName, ClientVersion version, IEnumerable<OtMapTile> mapTiles)
        {
            OtbmWriter mapWriter = new OtbmWriter(otbmFileName);
            mapWriter.WriteHeader(version);
            mapWriter.WriteMapStart(houseFileName, spawnFileName);
            foreach (OtMapTile tile in mapTiles)
            {
                mapWriter.WriteNodeStart(OtMap.OtMapNodeTypes.TILE_AREA);
                mapWriter.WriteTileAreaCoords(tile.Location);
                mapWriter.WriteNodeStart(OtMap.OtMapNodeTypes.TILE);
                mapWriter.WriteTileCoords(tile.Location);

                if (tile.TileId > 0)
                {
                    mapWriter.WriteByte((byte)OtMap.OtMapAttrTypes.ITEM);
                    mapWriter.WriteUInt16(tile.TileId);
                }

                foreach (OtMapItem item in tile.Items)
                {
                    mapWriter.WriteNodeStart(OtMap.OtMapNodeTypes.ITEM);
                    mapWriter.WriteUInt16(item.Info.Id);
                    if (item.AttrType != OtMapItemAttrTypes.NONE)
                    {
                        mapWriter.WriteByte((byte)item.AttrType);
                        mapWriter.WriteByte(item.Extra);
                    }
                    mapWriter.WriteNodeEnd();
                }

                mapWriter.WriteNodeEnd();
                mapWriter.WriteNodeEnd();
            }
            mapWriter.WriteNodeEnd(); // Map Data node
            mapWriter.WriteNodeEnd(); // Root node
            mapWriter.Close();
        }
        #endregion
    }
}
