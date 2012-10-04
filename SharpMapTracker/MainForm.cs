using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpTibiaProxy.Domain;
using System.Diagnostics;
using System.IO;
using SharpTibiaProxy;
using SharpTibiaProxy.Network;
using System.Threading;
using SharpTibiaProxy.Util;
using System.Xml.Linq;

namespace SharpMapTracker
{
    public partial class MainForm : Form
    {
        private const int MAP_SCROLL_SPEED = 8;

        private Client client;
        private OtItems otItems;

        private OtMap map;
        private Dictionary<string, NpcInfo> npcs;
        private string lastPlayerSpeech;
        private DateTime lastPlayerSpeechTime;
        private uint sendNpcWordScheduleId;

        public bool TrackMoveableItems { get; set; }
        public bool TrackSplashes { get; set; }
        public bool TrackMonsters { get; set; }
        public bool TrackNPCs { get; set; }
        public bool TrackOnlyCurrentFloor { get; set; }
        public bool NPCAutoTalk { get; set; }

        public MainForm()
        {
            InitializeComponent();

            Text = "SharpMapTracker v" + Constants.MAP_TRACKER_VERSION;

            map = new OtMap();
            npcs = new Dictionary<string, NpcInfo>();

            DataBindings.Add("TopMost", alwaysOnTopCheckBox, "Checked");
            DataBindings.Add("TrackMoveableItems", trackMoveableItemsCheckBox, "Checked");
            DataBindings.Add("TrackSplashes", trackSplashesCheckBox, "Checked");
            DataBindings.Add("TrackMonsters", trackMonstersCheckBox, "Checked");
            DataBindings.Add("TrackNPCs", trackNpcsCheckBox, "Checked");
            DataBindings.Add("TrackOnlyCurrentFloor", trackOnlyCurrentFloorCheckBox, "Checked");
            DataBindings.Add("NPCAutoTalk", npcAutoTalkCheckBox, "Checked");

            Trace.Listeners.Add(new TextBoxTraceListener(traceTextBox));
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));

            Trace.AutoFlush = true;

            KeyDown += new KeyEventHandler(MainForm_KeyDown);
            Load += MainForm_Load;
            FormClosed += new FormClosedEventHandler(MainForm_FormClosed);
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            LoadItems();
            NpcWordList.Load();
        }

        void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Client = null;
            NpcWordList.Save();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageUp || e.KeyValue == 0x6B)
            {
                e.SuppressKeyPress = true;
                miniMap.Floor++;
            }
            else if (e.KeyCode == Keys.PageDown || e.KeyValue == 0x6D)
            {
                e.SuppressKeyPress = true;
                miniMap.Floor--;
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
                if (miniMap.CenterLocation != null)
                    miniMap.CenterLocation = new Location(miniMap.CenterLocation.X, miniMap.CenterLocation.Y - MAP_SCROLL_SPEED, miniMap.CenterLocation.Z);
            }
            else if (e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
                if (miniMap.CenterLocation != null)
                    miniMap.CenterLocation = new Location(miniMap.CenterLocation.X, miniMap.CenterLocation.Y + MAP_SCROLL_SPEED, miniMap.CenterLocation.Z);
            }
            else if (e.KeyCode == Keys.Left)
            {
                e.SuppressKeyPress = true;
                if (miniMap.CenterLocation != null)
                    miniMap.CenterLocation = new Location(miniMap.CenterLocation.X - MAP_SCROLL_SPEED, miniMap.CenterLocation.Y, miniMap.CenterLocation.Z);
            }
            else if (e.KeyCode == Keys.Right)
            {
                e.SuppressKeyPress = true;
                if (miniMap.CenterLocation != null)
                    miniMap.CenterLocation = new Location(miniMap.CenterLocation.X + MAP_SCROLL_SPEED, miniMap.CenterLocation.Y, miniMap.CenterLocation.Z);
            }

        }

        protected Client Client
        {
            get { return client; }
            set
            {
                if (client != null)
                {
                    client.Map.Updated -= Map_Updated;
                    client.BattleList.CreatureAdded -= BattleList_CreatureAdded;
                    client.Chat.CreatureSpeak -= Chat_CreatureSpeak;
                    client.Chat.PlayerSpeak -= Chat_PlayerSpeak;
                    client.OpenShopWindow -= Client_OpenShopWindow;
                    client.Exited -= client_Exited;

                    client.Dispose();
                }

                client = value;

                if (client != null)
                {
                    client.Map.Updated += Map_Updated;
                    client.BattleList.CreatureAdded += BattleList_CreatureAdded;
                    client.Chat.CreatureSpeak += Chat_CreatureSpeak;
                    client.Chat.PlayerSpeak += Chat_PlayerSpeak;
                    client.OpenShopWindow += Client_OpenShopWindow;
                    client.Exited += client_Exited;
                }

            }
        }

        void client_Exited(object sender, EventArgs e)
        {
            client = null;
            Trace.WriteLine("Client unloaded.");
        }

        private void UpdateCounters(int tileCount, int npcCount, int monsterCount)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<int, int, int>(UpdateCounters), tileCount, npcCount, monsterCount);
                return;
            }

            tileCountLabel.Text = "Tiles: " + tileCount.ToString();
            npcCountLabel.Text = "NPCs: " + npcCount.ToString();
            monsterCountLabel.Text = "Monsters: " + monsterCount.ToString();
        }

        private void LoadItems()
        {
            try
            {
                var otbReader = new OtbReader();
                otItems = otbReader.Open("items.otb", false);
                Trace.WriteLine("Open Tibia items successfully loaded.");
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "Unable to load items.otb. Details: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void ReadTibiaCastFilesCallback(IAsyncResult ar)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<IAsyncResult>(ReadTibiaCastFilesCallback), ar);
                return;
            }

            miniMap.EndUpdate();
        }

        private void loadClientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var chooserOptions = new ClientChooserOptions();
                chooserOptions.Smart = true;
                chooserOptions.ShowOTOption = true;
                chooserOptions.OfflineOnly = true;

                var c = ClientChooser.ShowBox(chooserOptions, this);

                if (c != null)
                {
                    if (c.Version.OtbMajorVersion != otItems.MajorVersion || c.Version.OtbMinorVersion != otItems.MinorVersion)
                        Trace.WriteLine("[Warning] This client requires the version " + c.Version.OtbMajorVersion + "." + c.Version.OtbMinorVersion + " of items.otb.");

                    c.EnableProxy();
                    Client = c;
                    Trace.WriteLine("Client successfully loaded.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Error] Unable to load tibia client. Details: " + ex.Message);
            }
        }

        private void Client_OpenShopWindow(object sender, OpenShopWindowEventArgs e)
        {
            if (!TrackNPCs)
                return;

            var creature = client.BattleList.GetCreature(e.Shop.Name);
            if (creature == null)
                return;

            var key = creature.Name.ToLower().Trim();
            if (!npcs.ContainsKey(key))
                npcs.Add(key, new NpcInfo(creature));

            npcs[key].Shop = e.Shop;
        }

        private void Chat_PlayerSpeak(object sender, PlayerSpeakEventArgs e)
        {
            if (!TrackNPCs)
                return;

            lastPlayerSpeech = e.Text.ToLower();
            lastPlayerSpeechTime = DateTime.Now;
        }


        private void Chat_CreatureSpeak(object sender, CreatureSpeakEventArgs e)
        {
            if (!TrackNPCs)
                return;

            if (e.Creature.Type == CreatureType.NPC)
            {
                var key = e.Creature.Name.ToLower().Trim();
                if (!npcs.ContainsKey(key))
                    npcs.Add(key, new NpcInfo(e.Creature));

                var npcInfo = npcs[key];

                if (e.Type == MessageClasses.NPC_FROM)
                {

                    if (lastPlayerSpeech != null && lastPlayerSpeechTime.AddSeconds(2) > DateTime.Now)
                        npcInfo.AddStatement(lastPlayerSpeech, e.Text);

                    if (NPCAutoTalk && !client.IsClinentless && client.LoggedIn)
                    {
                        CancelSendNextNPCWordSchedule();
                        sendNpcWordScheduleId = client.Scheduler.Add(new Schedule(200, () => { SendNextNPCWord(npcInfo); }));
                    }
                }
                else if (e.Type == MessageClasses.SPEAK_SAY)
                {
                    npcInfo.AddVoice(e.Text, false);
                }
                else if (e.Type == MessageClasses.SPEAK_YELL)
                {
                    npcInfo.AddVoice(e.Text, true);
                }
            }
        }

        private void CancelSendNextNPCWordSchedule()
        {
            if (sendNpcWordScheduleId > 0)
            {
                client.Scheduler.Remove(sendNpcWordScheduleId);
                sendNpcWordScheduleId = 0;
            }
        }

        private void SendNextNPCWord(NpcInfo npcInfo)
        {
            CancelSendNextNPCWordSchedule();
            var word = npcInfo.NotTriedWords.FirstOrDefault();
            if (word == null)
                Trace.WriteLine("No more words to say to " + npcInfo.Name + ".");
            else
            {
                lastPlayerSpeech = word;
                lastPlayerSpeechTime = DateTime.Now;

                client.Chat.SayToNpc(word);
                npcInfo.TriedWords.Add(word);

                sendNpcWordScheduleId = client.Scheduler.Add(new Schedule(2000, () => { SendNextNPCWord(npcInfo); }));
            }
        }

        private void BattleList_CreatureAdded(object sender, CreatureAddedEventArgs e)
        {
            if (!TrackNPCs)
                return;

            if (e.Creature.Type == CreatureType.NPC)
            {
                var key = e.Creature.Name.ToLower().Trim();
                if (!npcs.ContainsKey(key))
                    npcs.Add(key, new NpcInfo(e.Creature));
            }
        }

        private void Map_Updated(object sender, MapUpdatedEventArgs e)
        {
            try
            {
                lock (map)
                {
                    miniMap.BeginUpdate();

                    foreach (var tile in e.Tiles)
                    {
                        if (TrackOnlyCurrentFloor && tile.Location.Z != Client.PlayerLocation.Z)
                            continue;

                        var index = tile.Location.ToIndex();

                        OtMapTile mapTile = map.GetTile(tile.Location);
                        if (mapTile == null)
                            mapTile = new OtMapTile(tile.Location);

                        mapTile.Clear();

                        for (int i = 0; i < tile.ThingCount; i++)
                        {
                            var thing = tile.GetThing(i);

                            if (thing is Creature)
                            {
                                var creature = thing as Creature;

                                if (creature.Type == CreatureType.PLAYER || (!TrackMonsters && creature.Type == CreatureType.MONSTER) || (!TrackNPCs && creature.Type == CreatureType.NPC))
                                    continue;

                                map.AddCreature(new OtCreature { Id = creature.Id, Location = creature.Location, Name = creature.Name, Type = creature.Type });
                            }
                            else if (thing is Item)
                            {
                                var item = tile.GetThing(i) as Item;

                                var info = otItems.GetItemBySpriteId((ushort)item.Id);
                                if (info == null)
                                {
                                    Trace.TraceWarning("Tibia item not in items.otb. Details: item id " + item.Id.ToString());
                                    continue;
                                }

                                if (item.Type.IsMoveable && !TrackMoveableItems)
                                    continue;

                                if (item.IsSplash && !TrackSplashes)
                                    continue;

                                OtMapItem mapItem = new OtMapItem(info);

                                if (mapItem.Info.IsStackable)
                                {
                                    mapItem.AttrType = OtMapItemAttrTypes.COUNT;
                                    mapItem.Extra = item.Count;
                                }

                                if (mapItem.Info.Type == OtbItemType.Splash || mapItem.Info.Type == OtbItemType.FluidContainer
                                    && item.SubType < Constants.ReverseFluidMap.Length)
                                {
                                    mapItem.AttrType = OtMapItemAttrTypes.COUNT;
                                    mapItem.Extra = Constants.ReverseFluidMap[item.SubType];
                                }

                                mapTile.AddItem(mapItem);
                            }
                        }

                        miniMap.SetColor(mapTile.Location, mapTile.MapColor);
                        map.SetTile(mapTile);
                    }

                    miniMap.CenterLocation = Client.PlayerLocation;
                    miniMap.EndUpdate();

                    UpdateCounters(map.TileCount, map.NpcCount, map.MonsterCount);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Error] Unable to convert tibia tile to open tibia tile. Details: " + ex.Message);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Map Files (*.otbm)|*.otbm";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lock (map)
                {
                    try
                    {
                        OtbmWriter.WriteMapToFile(saveFileDialog.FileName, map, client.Version);
                        Trace.WriteLine("Map successfully saved.");
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("[Error] Unable to save map file. Details: " + ex.Message);
                    }
                }
            }
        }

        private void trackTibiaCastFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TibiaCast Files (*.recording)|*.recording";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Tibiacast\\Recordings";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    if (Client == null)
                    {
                        var c = new Client("Tibia.dat", ClientVersion.Current);
                        Client = c;
                    }

                    miniMap.BeginUpdate();

                    var reader = new TibiaCastReader(Client);
                    reader.BeginRead(openFileDialog.FileNames, ReadTibiaCastFilesCallback, null);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception while tracking tibia cast files. Details: " + ex.Message);
                }
            }
        }

        private void saveNPCsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var directory = folderBrowserDialog.SelectedPath;
                    var scriptDirectory = Path.Combine(directory, "scripts");

                    if (!Directory.Exists(scriptDirectory))
                        Directory.CreateDirectory(scriptDirectory);

                    foreach (var npcEntry in npcs)
                    {
                        var npcInfo = npcEntry.Value;

                        var npc = new XElement("npc");
                        npc.Add(new XAttribute("name", npcInfo.Name));
                        npc.Add(new XAttribute("script", "data/npc/scripts/" + npcInfo.Name + ".lua"));
                        npc.Add(new XAttribute("walkinterval", "2000"));
                        npc.Add(new XAttribute("floorchange", "0"));

                        npc.Add(new XElement("health", new XAttribute("now", "100"), new XAttribute("max", "100")));
                        npc.Add(new XElement("look", new XAttribute("type", npcInfo.Outfit.LookType),
                            new XAttribute("head", npcInfo.Outfit.Head), new XAttribute("body", npcInfo.Outfit.Body),
                            new XAttribute("legs", npcInfo.Outfit.Legs), new XAttribute("feet", npcInfo.Outfit.Feet),
                            new XAttribute("addons", npcInfo.Outfit.Addons)));

                        if (npcInfo.Voices.Count > 0)
                        {
                            var voices = new XElement("voices");
                            foreach (var voice in npcInfo.Voices)
                            {
                                voices.Add(new XElement("voice", new XAttribute("text", voice.Text), new XAttribute("interval2", voice.Interval),
                                    new XAttribute("margin", "1"), new XAttribute("yell", voice.IsYell ? "yes" : "no")));
                            }

                            npc.Add(voices);
                        }

                        npc.Save(Path.Combine(directory, npcInfo.Name + ".xml"));

                        var builder = new StringBuilder();

                        builder.Append("local keywordHandler = KeywordHandler:new()\n");
                        builder.Append("local npcHandler = NpcHandler:new(keywordHandler)\n");
                        builder.Append("NpcSystem.parseParameters(npcHandler)\n");
                        builder.Append("\n");
                        builder.Append("function onCreatureAppear(cid)			npcHandler:onCreatureAppear(cid)			end\n");
                        builder.Append("function onCreatureDisappear(cid)		npcHandler:onCreatureDisappear(cid)			end\n");
                        builder.Append("function onCreatureSay(cid, type, msg)	npcHandler:onCreatureSay(cid, type, msg)	end\n");
                        builder.Append("function onThink()						npcHandler:onThink()						end\n");
                        builder.Append("\n");

                        var playerName = client.BattleList.GetPlayer().Name;

                        if (npcInfo.Statements.ContainsKey("hi"))
                        {
                            builder.Append("npcHandler:setMessage(MESSAGE_GREET, '")
                                .Append(npcInfo.Statements["hi"].Replace("'", "\\'").Replace(playerName, "|PLAYERNAME|"))
                                .Append("')\n");
                        }

                        if (npcInfo.Statements.ContainsKey("bye"))
                        {
                            builder.Append("npcHandler:setMessage(MESSAGE_FAREWELL, '")
                                .Append(npcInfo.Statements["bye"].Replace("'", "\\'").Replace(playerName, "|PLAYERNAME|"))
                                .Append("')\n");
                        }

                        if (npcInfo.Statements.ContainsKey("trade"))
                        {
                            builder.Append("npcHandler:setMessage(MESSAGE_SENDTRADE, '")
                                .Append(npcInfo.Statements["trade"].Replace("'", "\\'").Replace(playerName, "|PLAYERNAME|"))
                                .Append("')\n");
                        }

                        builder.Append("\n");

                        foreach (var statement in npcInfo.Statements)
                        {
                            if (statement.Key.Equals("hi") || statement.Key.Equals("bye") || statement.Key.Equals("trade"))
                                continue;

                            builder.Append("keywordHandler:addKeyword({'").Append(statement.Key.Replace("'", "\\'"))
                                .Append("'}, StdModule.say, {npcHandler = npcHandler, onlyFocus = true, text = '")
                                .Append(statement.Value.Replace("'", "\\'").Replace(playerName, "|PLAYERNAME|")).Append("'})\n");
                        }

                        if (npcInfo.Shop != null)
                        {
                            builder.Append("\n");
                            builder.Append("local shopModule = ShopModule:new()\n");
                            builder.Append("npcHandler:addModule(shopModule)\n");
                            builder.Append("\n");

                            foreach (var item in npcInfo.Shop.Items.Where(x => x.IsBuyable))
                            {
                                var otItem = otItems.GetItemBySpriteId(item.Id);

                                if (otItem == null)
                                    continue;

                                builder.Append("shopModule:addBuyableItem({'").Append(item.Name.ToLower()).
                                    Append("'}, ").Append(otItem.Id).Append(", ").Append(item.BuyPrice).
                                    Append(", ");

                                if (otItem.Type == OtbItemType.Splash || otItem.Type == OtbItemType.FluidContainer && item.SubType < Constants.ReverseFluidMap.Length)
                                     builder.Append(Constants.ReverseFluidMap[item.SubType]).Append(", ");

                                builder.Append('\'').Append(item.Name.ToLower()).Append("')\n");
                            }

                            builder.Append("\n");

                            foreach (var item in npcInfo.Shop.Items.Where(x => x.IsSellable))
                            {
                                var otItem = otItems.GetItemBySpriteId(item.Id);

                                if (otItem == null)
                                    continue;

                                builder.Append("shopModule:addSellableItem({'").Append(item.Name.ToLower()).
                                    Append("'}, ").Append(otItem.Id).Append(", ").Append(item.SellPrice).
                                    Append(", ");

                                if (otItem.Type == OtbItemType.Splash || otItem.Type == OtbItemType.FluidContainer && item.SubType < Constants.ReverseFluidMap.Length)
                                    builder.Append(Constants.ReverseFluidMap[item.SubType]).Append(", ");

                                builder.Append('\'').Append(item.Name.ToLower()).Append("')\n");
                            }
                        }


                        builder.Append("\n");
                        builder.Append("npcHandler:addModule(FocusModule:new())");

                        File.WriteAllText(Path.Combine(scriptDirectory, npcInfo.Name + ".lua"), builder.ToString());
                    }

                    Trace.WriteLine("NPCs successfully saved.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception while to save npc files. Details: " + ex.Message);
                }
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (map)
            {
                npcs.Clear();
                map.Clear();
                miniMap.Clear();
                traceTextBox.Text = "";
                tileCountLabel.Text = "Tiles: 0";
                npcCountLabel.Text = "NPCs: 0";
                monsterCountLabel.Text = "Monsters: 0";
            }
        }
    }
}
