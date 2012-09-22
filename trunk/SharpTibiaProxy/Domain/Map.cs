using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTibiaProxy.Domain
{
    public class TileEventArgs : EventArgs
    {
        public Tile Tile { get; private set; }
        public TileEventArgs(Tile tile)
        {
            this.Tile = tile;
        }
    }
    public class TileAddedEventArgs : TileEventArgs
    {
        public TileAddedEventArgs(Tile tile) : base(tile) { }
    }
    public class TileUpdatedEventArgs : TileEventArgs
    {
        public TileUpdatedEventArgs(Tile tile) : base(tile) { }
    }
    public class MapUpdatedEventArgs : EventArgs
    {
        public List<Tile> Tiles { get; private set; }
        public MapUpdatedEventArgs(List<Tile> tiles)
        {
            Tiles = tiles;
        }
    }

    public class Map
    {
        //private const int TILE_CACHE = 4096;

        private Client client;
        private Dictionary<Location, Tile> tiles;
        //private Tile[, ,] tiles;

        public event EventHandler<TileAddedEventArgs> TileAdded;
        public event EventHandler<TileUpdatedEventArgs> TileUpdated;
        public event EventHandler<MapUpdatedEventArgs> Updated;

        public Map(Client client)
        {
            this.client = client;
            tiles = new Dictionary<Location, Tile>();
            //this.tiles = new Tile[18, 14, 8];
        }

        //public Tile GetTile(int x, int y, int z)
        //{
        //    return this.tiles[x % 18, y % 14, z % 8];
        //}

        //public Tile GetTile(Location location)
        //{
        //    return this.GetTile(location.X, location.Y, location.Z);
        //}

        //internal void SetTile(Tile tile)
        //{
        //    var location = tile.Location;
        //    this.tiles[location.X % 18, location.Y % 14, location.Z % 8] = tile; 
        //}

        //internal void Clear()
        //{
        //    for (int x = 0; x < 18; x++)
        //    {
        //        for (int y = 0; y < 14; y++)
        //        {
        //            for (int z = 0; z < 8; z++)
        //            {
        //                tiles[x, y, z] = null;
        //            }
        //        }
        //    }
        //}

        public void Clear()
        {
            tiles.Clear();
        }

        public void SetTile(Tile tile)
        {
            if (tiles.ContainsKey(tile.Location))
            {
                tiles[tile.Location] = tile;
                OnTileUpdated(tile);
            }
            else
            {
                //if (tiles.Count > TILE_CACHE)
                //{
                //    var freeTiles = new List<Location>();

                //    foreach (var t in tiles.Values)
                //    {
                //        if (!PlayerCanSee(t.Location))
                //            freeTiles.Add(t.Location);
                //        if (freeTiles.Count >= 384)
                //            break;
                //    }

                //    if (freeTiles.Count == 0)
                //        throw new Exception("Unable to free tiles.");
                //    else
                //    {
                //        foreach (var t in freeTiles)
                //            tiles.Remove(t);
                //    }
                //}

                tiles[tile.Location] = tile;
                OnTileAdded(tile);
            }
        }

        public Tile GetTile(Location location)
        {
            if (tiles.ContainsKey(location))
                return tiles[location];

            return null;
        }

        //private bool PlayerCanSee(Location location)
        //{
        //    var playerLocation = client.PlayerLocation;
        //    if (playerLocation.Z <= 7)
        //    {
        //        //we are on ground level or above (7 -> 0)
        //        //view is from 7 -> 0
        //        if (location.Z > 7)
        //            return false;
        //    }
        //    else if (playerLocation.Z >= 8)
        //    {
        //        //we are underground (8 -> 15)
        //        //view is +/- 2 from the floor we stand on
        //        if (Math.Abs((int)playerLocation.Z - location.Z) > 2)
        //            return false;
        //    }

        //    //negative offset means that the action taken place is on a lower floor than ourself
        //    int offsetz = playerLocation.Z - location.Z;

        //    if ((location.X >= (int)playerLocation.X - 9 + offsetz) && (location.X <= (int)playerLocation.X + 10 + offsetz) &&
        //        (location.Y >= (int)playerLocation.Y - 7 + offsetz) && (location.Y <= (int)playerLocation.Y + 8 + offsetz))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        protected void OnTileAdded(Tile tile)
        {
            TileAdded.Raise(this, new TileAddedEventArgs(tile));
        }

        protected void OnTileUpdated(Tile tile)
        {
            TileUpdated.Raise(this, new TileUpdatedEventArgs(tile));
        }

        internal void OnMapUpdated(List<Tile> tiles)
        {
            Updated.Raise(this, new MapUpdatedEventArgs(tiles));
        }
    }
}
