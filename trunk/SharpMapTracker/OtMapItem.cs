﻿using SharpTibiaProxy.Domain;
namespace SharpMapTracker
{
    public enum OtMapItemAttrTypes
    {
        NONE = 0,
        //DESCRIPTION = 1,
        //EXT_FILE = 2,
        TILE_FLAGS = 3,
        ACTION_ID = 4,
        UNIQUE_ID = 5,
        TEXT = 6,
        DESC = 7,
        TELE_DEST = 8,
        ITEM = 9,
        DEPOT_ID = 10,
        //EXT_SPAWN_FILE = 11,
        RUNE_CHARGES = 12,
        //EXT_HOUSE_FILE = 13,
        HOUSEDOORID = 14,
        COUNT = 15,
        DURATION = 16,
        DECAYING_STATE = 17,
        WRITTENDATE = 18,
        WRITTENBY = 19,
        SLEEPERGUID = 20,
        SLEEPSTART = 21,
        CHARGES = 22,
        CONTAINER_ITEMS = 23,
        NAME = 30,
        PLURALNAME = 31,
        ATTACK = 33,
        EXTRAATTACK = 34,
        DEFENSE = 35,
        EXTRADEFENSE = 36,
        ARMOR = 37,
        ATTACKSPEED = 38,
        HITCHANCE = 39,
        SHOOTRANGE = 40,
        ARTICLE = 41,
        SCRIPTPROTECTED = 42,
        DUALWIELD = 43,
        ATTRIBUTE_MAP = 128
    };

    public class OtMapItem
    {
        public OtItemType Info { get; private set; }
        public OtMapItemAttrTypes AttrType;
        public byte Extra;

        public OtMapItem(OtItemType info)
        {
            Info = info;
        }

    }
}
