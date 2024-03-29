﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpTibiaProxy.Domain;
using System.Diagnostics;

namespace SharpTibiaProxy.Network
{
    public class ProtocolWorld
    {
        private Client client;

        public ProtocolWorld(Client client)
        {
            this.client = client;
        }

        #region Client

        public void ParseClientMessage(InMessage message)
        {
            var packetStart = 0;
            try
            {
                packetStart = message.ReadPosition;
                byte cmd = message.ReadByte();

                switch (cmd)
                {
                    case 0x96:
                        ParseClientSay(message);
                        break;
                }

            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message + "\nPacket Bytes: " + message.Buffer.ToHexString(packetStart, message.ReadPosition - packetStart));
            }

        }

        private void ParseClientSay(InMessage message)
        {
            string receiver = null;
            ushort channelId = 0;

            MessageClasses type = (MessageClasses)message.ReadByte();
            switch (type)
            {
                case MessageClasses.PRIVATE_TO:
                case MessageClasses.GAMEMASTER_PRIVATE_TO:
                    receiver = message.ReadString();
                    break;

                case MessageClasses.CHANNEL:
                case MessageClasses.CHANNEL_HIGHLIGHT:
                case MessageClasses.GAMEMASTER_CHANNEL:
                    channelId = message.ReadUShort();
                    break;

                default:
                    break;
            }

            string text = message.ReadString();

            client.Chat.OnPlayerSpeak(receiver, channelId, type, text);
        }

        #endregion

        #region ParseServer

        public void ParseServerMessage(InMessage message)
        {
            var packets = new List<byte>();
            var packetStart = 0;
            try
            {
                while (message.ReadPosition < message.Size)
                {
                    packetStart = message.ReadPosition;
                    byte cmd = message.ReadByte();
                    packets.Add(cmd);

                    switch (cmd)
                    {
                        case 0x0A:
                            ParseServerSelfAppear(message);
                            break;
                        case 0x0B:
                            ParseServerGMActions(message);
                            break;
                        case 0x14:
                            ParseServerErrorMessage(message);
                            break;
                        case 0x15:
                            ParseServerFYIMessage(message);
                            break;
                        case 0x16:
                            ParseServerWaitingList(message);
                            break;
                        case 0x1D:
                            ParseServerPing(message);
                            break;
                        case 0x1E:
                            ParseServerPingBack(message);
                            break;
                        case 0x28:
                            ParseServerDeath(message);
                            break;
                        case 0x32:
                            ParseServerCanReportBugs(message);
                            break;
                        case 0x64:
                            ParseServerMapDescription(message);
                            break;
                        case 0x65:
                            ParseServerMoveNorth(message);
                            break;
                        case 0x66:
                            ParseServerMoveEast(message);
                            break;
                        case 0x67:
                            ParseServerMoveSouth(message);
                            break;
                        case 0x68:
                            ParseServerMoveWest(message);
                            break;
                        case 0x69:
                            ParseServerUpdateTile(message);
                            break;
                        case 0x6A:
                            ParseServerTileAddThing(message);
                            break;
                        case 0x6B:
                            ParseServerTileTransformThing(message);
                            break;
                        case 0x6C:
                            ParseServerTileRemoveThing(message);
                            break;
                        case 0x6D:
                            ParseServerCreatureMove(message);
                            break;
                        case 0x6E:
                            ParseServerOpenContainer(message);
                            break;
                        case 0x6F:
                            ParseServerCloseContainer(message);
                            break;
                        case 0x70:
                            ParseServerContainerAddItem(message);
                            break;
                        case 0x71:
                            ParseServerContainerUpdateItem(message);
                            break;
                        case 0x72:
                            ParseServerContainerRemoveItem(message);
                            break;
                        case 0x78:
                            ParseServerInventorySetSlot(message);
                            break;
                        case 0x79:
                            ParseServerInventoryResetSlot(message);
                            break;
                        case 0x7D:
                            ParseServerSafeTradeRequestAck(message);
                            break;
                        case 0x7E:
                            ParseServerSafeTradeRequestNoAck(message);
                            break;
                        case 0x7F:
                            ParseServerSafeTradeClose(message);
                            break;
                        case 0x82:
                            ParseServerWorldLight(message);
                            break;
                        case 0x83:
                            ParseServerMagicEffect(message);
                            break;
                        case 0x84:
                            ParseServerAnimatedText(message);
                            break;
                        case 0x85:
                            ParseServerDistanceShot(message);
                            break;
                        case 0x86:
                            ParseServerCreatureSquare(message);
                            break;
                        case 0x87:
                            byte b = message.ReadByte();
                            if (b > 0)
                                message.ReadBytes(b * 4);
                            break;
                        case 0x8C:
                            ParseServerCreatureHealth(message);
                            break;
                        case 0x8D:
                            ParseServerCreatureLight(message);
                            break;
                        case 0x8E:
                            ParseServerCreatureOutfit(message);
                            break;
                        case 0x8F:
                            ParseServerCreatureSpeed(message);
                            break;
                        case 0x90:
                            ParseServerCreatureSkulls(message);
                            break;
                        case 0x91:
                            ParseServerCreatureShields(message);
                            break;
                        case 0x92:
                            ParseServerCreaturePassable(message);
                            break;
                        case 0x96:
                            ParseServerItemTextWindow(message);
                            break;
                        case 0x97:
                            ParseServerHouseTextWindow(message);
                            break;
                        case 0xA0:
                            ParseServerPlayerStats(message);
                            break;
                        case 0xA1:
                            ParseServerPlayerSkills(message);
                            break;
                        case 0xA2:
                            ParseServerPlayerIcons(message);
                            break;
                        case 0xA3:
                            ParseServerPlayerCancelAttack(message);
                            break;
                        case 0xA4:
                            ParseServerSpellCooldown(message);
                            break;
                        case 0xA5:
                            ParseServerSpellGroupCooldown(message);
                            break;
                        case 0xA6: //desconhecido
                            message.ReadUInt();
                            break;
                        case 0xAA:
                            ParseServerCreatureSpeak(message);
                            break;
                        case 0xAB:
                            ParseServerChannelList(message);
                            break;
                        case 0xAC:
                            ParseServerOpenChannel(message);
                            break;
                        case 0xAD:
                            ParseServerOpenPrivatePlayerChat(message);
                            break;
                        case 0xAE:
                            ParseServerOpenRuleViolation(message);
                            break;
                        case 0xB2:
                            ParseServerCreatePrivateChannel(message);
                            break;
                        case 0xB3:
                            ParseServerClosePrivateChannel(message);
                            break;
                        case 0xB4:
                            ParseServerTextMessage(message);
                            break;
                        case 0xB5:
                            ParseServerPlayerCancelWalk(message);
                            break;
                        case 0xB6:
                            message.ReadUShort();
                            break;
                        case 0xBE:
                            ParseServerFloorChangeUp(message);
                            break;
                        case 0xBF:
                            ParseServerFloorChangeDown(message);
                            break;
                        case 0xC8:
                            ParseServerOutfitWindow(message);
                            break;
                        case 0xD2:
                            ParseServerVipState(message);
                            break;
                        case 0xD3:
                            ParseServerVipLogin(message);
                            break;
                        case 0xD4:
                            ParseServerVipLogout(message);
                            break;
                        case 0xF0:
                            ParseServerQuestList(message);
                            break;
                        case 0xF1:
                            ParseServerQuestPartList(message);
                            break;
                        case 0x7A:
                            ParseServerOpenShopWindow(message);
                            break;
                        case 0x7B:
                            ParseServerPlayerCash(message);
                            break;
                        case 0x7C:
                            ParseServerCloseShopWindow(message);
                            break;
                        case 0x9F:
                            ParseServerBasicData(message);
                            break;
                        case 0xDC:
                            ParseServerShowTutorial(message);
                            break;
                        case 0xDD:
                            ParseServerAddMapMarker(message);
                            break;
                        case 0xF3:
                            ParseServerChannelEvent(message);
                            break;
                        case 0xF6:
                            ParseServerMarketEnter(message);
                            break;
                        case 0xF7:
                            ParseServerMarketLeave(message);
                            break;
                        case 0xF8:
                            ParseServerMarketDetail(message);
                            break;
                        case 0xF9:
                            ParseServerMarketBrowser(message);
                            break;
                        default:
                            throw new Exception("ProtocolWorld [ParseServerMessage]: Unkonw packet type " + cmd.ToString("X2"));
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message + "\nLast Packets: " + packets.ToArray().ToHexString() +
                    "\nPacket Bytes: " + message.Buffer.ToHexString(packetStart, message.ReadPosition - packetStart));
            }
        }

        private void ParseServerChannelEvent(InMessage message)
        {
            var channelId = message.ReadUShort();
            var playerName = message.ReadString();
            var channelEvent = message.ReadByte();
        }

        private void ParseServerMarketEnter(InMessage message)
        {
            message.ReadUInt();
            message.ReadByte();
            var num = message.ReadUShort();
            for (int i = 0; i < num; i++)
                message.ReadUInt();
        }

        private void ParseServerMarketDetail(InMessage message)
        {
            message.ReadUShort();
            for (int i = 0; i < 15; i++)
                message.ReadString();

            var num2 = message.ReadByte();
            if (num2 > 0)
                message.ReadBytes(num2 * 16);
            num2 = message.ReadByte();
            if (num2 > 0)
                message.ReadBytes(num2 * 16);
        }

        private void ParseServerMarketBrowser(InMessage message)
        {
            ushort num = message.ReadUShort();
            if (num == 65535)
            {
                var count = message.ReadUInt();
                for (int i = 0; i < count; i++)
                {
                    message.ReadUInt();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUInt();
                    message.ReadByte();
                }
                count = message.ReadUInt();
                for (int j = 0; j <= count; j++)
                {
                    message.ReadUInt();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUInt();
                    message.ReadByte();
                }
            }
            else if (num == 65534)
            {
                var count = message.ReadUInt();
                for (int k = 0; k <= count; k++)
                {
                    message.ReadUInt();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUInt();
                }
                count = message.ReadUInt();
                for (int l = 0; l <= count; l++)
                {
                    message.ReadUInt();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUInt();
                }
            }
            else
            {
                var count = message.ReadUInt();
                for (int m = 0; m < count; m++)
                {
                    message.ReadUInt();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUInt();
                    message.ReadString();
                }
                count = message.ReadUInt();
                for (int n = 0; n < count; n++)
                {
                    message.ReadUInt();
                    message.ReadUShort();
                    message.ReadUShort();
                    message.ReadUInt();
                    message.ReadString();
                }
            }
        }

        private void ParseServerMarketLeave(InMessage message)
        {
        }

        private void ParseServerBasicData(InMessage message)
        {
            var isPremmium = message.ReadByte();
            var vocation = message.ReadByte();

            var knowSpells = message.ReadUShort();

            message.ReadBytes(knowSpells);
        }

        private void ParseServerAddMapMarker(InMessage message)
        {
            Location location = message.ReadLocation();
            var icon = message.ReadByte();
            var desc = message.ReadString();
        }

        private void ParseServerShowTutorial(InMessage message)
        {
            var tutorialID = message.ReadByte();
        }

        private void ParseServerCloseShopWindow(InMessage message)
        {
        }

        private void ParseServerPlayerCash(InMessage message)
        {
            var cash = message.ReadUInt();
            var num = message.ReadByte();
            message.ReadBytes(num * 3);
        }

        private void ParseServerOpenShopWindow(InMessage message)
        {
            var shop = new Shop(message.ReadString());
            var size = message.ReadUShort();
            for (uint i = 0; i < size; ++i)
            {
                var shopItem = new ShopItem();
                shopItem.Id = message.ReadUShort();
                shopItem.SubType = message.ReadByte();
                shopItem.Name = message.ReadString();
                shopItem.Weight = message.ReadUInt();
                shopItem.BuyPrice = message.ReadUInt();
                shopItem.SellPrice = message.ReadUInt();

                shop.Items.Add(shopItem);
            }

            client.OnOpenShopWindow(shop);
        }

        private void ParseServerQuestPartList(InMessage message)
        {
            var questsID = message.ReadUShort();
            var nMission = message.ReadByte();
            for (uint i = 0; i < nMission; ++i)
            {
                var questsName = message.ReadString();
                var questsDesc = message.ReadString();
            }
        }

        private void ParseServerQuestList(InMessage message)
        {
            var nQuests = message.ReadUShort();
            for (uint i = 0; i < nQuests; ++i)
            {
                var questsID = message.ReadUShort();
                var questsName = message.ReadString();
                var questsState = message.ReadByte();
            }
        }

        private void ParseServerVipLogout(InMessage message)
        {
            var creatureID = message.ReadUInt();
        }

        private void ParseServerVipLogin(InMessage message)
        {
            var creatureID = message.ReadUInt();
        }

        private void ParseServerVipState(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var name = message.ReadString();

            if (client.Version.Number > ClientVersion.Version961.Number)
            {
                var unk1 = message.ReadString();
                var unk2 = message.ReadUInt();
                var unk3 = message.ReadByte();
            }

            var unk4 = message.ReadByte(); //status?
        }

        private void ParseServerOutfitWindow(InMessage message)
        {
            message.ReadOutfit();
            var nOutfits = message.ReadByte();
            for (uint i = 0; i < nOutfits; ++i)
            {
                var outfitID = message.ReadUShort();
                var name = message.ReadString();
                var addons = message.ReadByte();
            }

            int mountCount = message.ReadByte();
            for (int i = 0; i < mountCount; ++i)
            {
                int mountId = message.ReadUShort(); // mount type
                string mountName = message.ReadString(); // mount name
            }
        }

        private void ParseServerTextMessage(InMessage message)
        {
            var mClass = (MessageClasses)message.ReadByte();

            switch (mClass)
            {
                case MessageClasses.DAMAGE_DEALT:
                case MessageClasses.DAMAGE_RECEIVED:
                case MessageClasses.DAMAGE_OTHERS:
                    {
                        Location location = message.ReadLocation();

                        var detailsValue = message.ReadUInt();
                        var detailsColor = message.ReadByte();
                        var detailsSubValue = message.ReadUInt();
                        var deatilsSubColor = message.ReadByte();

                        break;
                    }

                case MessageClasses.EXPERIENCE:
                case MessageClasses.EXPERIENCE_OTHERS:
                case MessageClasses.HEALED:
                case MessageClasses.HEALED_OTHERS:
                    {
                        Location location = message.ReadLocation();
                        var detailsValue = message.ReadUInt();
                        var detailsColor = message.ReadByte();
                        break;
                    }

                default:
                    break;
            }

            var text = message.ReadString();
        }

        private void ParseServerClosePrivateChannel(InMessage message)
        {
            var channelId = message.ReadUShort();
        }

        private void ParseServerCreatePrivateChannel(InMessage message)
        {
            var channelId = message.ReadUShort();
            var name = message.ReadString();

            message.ReadUShort();
            message.ReadString();
            message.ReadUShort();
        }

        //private void ParseServerRuleViolationB1(InMessage message)
        //{
        //    message.ReadUShort();
        //}

        //private void ParseServerRuleViolationB0(InMessage message)
        //{
        //    message.ReadUShort();
        //}

        //private void ParseServerRuleViolationAF(InMessage message)
        //{
        //    message.ReadUShort();
        //}

        private void ParseServerOpenRuleViolation(InMessage message)
        {
            message.ReadUShort();
        }

        private void ParseServerOpenPrivatePlayerChat(InMessage message)
        {
            var name = message.ReadString();
        }

        private void ParseServerOpenChannel(InMessage message)
        {
            var channelId = message.ReadUShort();
            var name = message.ReadString();

            var num = message.ReadUShort();
            for (int i = 0; i < num; i++)
                message.ReadString();
            num = message.ReadUShort();
            for (int i = 0; i < num; i++)
                message.ReadString();
        }

        private void ParseServerChannelList(InMessage message)
        {
            var count = message.ReadByte();
            for (uint i = 0; i < count; ++i)
            {
                var channelId = message.ReadUShort();
                var name = message.ReadString();
            }
        }

        private void ParseServerCreatureSpeak(InMessage message)
        {
            var statementId = message.ReadUInt();
            var name = message.ReadString();
            var level = message.ReadUShort();
            var type = (MessageClasses)message.ReadByte();
            Location location = null;

            switch (type)
            {
                case MessageClasses.SPEAK_SAY:
                case MessageClasses.SPEAK_WHISPER:
                case MessageClasses.SPEAK_YELL:
                case MessageClasses.SPEAK_MONSTER_SAY:
                case MessageClasses.SPEAK_MONSTER_YELL:
                case MessageClasses.SPEAK_SPELL:
                case MessageClasses.NPC_FROM:
                    location = message.ReadLocation();
                    break;
                case MessageClasses.CHANNEL:
                case MessageClasses.CHANNEL_HIGHLIGHT:
                case MessageClasses.GAMEMASTER_CHANNEL:
                    var channelId = message.ReadUShort();
                    break;
                default:
                    break;
            }

            var text = message.ReadString();

            client.Chat.OnCreatureSpeak(statementId, name, level, type, location, text);
        }

        private void ParseServerHouseTextWindow(InMessage message)
        {
            var unk = message.ReadByte();
            var windowId = message.ReadUInt();
            var text = message.ReadString();
        }

        private void ParseServerItemTextWindow(InMessage message)
        {
            var windowID = message.ReadUInt();
            var itemID = message.ReadUShort();
            var maxlen = message.ReadUShort();
            var text = message.ReadString();
            var writter = message.ReadString();
            var date = message.ReadString();
        }

        private void ParseServerCreaturePassable(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var impassable = message.ReadByte();
        }

        private void ParseServerCreatureShields(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var shields = message.ReadByte();
        }

        private void ParseServerCreatureSkulls(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var skull = message.ReadByte();
        }

        private void ParseServerCreatureSpeed(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var speed = message.ReadUShort();
        }

        private void ParseServerCreatureOutfit(InMessage message)
        {
            var creatureID = message.ReadUInt();
            Creature creature = client.BattleList.GetCreature(creatureID);
            var outfit = message.ReadOutfit();
            if (creature != null)
                creature.Outfit = outfit;
        }

        private void ParseServerCreatureLight(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var level = message.ReadByte();
            var color = message.ReadByte();
        }

        private void ParseServerCreatureHealth(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var percent = message.ReadByte();
        }

        private void ParseServerCreatureSquare(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var color = message.ReadByte();
        }

        private void ParseServerDistanceShot(InMessage message)
        {
            Location fromLocation = message.ReadLocation();
            Location toLocation = message.ReadLocation();
            var effect = message.ReadByte();
        }

        private void ParseServerAnimatedText(InMessage message)
        {
            Location location = message.ReadLocation();
            var color = message.ReadByte();
            var text = message.ReadString();
        }

        private void ParseServerMagicEffect(InMessage message)
        {
            Location location = message.ReadLocation();
            var effect = message.ReadByte();
        }

        private void ParseServerWorldLight(InMessage message)
        {
            var level = message.ReadByte();
            var color = message.ReadByte();
        }

        private void ParseServerSafeTradeClose(InMessage message)
        {
        }

        private void ParseServerSafeTradeRequest(InMessage message, bool ack)
        {
            var name = message.ReadString();
            var count = message.ReadByte();

            for (uint i = 0; i < count; ++i)
            {
                Item item = GetItem(message, ushort.MaxValue);
            }
        }

        private void ParseServerSafeTradeRequestNoAck(InMessage message)
        {
            ParseServerSafeTradeRequest(message, false);
        }

        private void ParseServerSafeTradeRequestAck(InMessage message)
        {
            ParseServerSafeTradeRequest(message, true);
        }

        private void ParseServerInventoryResetSlot(InMessage message)
        {
            var slot = message.ReadByte();
        }

        private void ParseServerInventorySetSlot(InMessage message)
        {
            var slot = message.ReadByte();
            Item item = GetItem(message, ushort.MaxValue);
        }

        private void ParseServerContainerRemoveItem(InMessage message)
        {
            var cid = message.ReadByte();
            var slot = message.ReadByte();
        }

        private void ParseServerContainerUpdateItem(InMessage message)
        {
            var cid = message.ReadByte();
            var slot = message.ReadByte();
            Item item = GetItem(message, ushort.MaxValue);
        }

        private void ParseServerContainerAddItem(InMessage message)
        {
            var cid = message.ReadByte();
            Item item = GetItem(message, ushort.MaxValue);
        }

        private void ParseServerCloseContainer(InMessage message)
        {
            var cid = message.ReadByte();
        }

        private void ParseServerOpenContainer(InMessage message)
        {
            var containerId = message.ReadByte();
            var containerItem = GetItem(message, ushort.MaxValue);
            var name = message.ReadString();
            var capacity = message.ReadByte();
            var hasParent = message.ReadByte();
            var itemCount = message.ReadByte();

            for (uint i = 0; i < itemCount; ++i)
            {
                Item item = GetItem(message, ushort.MaxValue);
                if (item == null)
                    throw new Exception("Container Open - !item");
            }
        }

        private void ParseServerSpellGroupCooldown(InMessage message)
        {
            message.ReadByte(); //group id
            message.ReadUInt(); //time
        }

        private void ParseServerSpellCooldown(InMessage message)
        {
            message.ReadByte(); //icon
            message.ReadUInt(); //time
        }

        private void ParseServerWaitingList(InMessage message)
        {
            message.ReadString();
            message.ReadByte();
        }

        private void ParseServerFYIMessage(InMessage message)
        {
            message.ReadString();
        }

        private void ParseServerErrorMessage(InMessage message)
        {
            message.ReadString();
        }

        private void ParseServerGMActions(InMessage message)
        {
        }

        private void ParseServerPlayerCancelWalk(InMessage message)
        {
            var direction = message.ReadByte();
        }

        private void ParseServerFloorChangeDown(InMessage message)
        {
            Location myPos = client.PlayerLocation;
            myPos = new Location(myPos.X, myPos.Y, myPos.Z + 1);

            //going from surface to underground

            var tiles = new List<Tile>();

            int skipTiles = 0;
            if (myPos.Z == 8)
            {
                int j, i;
                for (i = myPos.Z, j = -1; i < (int)myPos.Z + 3; ++i, --j)
                    ParseServerFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, i, 18, 14, j, ref skipTiles);
            }
            //going further down
            else if (myPos.Z > 8 && myPos.Z < 14)
                ParseServerFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, myPos.Z + 2, 18, 14, -3, ref skipTiles);

            client.PlayerLocation = new Location(myPos.X - 1, myPos.Y - 1, myPos.Z);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseServerFloorChangeUp(InMessage message)
        {
            Location myPos = client.PlayerLocation;
            myPos = new Location(myPos.X, myPos.Y, myPos.Z - 1);

            var tiles = new List<Tile>();
            if (myPos.Z == 7)
            {
                int skip = 0;
                ParseServerFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 5, 18, 14, 3, ref skip); //(floor 7 and 6 already set)
                ParseServerFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 4, 18, 14, 4, ref skip);
                ParseServerFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 3, 18, 14, 5, ref skip);
                ParseServerFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 2, 18, 14, 6, ref skip);
                ParseServerFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 1, 18, 14, 7, ref skip);
                ParseServerFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 0, 18, 14, 8, ref skip);

            }
            else if (myPos.Z > 7)
            {
                int skip = 0;
                ParseServerFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, myPos.Z - 3, 18, 14, 3, ref skip);
            }

            client.PlayerLocation = new Location(myPos.X + 1, myPos.Y + 1, myPos.Z);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseServerCanReportBugs(InMessage message)
        {
            client.PlayerCanReportBugs = message.ReadByte() != 0;
        }

        private void ParseServerDeath(InMessage message)
        {
        }

        private void ParseServerPlayerCancelAttack(InMessage message)
        {
            var creatureId = message.ReadUInt(); //??
        }

        private void ParseServerPlayerIcons(InMessage message)
        {
            message.ReadUShort();
        }

        private void ParseServerPlayerSkills(InMessage message)
        {
            for (int i = 0; i <= (int)Skills.LAST; i++)
            {
                var skill = message.ReadByte();
                var skillBase = message.ReadByte();
                var skillPercent = message.ReadByte();
            }
        }

        private void ParseServerPlayerStats(InMessage message)
        {
            var health = message.ReadUShort();
            var healthMax = message.ReadUShort();

            var freeCapacity = message.ReadUInt();
            var capacity = message.ReadUInt();

            var experience = message.ReadULong();

            var level = message.ReadUShort();
            var levelPercent = message.ReadByte();

            var mana = message.ReadUShort();
            var manaMax = message.ReadUShort();

            var magicLevel = message.ReadByte();
            var baseMagicLevel = message.ReadByte();
            var magicLevelPercent = message.ReadByte();

            var soul = message.ReadByte();
            var stamina = message.ReadUShort();
            var speed = message.ReadUShort();
            var regeneration = message.ReadUShort();
            var offlineTranning = message.ReadUShort();
        }

        private void ParseServerCreatureMove(InMessage message)
        {
            Location oldLocation = message.ReadLocation();
            var oldStack = message.ReadByte();

            Location newLocation = message.ReadLocation();

            if (oldLocation.IsCreature)
            {
                var creatureId = oldLocation.GetCretureId(oldStack);
                Creature creature = client.BattleList.GetCreature(creatureId);

                if (creature == null)
                    throw new Exception("[ParseServerCreatureMove] Creature not found on battle list.");

                var tile = client.Map.GetTile(newLocation);
                if (tile == null)
                    throw new Exception("[ParseServerCreatureMove] New tile not found.");

                tile.AddThing(creature);
                client.Map.SetTile(tile);
            }
            else
            {
                Tile tile = client.Map.GetTile(oldLocation);
                if (tile == null)
                    throw new Exception("[ParseServerCreatureMove] Old tile not found.");

                Thing thing = tile.GetThing(oldStack);
                Creature creature = thing as Creature;
                if (creature == null)
                    return; //The client will send update tile.

                tile.RemoveThing(oldStack);
                client.Map.SetTile(tile);

                tile = client.Map.GetTile(newLocation);
                if (tile == null)
                    throw new Exception("[ParseServerCreatureMove] New tile not found.");

                tile.AddThing(creature);
                client.Map.SetTile(tile);

                //update creature direction
                if (oldLocation.X > newLocation.X)
                {
                    creature.LookDirection = Direction.DIRECTION_WEST;
                    creature.TurnDirection = Direction.DIRECTION_WEST;
                }
                else if (oldLocation.X < newLocation.X)
                {
                    creature.LookDirection = Direction.DIRECTION_EAST;
                    creature.TurnDirection = Direction.DIRECTION_EAST;
                }
                else if (oldLocation.Y > newLocation.Y)
                {
                    creature.LookDirection = Direction.DIRECTION_NORTH;
                    creature.TurnDirection = Direction.DIRECTION_NORTH;
                }
                else if (oldLocation.Y < newLocation.Y)
                {
                    creature.LookDirection = Direction.DIRECTION_SOUTH;
                    creature.TurnDirection = Direction.DIRECTION_SOUTH;
                }
            }
        }

        private void ParseServerTileRemoveThing(InMessage message)
        {
            Location location = message.ReadLocation();
            var stack = message.ReadByte();

            if (location.IsCreature) //TODO: Veirificar o porque disso.
                return;

            Tile tile = client.Map.GetTile(location);
            if (tile == null)
                throw new Exception("[ParseServerTileRemoveThing] Tile not found.");

            var thing = tile.GetThing(stack);
            if (thing == null) // The client will send update tile.
                return;

            tile.RemoveThing(stack);
        }

        private void ParseServerTileTransformThing(InMessage message)
        {
            Location location = message.ReadLocation();
            var stack = message.ReadByte();
            var thing = GetThing(message);

            if (!location.IsCreature)
            {
                //get tile
                Tile tile = client.Map.GetTile(location);
                if (tile == null)
                    throw new Exception("[ParseServerTileTransformThing] Tile not found.");

                var oldThing = tile.GetThing(stack);
                if (oldThing == null)
                    return; // the client will send update tile.

                tile.ReplaceThing(stack, thing);
                client.Map.SetTile(tile);
            }
        }

        private void ParseServerTileAddThing(InMessage message)
        {
            Location location = message.ReadLocation();
            var stack = message.ReadByte();

            Thing thing = GetThing(message);
            Tile tile = client.Map.GetTile(location);

            if (tile == null)
                throw new Exception("[ParseServerTileAddThing] Tile not found.");

            tile.AddThing(stack, thing);
            client.Map.SetTile(tile);
        }

        private void ParseServerUpdateTile(InMessage message)
        {
            Location location = message.ReadLocation();
            var thingId = message.PeekUShort();

            if (thingId == 0xFF01)
            {
                message.ReadUShort();
                Tile tile = client.Map.GetTile(location);
                if (tile == null)
                    throw new Exception("[ParseServerUpdateTile] Tile not found.");

                tile.Clear();
            }
            else
            {
                ParseServerTileDescription(message, location);
                message.ReadUShort();
            }
        }

        private void ParseServerMoveWest(InMessage message)
        {
            var location = new Location(client.PlayerLocation.X - 1, client.PlayerLocation.Y, client.PlayerLocation.Z);
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseServerMapDescription(message, tiles, location.X - 8, location.Y - 6, location.Z, 1, 14);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseServerMoveSouth(InMessage message)
        {
            var location = new Location(client.PlayerLocation.X, client.PlayerLocation.Y + 1, client.PlayerLocation.Z);
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseServerMapDescription(message, tiles, location.X - 8, location.Y + 7, location.Z, 18, 1);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseServerMoveEast(InMessage message)
        {
            var location = new Location(client.PlayerLocation.X + 1, client.PlayerLocation.Y, client.PlayerLocation.Z);
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseServerMapDescription(message, tiles, location.X + 9, location.Y - 6, location.Z, 1, 14);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseServerMoveNorth(InMessage message)
        {
            var location = new Location(client.PlayerLocation.X, client.PlayerLocation.Y - 1, client.PlayerLocation.Z);
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseServerMapDescription(message, tiles, location.X - 8, location.Y - 6, location.Z, 18, 1);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseServerPing(InMessage message)
        {
        }

        private void ParseServerPingBack(InMessage message)
        {
        }

        private void ParseServerMapDescription(InMessage message)
        {
            var location = message.ReadLocation();
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseServerMapDescription(message, tiles, location.X - 8, location.Y - 6, location.Z, 18, 14);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseServerMapDescription(InMessage message, List<Tile> tiles, int x, int y, int z, int width, int height)
        {
            int startz, endz, zstep;
            //calculate map limits
            if (z > 7)
            {
                startz = z - 2;
                endz = Math.Min(16 - 1, z + 2);
                zstep = 1;
            }
            else
            {
                startz = 7;
                endz = 0;
                zstep = -1;
            }

            int skipTiles = 0;
            for (int nz = startz; nz != endz + zstep; nz += zstep)
                ParseServerFloorDescription(message, tiles, x, y, nz, width, height, z - nz, ref skipTiles);
        }

        private void ParseServerFloorDescription(InMessage message, List<Tile> tiles, int x, int y, int z, int width, int height, int offset, ref int skipTiles)
        {
            for (int nx = 0; nx < width; nx++)
            {
                for (int ny = 0; ny < height; ny++)
                {
                    if (skipTiles == 0)
                    {
                        var tileOpt = message.PeekUShort();
                        // Decide if we have to skip tiles
                        // or if it is a real tile
                        if (tileOpt >= 0xFF00)
                            skipTiles = (short)(message.ReadUShort() & 0xFF);
                        else
                        {
                            //real tile so read tile
                            tiles.Add(ParseServerTileDescription(message, new Location(x + nx + offset, y + ny + offset, z)));
                            skipTiles = (short)(message.ReadUShort() & 0xFF);
                        }
                    }
                    else
                        skipTiles--;
                }
            }
        }

        private Tile ParseServerTileDescription(InMessage message, Location location)
        {
            Tile tile = new Tile(location);
            if (message.PeekUShort() < 0xFF00)
                message.ReadUShort();

            while (message.PeekUShort() < 0xFF00)
                tile.AddThing(GetThing(message));

            client.Map.SetTile(tile);
            return tile;
        }

        private Thing GetThing(InMessage message)
        {
            //get thing type
            var thingId = message.ReadUShort();

            if (thingId == 0x0061 || thingId == 0x0062)
            {
                //creatures
                Creature creature = null;
                if (thingId == 0x0062)
                {
                    creature = client.BattleList.GetCreature(message.ReadUInt());

                    if (creature == null)
                        throw new Exception("[GetThing] (0x0062) Can't find the creature in the battle list.");

                    creature.Health = message.ReadByte();
                }
                else if (thingId == 0x0061)
                { //creature is not known
                    client.BattleList.RemoveCreature(message.ReadUInt());

                    creature = new Creature(message.ReadUInt());
                    client.BattleList.AddCreature(creature);

                    creature.Type = (CreatureType)message.ReadByte();
                    creature.Name = message.ReadString();
                    creature.Health = message.ReadByte();
                }

                var direction = (Direction)message.ReadByte();
                creature.LookDirection = direction;
                creature.TurnDirection = direction;

                creature.Outfit = message.ReadOutfit();
                creature.LightLevel = message.ReadByte();
                creature.LightColor = message.ReadByte();
                creature.Speed = message.ReadUShort();
                creature.Skull = message.ReadByte();
                creature.Shield = message.ReadByte();

                if (thingId == 0x0061) // emblem is sent only in packet type 0x61
                    creature.Emblem = message.ReadByte();

                creature.IsImpassable = message.ReadBool();

                return creature;
            }
            else if (thingId == 0x0063)
            {
                Creature creature = client.BattleList.GetCreature(message.ReadUInt());
                if (creature == null)
                    throw new Exception("[GetThing] (0x0063)  Can't find the creature in the battle list.");

                creature.TurnDirection = (Direction)message.ReadByte();
                creature.IsImpassable = message.ReadBool();

                return creature;
            }
            else
                return GetItem(message, thingId);
        }

        private Item GetItem(InMessage message, ushort itemid)
        {
            if (itemid == ushort.MaxValue)
                itemid = message.ReadUShort();

            ItemType type = client.Items.Get(itemid);
            if (type == null)
                throw new Exception("[GetItem] (" + itemid + ") Can't find the item type.");

            byte count = 0;
            byte subtype = 0;

            if (type.IsStackable)
                count = message.ReadByte();
            else if (type.IsSplash || type.IsFluidContainer)
                subtype = message.ReadByte();

            if (type.IsAnimation)
                message.ReadByte(); // Desconhecido

            return new Item(type, count, subtype);
        }

        private void ParseServerSelfAppear(InMessage message)
        {
            client.BattleList.Clear();
            client.Map.Clear();

            client.PlayerId = message.ReadUInt();
            message.ReadUShort();
            client.PlayerCanReportBugs = message.ReadByte() != 0;
        }
        #endregion

        #region SendServer
        internal void SendServerSay(string text, MessageClasses type)
        {
            if (!client.LoggedIn)
                return;

            var message = new OutMessage();
            message.WriteByte(0x96);
            message.WriteByte((byte)type);
            message.WriteString(text);

            client.Proxy.SendToServer(message);
        }
        #endregion
    }
}
