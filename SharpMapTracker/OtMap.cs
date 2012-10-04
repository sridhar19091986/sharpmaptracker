using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;

namespace SharpMapTracker
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

        public enum OtMapAttrTypes
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

        public List<string> Descriptions { get; private set; }

        private Dictionary<ulong, OtMapTile> tiles;
        private Dictionary<uint, OtCreature> creatures;
        private Dictionary<ulong, OtSpawn> spawns;

        public OtMap()
        {
            tiles = new Dictionary<ulong, OtMapTile>();
            creatures = new Dictionary<uint, OtCreature>();
            spawns = new Dictionary<ulong, OtSpawn>();

            Descriptions = new List<string>();
        }

        public OtMapTile GetTile(Location location)
        {
            var index = location.ToIndex();
            if (tiles.ContainsKey(index))
                return tiles[index];

            return null;
        }

        public void SetTile(OtMapTile tile)
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

        public IEnumerable<OtMapTile> Tiles { get { return tiles.Values; } }
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
    }
}
