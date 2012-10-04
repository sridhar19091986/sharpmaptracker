using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;

namespace SharpMapTracker
{
    public class OtSpawn
    {
        public Location Location { get; private set; }
        public int Radius { get; private set; }

        private readonly OtCreature[,] creatures;
        private readonly int size;

        public OtSpawn(Location location, int radius)
        {
            this.Location = location;
            this.Radius = radius;
            this.size = (radius * 2) + 1;
            creatures = new OtCreature[size, size];
        }

        public void AddCreature(OtCreature creature)
        {
            if (!IsInside(creature.Location))
                throw new Exception("Can't add this creature to this spawn. Spawn Location: " + Location + ", Creature Location: " + creature.Location);

            var relativeLocation = GetRelativeLocation(creature.Location);
            creature.Location = relativeLocation;
            creatures[relativeLocation.X + Radius, relativeLocation.Y + Radius] = creature;
        }

        public Location GetGlobalLocation(Location loc)
        {
            return new Location(Location.X + loc.X, Location.Y + loc.Y, Location.Z);
        }

        public Location GetRelativeLocation(Location loc)
        {
            return new Location(loc.X - Location.X, loc.Y - Location.Y, loc.Z);
        }

        public bool IsFree(Location loc)
        {
            if (!IsInside(loc))
                throw new Exception("Invalid location");

            var relativeLocation = GetRelativeLocation(loc);
            return creatures[relativeLocation.X + Radius, relativeLocation.Y + Radius] == null;
        }

        public bool IsInside(Location loc)
        {
            if (loc == null || loc.Z != Location.Z)
                return false;

            var relativeLocation = GetRelativeLocation(loc);
            return Math.Abs(relativeLocation.X) <= Radius && Math.Abs(relativeLocation.Y) <= Radius;
        }

        public IEnumerable<OtCreature> GetCreatures()
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (creatures[x, y] != null)
                        yield return creatures[x, y];
                }
            }
        }

    }
}
