using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;
using System.Text.RegularExpressions;

namespace SharpMapTracker
{
    public class NpcVoice
    {
        public string Text { get; set; }
        public bool IsYell { get; set; }
        public int Interval { get; set; }
        public DateTime LastTime { get; set; }

        public NpcVoice()
        {
            Interval = 120;
            LastTime = DateTime.Now;
        }

        public override bool Equals(object obj)
        {
            var other = obj as NpcVoice;
            return other != null && other.Text == Text;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }

    public class NpcInfo
    {
        private string name;
        private Outfit outfit;
        private Shop shop;
        private Dictionary<string, string> statements;
        private HashSet<NpcVoice> voices;
        private HashSet<string> triedWords;

        public NpcInfo(Creature creature)
        {
            this.name = creature.Name;
            this.outfit = creature.Outfit;

            this.statements = new Dictionary<string, string>();
            this.triedWords = new HashSet<string>();
            this.voices = new HashSet<NpcVoice>();

            DefaultNPCWords.Words.Add(creature.Name.ToLower());
        }

        public void AddVoice(string value, bool yell)
        {
            var voice = voices.FirstOrDefault(x => x.Text == value);
            if (voice != null)
            {
                voice.Interval = (int)(DateTime.Now - voice.LastTime).TotalSeconds;
                voice.LastTime = DateTime.Now;
            }
            else
                voices.Add(new NpcVoice { Text = value, IsYell = yell });
        }

        public void AddStatement(string key, string value)
        {
            key = key.ToLower().Trim();
            DefaultNPCWords.AddWord(key);
            if (!statements.ContainsKey(key))
            {
                if (key.EndsWith("s"))
                {
                    var otherKey = key.Substring(0, key.Length - 1);
                    if (statements.ContainsKey(otherKey) && statements[otherKey] == value)
                        return;
                }
                else
                {
                    var otherKey = key + "s";
                    if (statements.ContainsKey(otherKey) && statements[otherKey] == value)
                    {
                        statements.Remove(otherKey);
                        statements[key] = value;
                        return;
                    }
                }

                statements[key] = value;

                var matches = Regex.Matches(value, @"{([\w'\s]+)}");

                foreach (Match match in matches)
                    DefaultNPCWords.AddWord(match.Groups[1].Value);
            }
        }

        public string Name { get { return name; } }
        public Outfit Outfit { get { return outfit; } }
        public Shop Shop { get { return shop; } set { this.shop = value; } }
        public Dictionary<string, string> Statements { get { return statements; } }
        public HashSet<string> TriedWords { get { return triedWords; } }
        public List<string> NotTriedWords { get { return DefaultNPCWords.Words.Where(x => !triedWords.Contains(x)).ToList(); } }
        public HashSet<NpcVoice> Voices { get { return voices; } }
    }
}
