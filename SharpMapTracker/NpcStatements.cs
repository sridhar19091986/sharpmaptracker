using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;

namespace SharpMapTracker
{
    public class NpcStatements
    {
        private Creature creature;
        private Dictionary<string, string> statements;

        public NpcStatements(Creature creature)
        {
            this.creature = creature;
            this.statements = new Dictionary<string, string>();
        }

        public void AddStatement(string key, string value)
        {
            key = key.ToLower().Trim();
            if (!statements.ContainsKey(key))
                statements[key.ToLower().Trim()] = value;
        }

        public Creature Creature { get { return creature; } }
        public Dictionary<string, string> Statements { get { return statements; } }
    }
}
