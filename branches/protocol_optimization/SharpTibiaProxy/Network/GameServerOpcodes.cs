using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTibiaProxy.Network
{
    public enum GameServerOpcodes : byte
    {
        InitGame = 10,
        GMActions = 11,
        LoginError = 20,
        LoginAdvice = 21,
        LoginWait = 22,
        PingBack = 29,
        Ping = 30,
        Challange = 31,
        Death = 40,

        // all in game opcodes must be greater than 50
        FullMap = 100,
        MapTopRow = 101,
        MapRightRow = 102,
        MapBottomRow = 103,
        MapLeftRow = 104,
        UpdateTile = 105,
        CreateOnMap = 106,
        ChangeOnMap = 107,
        DeleteOnMap = 108,
        MoveCreature = 109,
        OpenContainer = 110,
        CloseContainer = 111,
        CreateContainer = 112,
        ChangeInContainer = 113,
        DeleteInContainer = 114,
        SetInventory = 120,
        DeleteInventory = 121,
        OpenNpcTrade = 122,
        PlayerGoods = 123,
        CloseNpcTrade = 124,
        OwnTrade = 125,
        CounterTrade = 126,
        CloseTrade = 127,
        Ambient = 130,
        GraphicalEffect = 131,
        TextEffect = 132,
        MissleEffect = 133,
        MarkCreature = 134,
        Trappers = 135,
        CreatureHealth = 140,
        CreatureLight = 141,
        CreatureOutfit = 142,
        CreatureSpeed = 143,
        CreatureSkull = 144,
        CreatureParty = 145,
        CreatureUnpass = 146,
        EditText = 150,
        EditList = 151,
        PlayerDataBasic = 159, // 950
        PlayerData = 160,
        PlayerSkills = 161,
        PlayerState = 162,
        ClearTarget = 163,
        SpellDelay = 164, // 870
        SpellGroupDelay = 165, // 870
        MultiUseDelay = 166, // 870
        Talk = 170,
        Channels = 171,
        OpenChannel = 172,
        OpenPrivateChannel = 173,
        RuleViolationChannel = 174,
        RuleViolationRemove = 175,
        RuleViolationCancel = 176,
        RuleViolationLock = 177,
        OpenOwnChannel = 178,
        CloseChannel = 179,
        TextMessage = 180,
        CancelWalk = 181,
        WalkWait = 182,
        FloorChangeUp = 190,
        FloorChangeDown = 191,
        ChooseOutfit = 200,
        VipAdd = 210,
        VipLogin = 211,
        VipLogout = 212,
        TutorialHint = 220,
        AutomapFlag = 221,
        QuestLog = 240,
        QuestLine = 241,
        ChannelEvent = 243, // 910
        ItemInfo = 244, // 910
        PlayerInventory = 245, // 910
        MarketEnter = 246, // 944
        MarketLeave = 247, // 944
        MarketDetail = 248, // 944
        MarketBrowse = 249, // 944
        ShowModalDialog = 250  // 960

    }
}
