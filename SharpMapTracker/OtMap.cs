using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;

namespace SharpMapTracker
{
    public class OtMap
    {
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
        private int npcCount;
        private int monsterCount;

        public OtMap()
        {
            tiles = new Dictionary<ulong, OtMapTile>();
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
            var index = tile.Location.ToIndex();

            if (tiles.ContainsKey(index))
            {
                var oldTile = tiles[index];
                if (oldTile.Creature != null)
                {
                    if (oldTile.Creature.Type == CreatureType.NPC)
                        npcCount--;
                    else if (oldTile.Creature.Type == CreatureType.MONSTER)
                        monsterCount--;
                }
            }

            tiles[tile.Location.ToIndex()] = tile;

            if (tile.Creature != null)
            {
                if (tile.Creature.Type == CreatureType.NPC)
                    npcCount++;
                else if (tile.Creature.Type == CreatureType.MONSTER)
                    monsterCount++;
            }
        }

        public IEnumerable<OtMapTile> Tiles { get { return tiles.Values; } }
        public IEnumerable<OtMapCreature> Creatures { get { return Tiles.Where(x => x.Creature != null).Select(x => x.Creature); } }

        public int TileCount { get { return tiles.Count; } }
        public int NpcCount { get { return npcCount; } }
        public int MonsterCount { get { return monsterCount; } }

        public void Clear()
        {
            lock (this)
            {
                tiles.Clear();
                npcCount = 0;
                monsterCount = 0;
            }
        }
    }
}
