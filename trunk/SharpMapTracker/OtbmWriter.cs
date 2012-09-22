using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using SharpTibiaProxy.Domain;
using System.Xml.Linq;

namespace SharpMapTracker
{
    public class OtbmWriter
    {
        private const byte NodeStart = 0xFE;
        private const byte NodeEnd = 0xFF;
        private const byte Escape = 0xFD;

        private enum OtMapNodeTypes
        {
            ROOTV1 = 0,
            ROOTV2 = 1,
            MAP_DATA = 2,
            ITEM_DEF = 3,
            TILE_AREA = 4,
            TILE = 5,
            ITEM = 6,
            TILE_SQUARE = 7,
            TILE_REF = 8,
            SPAWNS = 9,
            SPAWN_AREA = 10,
            MONSTER = 11,
            TOWNS = 12,
            TOWN = 13,
            HOUSETILE = 14,
            WAYPOINTS = 15,
            WAYPOINT = 16
        };

        private enum OtMapAttrTypes
        {
            DESCRIPTION = 1,
            EXT_FILE = 2,
            TILE_FLAGS = 3,
            ACTION_ID = 4,
            UNIQUE_ID = 5,
            TEXT = 6,
            DESC = 7,
            TELE_DEST = 8,
            ITEM = 9,
            DEPOT_ID = 10,
            EXT_SPAWN_FILE = 11,
            RUNE_CHARGES = 12,
            EXT_HOUSE_FILE = 13,
            HOUSEDOORID = 14,
            COUNT = 15,
            DURATION = 16,
            DECAYING_STATE = 17,
            WRITTENDATE = 18,
            WRITTENBY = 19,
            SLEEPERGUID = 20,
            SLEEPSTART = 21,
            CHARGES = 22,
            CONTAINER_ITEMS = 23,
            ATTRIBUTE_MAP = 128
        };

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
        private void WriteHeader(Version version)
        {
            // Version, unescaped
            WriteUInt32(0, false);

            // Root node
            WriteNodeStart(OtMapNodeTypes.ROOTV1);

            // Header information
            // Version
            WriteUInt32(2);
            // Width
            WriteUInt16(0xFCFC);
            // Height
            WriteUInt16(0xFCFC);

            // Major version items
            WriteUInt32((uint)version.Major);
            // Minor version items
            WriteUInt32((uint)version.Minor);
        }

        private void WriteMapStart(string houseFileName, string spawnFileName)
        {
            WriteNodeStart(OtMapNodeTypes.MAP_DATA);

            WriteByte((byte)OtMapAttrTypes.DESCRIPTION);
            WriteString("Created with SharpMapTracker v" + Constants.MAP_TRACKER_VERSION);

            WriteByte((byte)OtMapAttrTypes.EXT_HOUSE_FILE);
            WriteString(houseFileName);

            WriteByte((byte)OtMapAttrTypes.EXT_SPAWN_FILE);
            WriteString(spawnFileName);
        }

        private void WriteNodeStart(OtMapNodeTypes type)
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
        public static void WriteMapTilesToFile(String filename, IEnumerable<OtMapTile> mapTiles, IEnumerable<OtMapCreature> creatures, Version version)
        {
            var dir = Path.GetDirectoryName(filename);
            var baseFileName = Path.GetFileNameWithoutExtension(filename);
            string otbmFileName = baseFileName + ".otbm";
            string houseFileName = baseFileName + "-house.xml";
            string spawnFileName = baseFileName + "-spawn.xml";
            WriteOtbm(Path.Combine(dir, otbmFileName), houseFileName, spawnFileName, version, mapTiles);
            WriteHouses(Path.Combine(dir, baseFileName + "-house.xml"));
            WriteCreatures(Path.Combine(dir, baseFileName + "-spawn.xml"), creatures);
        }

        private static void WriteCreatures(string spawnFileName, IEnumerable<OtMapCreature> creatures)
        {
            XElement spawns = new XElement("spawns");

            foreach (var creature in creatures)
            {
                XElement spawn = new XElement("spawn");
                spawn.Add(new XAttribute("centerx", (creature.Location.X - 1).ToString()));
                spawn.Add(new XAttribute("centery", creature.Location.Y.ToString()));
                spawn.Add(new XAttribute("centerz", creature.Location.Z.ToString()));
                spawn.Add(new XAttribute("radius", "1"));

                XElement monster = new XElement("monster");
                monster.Add(new XAttribute("name", creature.Name));
                monster.Add(new XAttribute("x", "1"));
                monster.Add(new XAttribute("y", "0"));
                monster.Add(new XAttribute("z", creature.Location.Z.ToString()));
                monster.Add(new XAttribute("spawntime", "60"));

                spawn.Add(monster);
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

        private static void WriteOtbm(string otbmFileName, string houseFileName, string spawnFileName, Version version, IEnumerable<OtMapTile> mapTiles)
        {
            OtbmWriter mapWriter = new OtbmWriter(otbmFileName);
            mapWriter.WriteHeader(version);
            mapWriter.WriteMapStart(houseFileName, spawnFileName);
            foreach (OtMapTile tile in mapTiles)
            {
                mapWriter.WriteNodeStart(OtMapNodeTypes.TILE_AREA);
                mapWriter.WriteTileAreaCoords(tile.Location);
                mapWriter.WriteNodeStart(OtMapNodeTypes.TILE);
                mapWriter.WriteTileCoords(tile.Location);

                if (tile.TileId > 0)
                {
                    mapWriter.WriteByte((byte)OtMapAttrTypes.ITEM);
                    mapWriter.WriteUInt16(tile.TileId);
                }

                foreach (OtMapItem item in tile.Items)
                {
                    mapWriter.WriteNodeStart(OtMapNodeTypes.ITEM);
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
