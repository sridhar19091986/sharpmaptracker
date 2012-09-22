using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SharpTibiaProxy.Domain
{
    public class Tile
    {
        public Location Location { get; private set; }
        public int ThingCount { get { return things.Count; } }

        private List<Thing> things;

        public Tile(Location location)
        {
            Location = location;
            things = new List<Thing>();
        }

        public void AddThing(Thing thing)
        {
            AddThing(0xFF, thing);
        }

        public void AddThing(int index, Thing thing)
        {
            if (thing == null)
                throw new Exception("[AddThing] Null thing.");

            if (index != 0xFF)
            {
                if (index == 0)
                {
                    var item = thing as Item;
                    if (item == null || !item.IsGround)
                        throw new Exception("[AddThing] Invalid ground item.");
                }

                things.Insert(index, thing);
            }
            else
            {
                if (thing is Creature)
                {
                    int pos = things.Count;
                    for (int i = 0; i < things.Count; i++)
                    {
                        if (things[i].Order > thing.Order)
                        {
                            pos = i;
                            break;
                        }
                    }

                    things.Insert(pos, thing);
                    ((Creature)thing).Location = Location;
                }
                else
                {
                    int pos = things.Count;
                    for (int i = 0; i < things.Count; i++)
                    {
                        if (things[i].Order >= thing.Order)
                        {
                            pos = i;
                            break;
                        }
                    }

                    things.Insert(pos, thing);
                }
            }

            if (things.Count > 10)
                RemoveThing(10);

        }

        public void RemoveThing(int index)
        {
            if (index < 0 || index >= ThingCount)
                throw new Exception("[RemoveThing] Invalid stack value: " + index);

            things.RemoveAt(index);
        }

        public Thing GetThing(int index)
        {
            if (index < 0 || index >= ThingCount)
                throw new Exception("[GetThing] Invalid stack value: " + index);

            return things[index];
        }

        public void ReplaceThing(int index, Thing thing)
        {
            if (index < 0 || index >= ThingCount)
                throw new Exception("[ReplaceThing] Invalid stack value: " + index);

            if (index == 0)
            {
                var item = thing as Item;
                if (item == null || !item.IsGround)
                    throw new Exception("[ReplaceThing] Invalid ground item.");
            }

            things[index] = thing;

            if(thing is Creature)
                ((Creature)thing).Location = Location;
        }

        public void Clear()
        {
            things.Clear();
        }
    }
}
