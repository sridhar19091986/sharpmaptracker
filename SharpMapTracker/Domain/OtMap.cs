using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;
using System.IO;
using SharpMapTracker.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;

namespace SharpMapTracker.Domain
{
    public class OtMap
    {
        private const int SPAWN_RADIUS = 3;
        private const int SPAWN_SIZE = SPAWN_RADIUS * 2 + 1;

        #region Enums

        public enum OtMapNodeTypes
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

        public enum OtMapAttribute
        {
            NONE = 0,
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

        #endregion

        public uint Version { get; set; }

        public ushort Width { get; set; }
        public ushort Height { get; set; }

        public uint MajorVersionItems { get; set; }
        public uint MinorVersionItems { get; set; }

        public string HouseFile { get; set; }
        public string SpawnFile { get; set; }

        public OtItems Items { get; private set; }
        
        public List<string> Descriptions { get; private set; }

        private Dictionary<uint, OtTown> towns;
        private Dictionary<ulong, OtTile> tiles;
        private Dictionary<uint, OtCreature> creatures;
        private Dictionary<ulong, OtSpawn> spawns;

        public OtMap(OtItems items)
        {
            tiles = new Dictionary<ulong, OtTile>();
            creatures = new Dictionary<uint, OtCreature>();
            spawns = new Dictionary<ulong, OtSpawn>();
            towns = new Dictionary<uint,OtTown>();

            Version = 3;
            Width = 0xFCFC;
            Height = 0xFCFC;

            Items = items;

            Descriptions = new List<string>();
        }

        public OtTile GetTile(Location location)
        {
            return GetTile(location.ToIndex());
        }

        public OtTile GetTile(ulong index)
        {
            if (tiles.ContainsKey(index))
                return tiles[index];

            return null;
        }

        public void SetTile(OtTile tile)
        {
            tiles[tile.Location.ToIndex()] = tile;
        }

        public void AddCreature(OtCreature creature)
        {
            if (!creatures.ContainsKey(creature.Id))
            {
                var spawnLocation = new Location(creature.Location.X - (creature.Location.X % SPAWN_SIZE) + SPAWN_RADIUS,
                    creature.Location.Y - (creature.Location.Y % SPAWN_SIZE) + SPAWN_RADIUS, creature.Location.Z);
                var spawnIndex = spawnLocation.ToIndex();

                if (!spawns.ContainsKey(spawnIndex))
                    spawns.Add(spawnIndex, new OtSpawn(spawnLocation, SPAWN_RADIUS));

                var spwan = spawns[spawnIndex];
                spwan.AddCreature(creature);
                creatures.Add(creature.Id, creature);
            }
        }

        public IEnumerable<OtTile> Tiles { get { return tiles.Values; } }
        public IEnumerable<OtSpawn> Spawns { get { return spawns.Values; } }

        public int TileCount { get { return tiles.Count; } }
        public int NpcCount { get { return creatures.Count(x => x.Value.Type == CreatureType.NPC); } }
        public int MonsterCount { get { return creatures.Count(x => x.Value.Type == CreatureType.MONSTER); } }

        public void Clear()
        {
            lock (this)
            {
                tiles.Clear();
                spawns.Clear();
                creatures.Clear();
            }
        }

        #region Save

        public void Save(string fileName)
        {
            var dir = Path.GetDirectoryName(fileName);
            var baseFileName = Path.GetFileNameWithoutExtension(fileName);
            string otbmFileName = baseFileName + ".otbm";
            HouseFile = baseFileName + "-house.xml";
            SpawnFile = baseFileName + "-spawn.xml";

            SaveOtbm(Path.Combine(dir, otbmFileName));
            SaveHouses(Path.Combine(dir, HouseFile));
            SaveSpawns(Path.Combine(dir, SpawnFile));
        }

        private void SaveOtbm(string fileName)
        {
            using (var writer = new OtFileWriter(fileName))
            {
                //Header
                writer.Write((uint)0, false);

                writer.WriteNodeStart((byte)OtMapNodeTypes.ROOTV1);

                writer.Write(Version);
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Items.MajorVersion);
                writer.Write(Items.MinorVersion);

                //Map Data
                writer.WriteNodeStart((byte)OtMapNodeTypes.MAP_DATA);
                
                writer.Write((byte)OtMapAttribute.DESCRIPTION);
                writer.Write("Created with SharpMapTracker v" + Constants.MAP_TRACKER_VERSION);

                writer.Write((byte)OtMapAttribute.EXT_HOUSE_FILE);
                writer.Write(HouseFile);

                writer.Write((byte)OtMapAttribute.EXT_SPAWN_FILE);
                writer.Write(SpawnFile);

                foreach (var tile in Tiles)
                {
                    writer.WriteNodeStart((byte)OtMapNodeTypes.TILE_AREA);
                    writer.Write((ushort)(tile.Location.X & 0xFF00));
                    writer.Write((ushort)(tile.Location.Y & 0xFF00));
                    writer.Write((byte)tile.Location.Z);

                    writer.WriteNodeStart((byte)OtMapNodeTypes.TILE);
                    writer.Write((byte)tile.Location.X);
                    writer.Write((byte)tile.Location.Y);

                    if (tile.Flags > 0)
                    {
                        writer.Write((byte)OtMapAttribute.TILE_FLAGS);
                        writer.Write(tile.Flags);
                    }

                    if (tile.TileId > 0)
                    {
                        writer.Write((byte)OtMapAttribute.ITEM);
                        writer.Write(tile.TileId);
                    }

                    foreach (var item in tile.Items)
                    {
                        writer.WriteNodeStart((byte)OtMapNodeTypes.ITEM);

                        writer.Write(item.Type.Id);
                        item.Serialize(writer.GetPropertyWriter());

                        writer.WriteNodeEnd(); //Item
                    }

                    writer.WriteNodeEnd(); //Tile
                    writer.WriteNodeEnd(); //Tile Area
                }

                //writer.WriteNodeStart((byte)OtMapNodeTypes.TOWNS);

                //foreach (var town in towns.Values)
                //{
                //    writer.WriteNodeStart((byte)OtMapNodeTypes.TOWN);
                //    writer.Write(town.Id);
                //    writer.Write(town.Name);
                //    writer.Write(town.TempleLocation);
                //    writer.WriteNodeEnd(); //Town
                //}

                //writer.WriteNodeEnd(); //Towns

                writer.WriteNodeEnd(); //Map Data
                writer.WriteNodeEnd(); //Root
            }
        }

        private void SaveSpawns(string spawnFileName)
        {
            XElement spawns = new XElement("spawns");

            foreach (var s in Spawns)
            {
                XElement spawn = new XElement("spawn");
                spawn.Add(new XAttribute("centerx", s.Location.X));
                spawn.Add(new XAttribute("centery", s.Location.Y));
                spawn.Add(new XAttribute("centerz", s.Location.Z));
                spawn.Add(new XAttribute("radius", s.Radius));

                foreach (var creature in s.GetCreatures())
                {
                    XElement creatureSpawn = new XElement(creature.Type == CreatureType.NPC ? "npc" : "monster");
                    creatureSpawn.Add(new XAttribute("name", creature.Name));
                    creatureSpawn.Add(new XAttribute("x", creature.Location.X));
                    creatureSpawn.Add(new XAttribute("y", creature.Location.Y));
                    creatureSpawn.Add(new XAttribute("z", creature.Location.Z));
                    creatureSpawn.Add(new XAttribute("spawntime", "60"));

                    spawn.Add(creatureSpawn);
                }

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

        private void SaveHouses(string housesFileName)
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

        #endregion

        #region Load

        public void Load(string fileName, bool replaceTiles)
        {
            if (!File.Exists(fileName))
                throw new Exception(string.Format("File not found {0}.", fileName));

            var loader = new OtFileReader();
            loader.Open(fileName);
            OtFileNode node = loader.GetRootNode();

            OtPropertyReader props;

            if (!loader.GetProps(node, out props))
                throw new Exception("Could not read root property.");

            props.ReadByte(); // junk?

            var version = props.ReadUInt32();
            props.ReadUInt16();
            props.ReadUInt16();

            var majorVersionItems = props.ReadUInt32();
            var minorVersionItems = props.ReadUInt32();

            if (version <= 0)
            {
                //In otbm version 1 the count variable after splashes/fluidcontainers and stackables
                //are saved as attributes instead, this solves alot of problems with items
                //that is changed (stackable/charges/fluidcontainer/splash) during an update.
                throw new Exception(
                    "This map needs to be upgraded by using the latest map editor version to be able to load correctly.");
            }

            if (version > 3)
            {
                throw new Exception("Unknown OTBM version detected.");
            }

            if (majorVersionItems < 3)
            {
                throw new Exception(
                    "This map needs to be upgraded by using the latest map editor version to be able to load correctly.");
            }

            if (majorVersionItems > Items.MajorVersion)
            {
                throw new Exception("The map was saved with a different items.otb version, an upgraded items.otb is required.");
            }

            /*if (MinorVersionItems < (uint)ClientVersion.ClientVersion810)
            {
                throw new Exception("This map needs to be updated.");
            }*/

            if (minorVersionItems > Items.MinorVersion)
                Trace.WriteLine("This map needs an updated items.otb.");
            //if (MinorVersionItems == (uint)ClientVersion.ClientVersion854Bad)
            //    Trace.WriteLine("This map needs uses an incorrect version of items.otb.");


            node = node.Child;

            if ((OtMapNodeTypes)node.Type != OtMapNodeTypes.MAP_DATA)
            {
                throw new Exception("Could not read data node.");
            }

            if (!loader.GetProps(node, out props))
            {
                throw new Exception("Could not read map data attributes.");
            }

            while (props.PeekChar() != -1)
            {
                byte attribute = props.ReadByte();
                switch ((OtMapAttribute)attribute)
                {
                    case OtMapAttribute.DESCRIPTION:
                        var description = props.GetString();
                        Descriptions.Add(description);
                        break;
                    case OtMapAttribute.EXT_SPAWN_FILE:
                        SpawnFile = props.GetString();
                        break;
                    case OtMapAttribute.EXT_HOUSE_FILE:
                        HouseFile = props.GetString();
                        break;
                    default:
                        throw new Exception("Unknown header node.");
                }
            }

            OtFileNode nodeMapData = node.Child;

            while (nodeMapData != null)
            {
                switch ((OtMapNodeTypes)nodeMapData.Type)
                {
                    case OtMapNodeTypes.TILE_AREA:
                        ParseTileArea(loader, nodeMapData, replaceTiles);
                        break;
                    case OtMapNodeTypes.TOWNS:
                        ParseTowns(loader, nodeMapData);
                        break;
                }
                nodeMapData = nodeMapData.Next;
            }
        }

        private void ParseTileArea(OtFileReader loader, OtFileNode otbNode, bool replaceTiles)
        {
            OtPropertyReader props;
            if (!loader.GetProps(otbNode, out props))
            {
                throw new Exception("Invalid map node.");
            }

            int baseX = props.ReadUInt16();
            int baseY = props.ReadUInt16();
            int baseZ = props.ReadByte();

            OtFileNode nodeTile = otbNode.Child;

            while (nodeTile != null)
            {
                if (nodeTile.Type == (long)OtMapNodeTypes.TILE ||
                    nodeTile.Type == (long)OtMapNodeTypes.HOUSETILE)
                {
                    loader.GetProps(nodeTile, out props);

                    var tileLocation = new Location(baseX + props.ReadByte(), baseY + props.ReadByte(), baseZ);

                    var tile = GetTile(tileLocation);
                    if (tile == null)
                        tile = new OtTile(tileLocation);

                    if (nodeTile.Type == (long)OtMapNodeTypes.HOUSETILE)
                    {
                        tile.HouseId = props.ReadUInt32();
                    }

                    while (props.PeekChar() != -1)
                    {
                        byte attribute = props.ReadByte();
                        switch ((OtMapAttribute)attribute)
                        {
                            case OtMapAttribute.TILE_FLAGS:
                                {
                                    tile.Flags = props.ReadUInt32();
                                    break;
                                }
                            case OtMapAttribute.ITEM:
                                {
                                    ushort itemId = props.ReadUInt16();

                                    var itemType = Items.GetItem(itemId);
                                    if (itemType == null)
                                    {
                                        throw new Exception("Unkonw item type " + itemId + " in position " + tileLocation + ".");
                                    }

                                    var item = new OtItem(itemType);
                                    tile.AddItem(item);

                                    break;
                                }
                            default:
                                throw new Exception(string.Format("{0} Unknown tile attribute.", tileLocation));
                        }
                    }   

                    OtFileNode nodeItem = nodeTile.Child;

                    while (nodeItem != null)
                    {
                        if (nodeItem.Type == (long)OtMapNodeTypes.ITEM)
                        {
                            loader.GetProps(nodeItem, out props);

                            ushort itemId = props.ReadUInt16();

                            var itemType = Items.GetItem(itemId);
                            if (itemType == null)
                            {
                                throw new Exception("Unkonw item type " + itemId + " in position " + tileLocation + ".");
                            }

                            var item = new OtItem(itemType);
                            
                            item.Deserialize(props);

                            tile.AddItem(item);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0} Unknown node type.", tileLocation));
                        }
                        nodeItem = nodeItem.Next;
                    }

                    if(GetTile(tileLocation) == null || replaceTiles)
                        SetTile(tile);
                }

                nodeTile = nodeTile.Next;
            }
        }

        private void ParseTowns(OtFileReader loader, OtFileNode otbNode)
        {
            OtFileNode nodeTown = otbNode.Child;

            while (nodeTown != null)
            {
                OtPropertyReader props;
                if (!loader.GetProps(nodeTown, out props))
                {
                    throw new Exception("Could not read town data.");
                }

                uint townid = props.ReadUInt32();
                string townName = props.GetString();
                var templeLocation = props.ReadLocation();

                var town = new OtTown { Id = townid, Name = townName, TempleLocation = templeLocation };
                towns[townid] = town;

                nodeTown = nodeTown.Next;
            }
        }

        #endregion

    }
}
