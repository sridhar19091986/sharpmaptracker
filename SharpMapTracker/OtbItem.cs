using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMapTracker
{
    public enum OtbItemType
    {
        None,
        Ground,
        Container,
        FluidContainer,
        Splash,
        Deprecated
    };

    public class OtItemType
    {
        public UInt16 Id;
        public UInt16 SpriteId;
        public UInt16 GroundSpeed;
        public OtbItemType Type;
        public bool AlwaysOnTop;
        public UInt16 AlwaysOnTopOrder;
        public bool HasUseWith;
        public UInt16 MaxReadChars;
        public UInt16 MaxReadWriteChars;
        public bool HasHeight;
        public UInt16 MinimapColor;
        public bool LookThrough;
        public UInt16 LightLevel;
        public UInt16 LightColor;
        public bool IsStackable;
        public bool IsReadable;
        public bool IsMoveable;
        public bool IsPickupable;
        public bool IsHangable;
        public bool IsHorizontal;
        public bool IsVertical;
        public bool IsRotatable;
        public bool BlockObject;
        public bool BlockProjectile;
        public bool BlockPathFind;
        public bool AllowDistRead;
        public bool IsAnimation;
        public bool WalkStack;
        public UInt16 WareId;
        public string Name;
        public byte[] SpriteHash;
    }
}
