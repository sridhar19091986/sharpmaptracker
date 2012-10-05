using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTibiaProxy.Domain
{
    public class Position
    {
        public static readonly Position Invalid = new Position(ushort.MaxValue, ushort.MaxValue, byte.MaxValue);

        private ushort X, Y;
        private byte Z;

        public Position(ushort x, ushort y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public bool IsMapPosition { get { return (X < ushort.MaxValue && Y < ushort.MaxValue && Z <= Constants.MAX_Z); } }
        public bool IsValid { get { return !(X == ushort.MaxValue && Y == ushort.MaxValue && Z == byte.MaxValue); } }
        public double GetDistance(Position pos) { return Math.Sqrt(Math.Pow((pos.X - X), 2) + Math.Pow((pos.Y - Y), 2)); }
        public int GetManhattanDistance(Position pos) { return Math.Abs(pos.X - X) + Math.Abs(pos.Y - Y); }

        public Position operator +(Position other) { return new Position((ushort)(X + other.X), (ushort)(Y + other.Y), (byte)(Z + other.Z)); }
        //Position operator+=(Position other) { x+=other.x; y+=other.y; z +=other.z; return *this; }
        public Position operator -(Position other) { return new Position((ushort)(X - other.X), (ushort)(Y - other.Y), (byte)(Z - other.Z)); }
        //Position operator-=(const Position& other) { x-=other.x; y-=other.y; z-=other.z; return *this; }

        public Position operator=(Position other) { X = other.X; Y = other.Y; Z = other.Z; return this; }

        public bool IsCreature
        {
            get
            {
                return this.X == 65535;
            }
        }

        public uint GetCretureId(int stack)
        {
            if (this.IsCreature)
                return checked((uint)(this.Y | this.Z << 16 | stack << 24));

            throw new InvalidOperationException("Attempted to get the creature id of a location that is not a creature reference.");
        }

        public ulong ToIndex()
        {
            return (ulong)((uint)X & 0xFFFF) << 24 | ((uint)Y & 0xFFFF) << 8 | ((uint)Z & 0xFF);
        }

        public static ulong ToIndex(int x, int y, int z)
        {
            return (ulong)((uint)x & 0xFFFF) << 24 | ((uint)y & 0xFFFF) << 8 | ((uint)z & 0xFF);
        }

        public static Position FromIndex(ulong index)
        {
            return new Position((int)((index >> 24) & 0xFFFF), (int)((index >> 8) & 0xFFFF), (int)(index & 0xFF));
        }

        public override bool Equals(object obj)
        {
            var other = obj as Position;
            return other != null && other.X == X && other.Y == Y && other.Z == Z;
        }

        public override int GetHashCode()
        {
            return (X + Y + Z) ^ 31;
        }

        public override string ToString()
        {
            return "[" + X + ", " + Y + ", " + Z + "]";
        }

    }
}
