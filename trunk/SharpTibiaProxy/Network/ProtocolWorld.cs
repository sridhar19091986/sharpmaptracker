using System;
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

        public void ParseMessage(InMessage message)
        {
            var packets = new List<byte>();
            var packetStart = 0;
            try
            {

                while (message.ReadPosition < message.Size)
                {
                    byte cmd = message.ReadByte();
                    packets.Add(cmd);
                    packetStart = message.ReadPosition;

                    switch (cmd)
                    {
                        case 0x0A:
                            ParseSelfAppear(message);
                            break;
                        case 0x0B:
                            ParseGMActions(message);
                            break;
                        case 0x14:
                            ParseErrorMessage(message);
                            break;
                        case 0x15:
                            ParseFYIMessage(message);
                            break;
                        case 0x16:
                            ParseWaitingList(message);
                            break;
                        case 0x1D:
                            ParsePing(message);
                            break;
                        case 0x1E:
                            ParsePingBack(message);
                            break;
                        case 0x28:
                            ParseDeath(message);
                            break;
                        case 0x32:
                            ParseCanReportBugs(message);
                            break;
                        case 0x64:
                            ParseMapDescription(message);
                            break;
                        case 0x65:
                            ParseMoveNorth(message);
                            break;
                        case 0x66:
                            ParseMoveEast(message);
                            break;
                        case 0x67:
                            ParseMoveSouth(message);
                            break;
                        case 0x68:
                            ParseMoveWest(message);
                            break;
                        case 0x69:
                            ParseUpdateTile(message);
                            break;
                        case 0x6A:
                            ParseTileAddThing(message);
                            break;
                        case 0x6B:
                            ParseTileTransformThing(message);
                            break;
                        case 0x6C:
                            ParseTileRemoveThing(message);
                            break;
                        case 0x6D:
                            ParseCreatureMove(message);
                            break;
                        case 0x6E:
                            ParseOpenContainer(message);
                            break;
                        case 0x6F:
                            ParseCloseContainer(message);
                            break;
                        case 0x70:
                            ParseContainerAddItem(message);
                            break;
                        case 0x71:
                            ParseContainerUpdateItem(message);
                            break;
                        case 0x72:
                            ParseContainerRemoveItem(message);
                            break;
                        case 0x78:
                            ParseInventorySetSlot(message);
                            break;
                        case 0x79:
                            ParseInventoryResetSlot(message);
                            break;
                        case 0x7D:
                            ParseSafeTradeRequestAck(message);
                            break;
                        case 0x7E:
                            ParseSafeTradeRequestNoAck(message);
                            break;
                        case 0x7F:
                            ParseSafeTradeClose(message);
                            break;
                        case 0x82:
                            ParseWorldLight(message);
                            break;
                        case 0x83:
                            ParseMagicEffect(message);
                            break;
                        case 0x84:
                            ParseAnimatedText(message);
                            break;
                        case 0x85:
                            ParseDistanceShot(message);
                            break;
                        case 0x86:
                            ParseCreatureSquare(message);
                            break;
                        case 0x87:
                            byte b = message.ReadByte();
                            if (b > 0)
                                message.ReadBytes(b * 4);
                            break;
                        case 0x8C:
                            ParseCreatureHealth(message);
                            break;
                        case 0x8D:
                            ParseCreatureLight(message);
                            break;
                        case 0x8E:
                            ParseCreatureOutfit(message);
                            break;
                        case 0x8F:
                            ParseCreatureSpeed(message);
                            break;
                        case 0x90:
                            ParseCreatureSkulls(message);
                            break;
                        case 0x91:
                            ParseCreatureShields(message);
                            break;
                        case 0x92:
                            ParseCreaturePassable(message);
                            break;
                        case 0x96:
                            ParseItemTextWindow(message);
                            break;
                        case 0x97:
                            ParseHouseTextWindow(message);
                            break;
                        case 0xA0:
                            ParsePlayerStats(message);
                            break;
                        case 0xA1:
                            ParsePlayerSkills(message);
                            break;
                        case 0xA2:
                            ParsePlayerIcons(message);
                            break;
                        case 0xA3:
                            ParsePlayerCancelAttack(message);
                            break;
                        case 0xA4:
                            ParseSpellCooldown(message);
                            break;
                        case 0xA5:
                            ParseSpellGroupCooldown(message);
                            break;
                        case 0xA6: //desconhecido
                            message.ReadUInt();
                            break;
                        case 0xAA:
                            ParseCreatureSpeak(message);
                            break;
                        case 0xAB:
                            ParseChannelList(message);
                            break;
                        case 0xAC:
                            ParseOpenChannel(message);
                            break;
                        case 0xAD:
                            ParseOpenPrivatePlayerChat(message);
                            break;
                        case 0xAE:
                            ParseOpenRuleViolation(message);
                            break;
                        case 0xB2:
                            ParseCreatePrivateChannel(message);
                            break;
                        case 0xB3:
                            ParseClosePrivateChannel(message);
                            break;
                        case 0xB4:
                            ParseTextMessage(message);
                            break;
                        case 0xB5:
                            ParsePlayerCancelWalk(message);
                            break;
                        case 0xB6:
                            message.ReadUShort();
                            break;
                        case 0xBE:
                            ParseFloorChangeUp(message);
                            break;
                        case 0xBF:
                            ParseFloorChangeDown(message);
                            break;
                        case 0xC8:
                            ParseOutfitWindow(message);
                            break;
                        case 0xD2:
                            ParseVipState(message);
                            break;
                        case 0xD3:
                            ParseVipLogin(message);
                            break;
                        case 0xD4:
                            ParseVipLogout(message);
                            break;
                        case 0xF0:
                            ParseQuestList(message);
                            break;
                        case 0xF1:
                            ParseQuestPartList(message);
                            break;
                        case 0x7A:
                            ParseOpenShopWindow(message);
                            break;
                        case 0x7B:
                            ParsePlayerCash(message);
                            break;
                        case 0x7C:
                            ParseCloseShopWindow(message);
                            break;
                        case 0x9F:
                            ParseBasicData(message);
                            break;
                        case 0xDC:
                            ParseShowTutorial(message);
                            break;
                        case 0xDD:
                            ParseAddMapMarker(message);
                            break;
                        case 0xF3:
                            ParseChannelEvent(message);
                            break;
                        case 0xF6:
                            ParseMarketEnter(message);
                            break;
                        case 0xF7:
                            ParseMarketLeave(message);
                            break;
                        case 0xF8:
                            ParseMarketDetail(message);
                            break;
                        case 0xF9:
                            ParseMarketBrowser(message);
                            break;
                        default:
                            throw new Exception("ProtocolWorld [ParseMessage]: Unkonw packet type " + cmd.ToString("X2"));
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message + "\nLast Packets: " + packets.ToArray().ToHexString() + 
                    "\nPacket Bytes: " + message.Buffer.ToHexString(packetStart, message.ReadPosition - packetStart));
            }
        }

        private void ParseChannelEvent(InMessage message)
        {
            var channelId = message.ReadUShort();
            var playerName = message.ReadString();
            var channelEvent = message.ReadByte();
        }

        private void ParseMarketEnter(InMessage message)
        {
            message.ReadUInt();
            message.ReadByte();
            var num = message.ReadUShort();
            for (int i = 0; i < num; i++)
                message.ReadUInt();
        }

        private void ParseMarketDetail(InMessage message)
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

        private void ParseMarketBrowser(InMessage message)
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

        private void ParseMarketLeave(InMessage message)
        {
        }

        private void ParseBasicData(InMessage message)
        {
            var isPremmium = message.ReadByte();
            var vocation = message.ReadByte();

            var knowSpells = message.ReadUShort();

            message.ReadBytes(knowSpells);
        }

        private void ParseAddMapMarker(InMessage message)
        {
            Location location = message.ReadLocation();
            var icon = message.ReadByte();
            var desc = message.ReadString();
        }

        private void ParseShowTutorial(InMessage message)
        {
            var tutorialID = message.ReadByte();
        }

        private void ParseCloseShopWindow(InMessage message)
        {
        }

        private void ParsePlayerCash(InMessage message)
        {
            var cash = message.ReadUInt();
            var num = message.ReadByte();
            message.ReadBytes(num * 3);
        }

        private void ParseOpenShopWindow(InMessage message)
        {
            message.ReadString();
            var size = message.ReadUShort();
            for (uint i = 0; i < size; ++i)
            {
                var itemid = message.ReadUShort();
                var subtype = message.ReadByte();
                var itemname = message.ReadString();
                message.ReadUInt();
                message.ReadUInt();
                message.ReadUInt();
            }
        }

        private void ParseQuestPartList(InMessage message)
        {
            var questsID = message.ReadUShort();
            var nMission = message.ReadByte();
            for (uint i = 0; i < nMission; ++i)
            {
                var questsName = message.ReadString();
                var questsDesc = message.ReadString();
            }
        }

        private void ParseQuestList(InMessage message)
        {
            var nQuests = message.ReadUShort();
            for (uint i = 0; i < nQuests; ++i)
            {
                var questsID = message.ReadUShort();
                var questsName = message.ReadString();
                var questsState = message.ReadByte();
            }
        }

        private void ParseVipLogout(InMessage message)
        {
            var creatureID = message.ReadUInt();
        }

        private void ParseVipLogin(InMessage message)
        {
            var creatureID = message.ReadUInt();
        }

        private void ParseVipState(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var name = message.ReadString();
            var unk1 = message.ReadString();
            var unk2 = message.ReadUInt();
            var unk3 = message.ReadByte();
            var unk4 = message.ReadByte();
        }

        private void ParseOutfitWindow(InMessage message)
        {
            message.ReadOutfit();
            var nOutfits = message.ReadByte();
            for (uint i = 0; i < nOutfits; ++i)
            {
                var outfitID = message.ReadUShort();
                var name = message.ReadString();
                var addons = message.ReadByte();
            }
        }

        private void ParseTextMessage(InMessage message)
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

        private void ParseClosePrivateChannel(InMessage message)
        {
            var channelId = message.ReadUShort();
        }

        private void ParseCreatePrivateChannel(InMessage message)
        {
            var channelId = message.ReadUShort();
            var name = message.ReadString();

            message.ReadUShort();
            message.ReadString();
            message.ReadUShort();
        }

        private void ParseRuleViolationB1(InMessage message)
        {
            message.ReadUShort();
        }

        private void ParseRuleViolationB0(InMessage message)
        {
            message.ReadUShort();
        }

        private void ParseRuleViolationAF(InMessage message)
        {
            message.ReadUShort();
        }

        private void ParseOpenRuleViolation(InMessage message)
        {
            message.ReadUShort();
        }

        private void ParseOpenPrivatePlayerChat(InMessage message)
        {
            var name = message.ReadString();
        }

        private void ParseOpenChannel(InMessage message)
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

        private void ParseChannelList(InMessage message)
        {
            var count = message.ReadByte();
            for (uint i = 0; i < count; ++i)
            {
                var channelId = message.ReadUShort();
                var name = message.ReadString();
            }
        }

        private void ParseCreatureSpeak(InMessage message)
        {
            var statementId = message.ReadUInt();
            var name = message.ReadString();
            var level = message.ReadUShort();

            var type = (MessageClasses)message.ReadByte();

            switch (type)
            {
                case MessageClasses.SPEAK_SAY:
                case MessageClasses.SPEAK_WHISPER:
                case MessageClasses.SPEAK_YELL:
                case MessageClasses.SPEAK_MONSTER_SAY:
                case MessageClasses.SPEAK_MONSTER_YELL:
                case MessageClasses.SPEAK_SPELL:
                case MessageClasses.NPC_FROM:
                    Location location = message.ReadLocation();
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
        }

        private void ParseHouseTextWindow(InMessage message)
        {
            var unk = message.ReadByte();
            var windowId = message.ReadUInt();
            var text = message.ReadString();
        }

        private void ParseItemTextWindow(InMessage message)
        {
            var windowID = message.ReadUInt();
            var itemID = message.ReadUShort();
            var maxlen = message.ReadUShort();
            var text = message.ReadString();
            var writter = message.ReadString();
            var date = message.ReadString();
        }

        private void ParseCreaturePassable(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var impassable = message.ReadByte();
        }

        private void ParseCreatureShields(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var shields = message.ReadByte();
        }

        private void ParseCreatureSkulls(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var skull = message.ReadByte();
        }

        private void ParseCreatureSpeed(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var speed = message.ReadUShort();
        }

        private void ParseCreatureOutfit(InMessage message)
        {
            var creatureID = message.ReadUInt();
            Creature creature = client.BattleList.GetCreature(creatureID);
            var outfit = message.ReadOutfit();
            if (creature != null)
                creature.Outfit = outfit;
        }

        private void ParseCreatureLight(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var level = message.ReadByte();
            var color = message.ReadByte();
        }

        private void ParseCreatureHealth(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var percent = message.ReadByte();
        }

        private void ParseCreatureSquare(InMessage message)
        {
            var creatureID = message.ReadUInt();
            var color = message.ReadByte();
        }

        private void ParseDistanceShot(InMessage message)
        {
            Location fromLocation = message.ReadLocation();
            Location toLocation = message.ReadLocation();
            var effect = message.ReadByte();
        }

        private void ParseAnimatedText(InMessage message)
        {
            Location location = message.ReadLocation();
            var color = message.ReadByte();
            var text = message.ReadString();
        }

        private void ParseMagicEffect(InMessage message)
        {
            Location location = message.ReadLocation();
            var effect = message.ReadByte();
        }

        private void ParseWorldLight(InMessage message)
        {
            var level = message.ReadByte();
            var color = message.ReadByte();
        }

        private void ParseSafeTradeClose(InMessage message)
        {
        }

        private void ParseSafeTradeRequest(InMessage message, bool ack)
        {
            var name = message.ReadString();
            var count = message.ReadByte();

            for (uint i = 0; i < count; ++i)
            {
                Item item = GetItem(message, ushort.MaxValue);
            }
        }

        private void ParseSafeTradeRequestNoAck(InMessage message)
        {
            ParseSafeTradeRequest(message, false);
        }

        private void ParseSafeTradeRequestAck(InMessage message)
        {
            ParseSafeTradeRequest(message, true);
        }

        private void ParseInventoryResetSlot(InMessage message)
        {
            var slot = message.ReadByte();
        }

        private void ParseInventorySetSlot(InMessage message)
        {
            var slot = message.ReadByte();
            Item item = GetItem(message, ushort.MaxValue);
        }

        private void ParseContainerRemoveItem(InMessage message)
        {
            var cid = message.ReadByte();
            var slot = message.ReadByte();
        }

        private void ParseContainerUpdateItem(InMessage message)
        {
            var cid = message.ReadByte();
            var slot = message.ReadByte();
            Item item = GetItem(message, ushort.MaxValue);
        }

        private void ParseContainerAddItem(InMessage message)
        {
            var cid = message.ReadByte();
            Item item = GetItem(message, ushort.MaxValue);
        }

        private void ParseCloseContainer(InMessage message)
        {
            var cid = message.ReadByte();
        }

        private void ParseOpenContainer(InMessage message)
        {
            var cid = message.ReadByte();
            var itemid = message.ReadUShort();
            var name = message.ReadString();
            var capacity = message.ReadByte();
            var hasParent = message.ReadByte();
            var size = message.ReadByte();

            for (uint i = 0; i < size; ++i)
            {
                Item item = GetItem(message, ushort.MaxValue);
                if (item == null)
                    throw new Exception("Container Open - !item");
            }
        }

        private void ParseSpellGroupCooldown(InMessage message)
        {
            message.ReadByte(); //group id
            message.ReadUInt(); //time
        }

        private void ParseSpellCooldown(InMessage message)
        {
            message.ReadByte(); //icon
            message.ReadUInt(); //time
        }

        private void ParseWaitingList(InMessage message)
        {
            message.ReadString();
            message.ReadByte();
        }

        private void ParseFYIMessage(InMessage message)
        {
            message.ReadString();
        }

        private void ParseErrorMessage(InMessage message)
        {
            message.ReadString();
        }

        private void ParseGMActions(InMessage message)
        {
        }

        private void ParsePlayerCancelWalk(InMessage message)
        {
            var direction = message.ReadByte();
        }

        private void ParseFloorChangeDown(InMessage message)
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
                    ParseFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, i, 18, 14, j, ref skipTiles);
            }
            //going further down
            else if (myPos.Z > 8 && myPos.Z < 14)
                ParseFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, myPos.Z + 2, 18, 14, -3, ref skipTiles);

            client.PlayerLocation = new Location(myPos.X - 1, myPos.Y - 1, myPos.Z);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseFloorChangeUp(InMessage message)
        {
            Location myPos = client.PlayerLocation;
            myPos = new Location(myPos.X, myPos.Y, myPos.Z - 1);

            var tiles = new List<Tile>();
            if (myPos.Z == 7)
            {
                int skip = 0;
                ParseFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 5, 18, 14, 3, ref skip); //(floor 7 and 6 already set)
                ParseFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 4, 18, 14, 4, ref skip);
                ParseFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 3, 18, 14, 5, ref skip);
                ParseFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 2, 18, 14, 6, ref skip);
                ParseFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 1, 18, 14, 7, ref skip);
                ParseFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, 0, 18, 14, 8, ref skip);

            }
            else if (myPos.Z > 7)
            {
                int skip = 0;
                ParseFloorDescription(message, tiles, myPos.X - 8, myPos.Y - 6, myPos.Z - 3, 18, 14, 3, ref skip);
            }

            client.PlayerLocation = new Location(myPos.X + 1, myPos.Y + 1, myPos.Z);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseCanReportBugs(InMessage message)
        {
            client.PlayerCanReportBugs = message.ReadByte() != 0;
        }

        private void ParseDeath(InMessage message)
        {
        }

        private void ParsePlayerCancelAttack(InMessage message)
        {
            var creatureId = message.ReadUInt(); //??
        }

        private void ParsePlayerIcons(InMessage message)
        {
            message.ReadUShort();
        }

        private void ParsePlayerSkills(InMessage message)
        {
            for (int i = 0; i <= (int)Skills.LAST; i++)
            {
                var skill = message.ReadByte();
                var skillBase = message.ReadByte();
                var skillPercent = message.ReadByte();
            }
        }

        private void ParsePlayerStats(InMessage message)
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

        private void ParseCreatureMove(InMessage message)
        {
            Location oldLocation = message.ReadLocation();
            var oldStack = message.ReadByte();

            Location newLocation = message.ReadLocation();

            if (oldLocation.IsCreature)
            {
                var creatureId = oldLocation.GetCretureId(oldStack);
                Creature creature = client.BattleList.GetCreature(creatureId);

                if (creature == null)
                    throw new Exception("[ParseCreatureMove] Creature not found on battle list.");

                var tile = client.Map.GetTile(newLocation);
                if (tile == null)
                    throw new Exception("[ParseCreatureMove] New tile not found.");

                tile.AddThing(creature);
                client.Map.SetTile(tile);
            }
            else
            {
                Tile tile = client.Map.GetTile(oldLocation);
                if (tile == null)
                    throw new Exception("[ParseCreatureMove] Old tile not found.");

                Thing thing = tile.GetThing(oldStack);
                Creature creature = thing as Creature;
                if (creature == null)
                    throw new Exception("[ParseCreatureMove] Creature not found on tile.");

                tile.RemoveThing(oldStack);
                client.Map.SetTile(tile);

                tile = client.Map.GetTile(newLocation);
                if (tile == null)
                    throw new Exception("[ParseCreatureMove] New tile not found.");

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

        private void ParseTileRemoveThing(InMessage message)
        {
            Location location = message.ReadLocation();
            var stack = message.ReadByte();

            if (location.IsCreature) //TODO: Veirificar o porque disso.
                return;

            Tile tile = client.Map.GetTile(location);
            if (tile == null)
                throw new Exception("[ParseTileRemoveThing] Tile not found.");

            tile.RemoveThing(stack);
        }

        private void ParseTileTransformThing(InMessage message)
        {
            Location location = message.ReadLocation();
            var stack = message.ReadByte();
            var thing = GetThing(message);

            if (!location.IsCreature)
            {
                //get tile
                Tile tile = client.Map.GetTile(location);

                if (tile == null)
                    throw new Exception("[ParseTileTransformThing] Tile not found.");

                tile.ReplaceThing(stack, thing);
                client.Map.SetTile(tile);
            }
        }

        private void ParseTileAddThing(InMessage message)
        {
            Location location = message.ReadLocation();
            var stack = message.ReadByte();

            Thing thing = GetThing(message);
            Tile tile = client.Map.GetTile(location);

            if (tile == null)
                throw new Exception("[ParseTileAddThing] Tile not found.");

            tile.AddThing(stack, thing);
            client.Map.SetTile(tile);
        }

        private void ParseUpdateTile(InMessage message)
        {
            Location location = message.ReadLocation();
            var thingId = message.PeekUShort();

            if (thingId == 0xFF01)
            {
                message.ReadUShort();
                Tile tile = client.Map.GetTile(location);
                if (tile == null)
                    throw new Exception("[ParseUpdateTile] Tile not found.");

                tile.Clear();
            }
            else
            {
                ParseTileDescription(message, location);
                message.ReadUShort();
            }
        }

        private void ParseMoveWest(InMessage message)
        {
            var location = new Location(client.PlayerLocation.X - 1, client.PlayerLocation.Y, client.PlayerLocation.Z);
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseMapDescription(message, tiles, location.X - 8, location.Y - 6, location.Z, 1, 14);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseMoveSouth(InMessage message)
        {
            var location = new Location(client.PlayerLocation.X, client.PlayerLocation.Y + 1, client.PlayerLocation.Z);
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseMapDescription(message, tiles, location.X - 8, location.Y + 7, location.Z, 18, 1);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseMoveEast(InMessage message)
        {
            var location = new Location(client.PlayerLocation.X + 1, client.PlayerLocation.Y, client.PlayerLocation.Z);
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseMapDescription(message, tiles, location.X + 9, location.Y - 6, location.Z, 1, 14);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseMoveNorth(InMessage message)
        {
            var location = new Location(client.PlayerLocation.X, client.PlayerLocation.Y - 1, client.PlayerLocation.Z);
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseMapDescription(message, tiles, location.X - 8, location.Y - 6, location.Z, 18, 1);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParsePing(InMessage message)
        {
        }

        private void ParsePingBack(InMessage message)
        {
        }

        private void ParseMapDescription(InMessage message)
        {
            var location = message.ReadLocation();
            client.PlayerLocation = location;

            var tiles = new List<Tile>();
            ParseMapDescription(message, tiles, location.X - 8, location.Y - 6, location.Z, 18, 14);
            client.Map.OnMapUpdated(tiles);
        }

        private void ParseMapDescription(InMessage message, List<Tile> tiles, int x, int y, int z, int width, int height)
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
                ParseFloorDescription(message, tiles, x, y, nz, width, height, z - nz, ref skipTiles);
        }

        private void ParseFloorDescription(InMessage message, List<Tile> tiles, int x, int y, int z, int width, int height, int offset, ref int skipTiles)
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
                            tiles.Add(ParseTileDescription(message, new Location(x + nx + offset, y + ny + offset, z)));
                            skipTiles = (short)(message.ReadUShort() & 0xFF);
                        }
                    }
                    else
                        skipTiles--;
                }
            }
        }

        private Tile ParseTileDescription(InMessage message, Location location)
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

        private void ParseSelfAppear(InMessage message)
        {
            client.BattleList.Clear();
            client.Map.Clear();

            client.PlayerId = message.ReadUInt();
            message.ReadUShort();
            client.PlayerCanReportBugs = message.ReadByte() != 0;
        }
    }
}
