using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTibiaProxy.Domain
{
    public class BattleList
    {
        private const int CREATURES_ARRAY = 250;

        private Client client;
        private Dictionary<uint, Creature> creatures;

        public BattleList(Client client)
        {
            this.client = client;
            this.creatures = new Dictionary<uint, Creature>();
        }

        public Creature GetCreature(uint id)
        {
            if (creatures.ContainsKey(id))
                return creatures[id];

            return null;
        }

        public void AddCreature(Creature creature)
        {
            creatures[creature.Id] = creature;
        }

        public void RemoveCreature(uint id)
        {
            if (creatures.ContainsKey(id))
                creatures.Remove(id);
        }


        internal void Clear()
        {
            
        }

        public bool ContainsCreature(uint id)
        {
            return creatures.ContainsKey(id);
        }
    }
}
