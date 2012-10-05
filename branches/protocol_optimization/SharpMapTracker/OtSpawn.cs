using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;

namespace SharpMapTracker
{
    public class OtSpawn
    {
        public Position Location { get; private set; }
        public int Radius { get; private set; }

        private readonly OtCreature[,] creatures;
        private readonly int size;

        public OtSpawn(Position location, int radius)
        {
            this.Location = location;
            this.Radius = radius;
            this.size = (radius * 2) + 1;
            creatures = new OtCreature[size, size];
        }

        public void AddCreature(OtCreature creature)
        {
            var relativeLocation = GetRelativeLocation(creature.Location);

            if (!(Math.Abs(relativeLocation.X) <= Radius && Math.Abs(relativeLocation.Y) <= Radius))
                throw new Exception("Can't add this creature to this spawn. Spawn Location: " + Location + ", Creature Location: " + creature.Location);

            creature.Location = relativeLocation;
            creatures[relativeLocation.X + Radius, relativeLocation.Y + Radius] = creature;
        }

        private Position GetRelativeLocation(Position loc)
        {
            return new Position(loc.X - Location.X, loc.Y - Location.Y, loc.Z);
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
