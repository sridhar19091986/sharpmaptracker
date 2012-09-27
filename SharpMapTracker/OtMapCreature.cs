using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;

namespace SharpMapTracker
{
    public class OtMapCreature
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Location Location{ get; set; }
        public CreatureType Type { get; set; }
    }
}
