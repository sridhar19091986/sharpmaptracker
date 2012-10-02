using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;
using System.Text.RegularExpressions;

namespace SharpMapTracker
{
    public class NpcInfo
    {
        private string name;
        private Outfit outfit;
        private Shop shop;
        private Dictionary<string, string> statements;
        private HashSet<string> words;
        private HashSet<string> triedWords;

        public NpcInfo(Creature creature)
        {
            this.name = creature.Name;
            this.outfit = creature.Outfit;

            this.statements = new Dictionary<string, string>();
            this.words = new HashSet<string>();
            this.triedWords = new HashSet<string>();

            //Default Words
            this.Words.Add("name");
            this.Words.Add("job");
            this.Words.Add("time");
            this.Words.Add("offer");
            this.Words.Add("trade");
            this.Words.Add(name.ToLower());
            this.Words.Add("tibia");
            this.Words.Add("king");
            this.Words.Add("thais");
            this.Words.Add("rookgaard");
            this.Words.Add("gods");
            this.Words.Add("quest");
            this.Words.Add("help");
            this.Words.Add("hints");
            this.Words.Add("citizens");
            this.Words.Add("merchants");
            this.Words.Add("monsters");
        }

        public void AddStatement(string key, string value)
        {
            key = key.ToLower().Trim();
            if (!statements.ContainsKey(key))
            {
                statements[key.ToLower().Trim()] = value;

                var matches = Regex.Matches(value, @"{([\w'\s]+)}");

                foreach (Match match in matches)
                    words.Add(match.Groups[1].Value.ToLower());
            }
        }

        public string Name { get { return name; } }
        public Outfit Outfit { get { return outfit; } }
        public Shop Shop { get { return shop; } set { this.shop = value; } }
        public Dictionary<string, string> Statements { get { return statements; } }
        public HashSet<string> Words { get { return words; } }
        public HashSet<string> TriedWords { get { return triedWords; } }
        public List<string> NotTriedWords { get { return words.Where(x => !triedWords.Contains(x)).ToList(); } }
    }
}
