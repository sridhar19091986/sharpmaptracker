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

namespace SharpMapTracker
{
    public partial class MainForm : Form
    {
        private const int MAP_SCROLL_SPEED = 8;

        private Client client;
        private OtItems otItems;

        private OtMap map;
        private Dictionary<Creature, Dictionary<string, string>> npcStatements;
        private string lastPlayerSpeech;

        public bool TrackMoveableItems { get; set; }
        public bool TrackSplashes { get; set; }
        public bool TrackMonsters { get; set; }
        public bool TrackNPCs { get; set; }
        public bool TrackOnlyCurrentFloor { get; set; }

        public MainForm()
        {
            LoadItems();

            InitializeComponent();

            Text = "SharpMapTracker v" + Constants.MAP_TRACKER_VERSION;

            map = new OtMap();
            npcStatements = new Dictionary<Creature, Dictionary<string, string>>();

            DataBindings.Add("TopMost", alwaysOnTopCheckBox, "Checked");
            DataBindings.Add("TrackMoveableItems", trackMoveableItemsCheckBox, "Checked");
            DataBindings.Add("TrackSplashes", trackSplashesCheckBox, "Checked");
            DataBindings.Add("TrackMonsters", trackMonstersCheckBox, "Checked");
            DataBindings.Add("TrackNPCs", trackNpcsCheckBox, "Checked");
            DataBindings.Add("TrackOnlyCurrentFloor", trackOnlyCurrentFloorCheckBox, "Checked");

            Trace.Listeners.Add(new TextBoxTraceListener(traceTextBox));
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));

            Trace.AutoFlush = true;

            KeyDown += new KeyEventHandler(MainForm_KeyDown);
            FormClosed += new FormClosedEventHandler(MainForm_FormClosed);
        }

        void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (client != null)
                client.Dispose();

            client = null;
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

        private void clearButton_Click(object sender, EventArgs e)
        {
            lock (map)
            {
                map.Clear();
                miniMap.Clear();
                traceTextBox.Text = "";
                tileCountLabel.Text = "Tiles: 0";
                npcCountLabel.Text = "NPCs: 0";
                monsterCountLabel.Text = "Monsters: 0";
            }
        }

        private void loadClientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }

                var chooserOptions = new ClientChooserOptions();
                chooserOptions.Version = ClientVersion.Version963.FileVersion;
                chooserOptions.Smart = true;
                chooserOptions.ShowOTOption = true;
                chooserOptions.OfflineOnly = true;

                client = ClientChooser.ShowBox(chooserOptions, this);

                if (client != null)
                {
                    client.EnableProxy();
                    client.Map.Updated += Map_Updated;
                    client.BattleList.CreatureAdded += BattleList_CreatureAdded;
                    client.Chat.CreatureSpeak += Chat_CreatureSpeak;
                    client.Chat.PlayerSpeak += Chat_PlayerSpeak;
                    Trace.WriteLine("Client successfully loaded.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Error] Unable to load tibia client. Details: " + ex.Message);
            }
        }

        void Chat_PlayerSpeak(object sender, PlayerSpeakEventArgs e)
        {
            lastPlayerSpeech = e.Text;
        }

        private void Chat_CreatureSpeak(object sender, CreatureSpeakEventArgs e)
        {
            if (e.Type == MessageClasses.NPC_FROM && e.Creature.Type == CreatureType.NPC)
            {
                if (!npcStatements.ContainsKey(e.Creature))
                    npcStatements.Add(e.Creature, new Dictionary<string, string>());

                if (lastPlayerSpeech != null)
                    npcStatements[e.Creature][lastPlayerSpeech] = e.Text;
            }
        }

        private void BattleList_CreatureAdded(object sender, CreatureAddedEventArgs e)
        {
            if (e.Creature.Type == CreatureType.NPC)
            {
                if (!npcStatements.ContainsKey(e.Creature))
                    npcStatements[e.Creature] = new Dictionary<string, string>();
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
                        if (TrackOnlyCurrentFloor && tile.Location.Z != client.PlayerLocation.Z)
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

                                if (creature.Type == CreatureType.PLAYER || (!TrackMonsters && creature.Type == CreatureType.MONSTER) ||
                                    (!TrackNPCs && creature.Type == CreatureType.NPC) || mapTile.Creature != null)
                                {
                                    continue;
                                }

                                mapTile.Creature = creature;
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

                    miniMap.CenterLocation = client.PlayerLocation;
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
                        OtbmWriter.WriteMapToFile(saveFileDialog.FileName, map, Constants.GetMapVersion(963));
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
                    if (client == null)
                    {
                        client = new Client("Tibia.dat", ClientVersion.Version963);
                        client.Map.Updated += Map_Updated;
                        client.BattleList.CreatureAdded += BattleList_CreatureAdded;
                    }

                    miniMap.BeginUpdate();

                    var reader = new TibiaCastReader(client);
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
                    foreach (var npcStatement in npcStatements)
                    {
                        var cr = npcStatement.Key;

                        var builder = new StringBuilder();
                        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
                        builder.Append("\t<npc name=\"").Append(cr.Name).Append("\" script=\"data/npc/scripts/").Append(cr.Name).Append(".lua\" walkinterval=\"2000\" floorchange=\"0\">\n");
                        builder.Append("\t<health now=\"100\" max=\"100\"/>\n");

                        if (cr.Outfit.LookItem > 0)
                        {

                        }
                        else
                        {
                            builder.Append("\t<look type=\"").Append(cr.Outfit.LookType).
                                Append("\" head=\"").Append(cr.Outfit.Head).
                                Append("\" body=\"").Append(cr.Outfit.Body).
                                Append("\" legs=\"").Append(cr.Outfit.Legs).
                                Append("\" feet=\"").Append(cr.Outfit.Feet).
                                Append("\" addons=\"").Append(cr.Outfit.Addons).Append("\"/>\n");
                        }

                        builder.Append("</npc>");

                        File.WriteAllText(Path.Combine(directory, cr.Name + ".xml"), builder.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception while to save npc files. Details: " + ex.Message);
                }
            }
        }
    }
}
