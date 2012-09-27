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
        private Otb otb;

        private Dictionary<ulong, OtMapTile> tiles;
        private Dictionary<uint, OtMapCreature> creatures;
        private HashSet<ulong> creatureLocations;

        public bool TrackMoveableItems { get; set; }
        public bool TrackSplashItems { get; set; }
        public bool TrackCreatures { get; set; }
        public bool TrackOnlyCurrentFloor { get; set; }

        public MainForm()
        {
            InitializeComponent();

            Text = "SharpMapTracker v" + Constants.MAP_TRACKER_VERSION;

            tiles = new Dictionary<ulong, OtMapTile>();
            creatures = new Dictionary<uint, OtMapCreature>();
            creatureLocations = new HashSet<ulong>();

            DataBindings.Add("TopMost", topMostCheckBox, "Checked");
            DataBindings.Add("TrackMoveableItems", trackMoveableItemCheckBox, "Checked");
            DataBindings.Add("TrackSplashItems", trackSplashItemsCheckBox, "Checked");
            DataBindings.Add("TrackCreatures", trackCreaturesCheckBox, "Checked");
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadItems();
        }

        private void Map_Updated(object sender, MapUpdatedEventArgs e)
        {
            try
            {
                lock (tiles)
                {
                    miniMap.BeginUpdate();

                    foreach (var tile in e.Tiles)
                    {
                        if (TrackOnlyCurrentFloor && tile.Location.Z != client.PlayerLocation.Z)
                            continue;

                        var index = tile.Location.ToIndex();

                        OtMapTile mapTile = null;
                        tiles.TryGetValue(index, out mapTile);
                        if (mapTile == null)
                            mapTile = new OtMapTile();

                        mapTile.Location = tile.Location;
                        mapTile.Clear();

                        for (int i = 0; i < tile.ThingCount; i++)
                        {
                            var thing = tile.GetThing(i);

                            if (thing is Creature)
                            {
                                if (!TrackCreatures)
                                    continue;

                                var creature = thing as Creature;

                                if (creature.Type != CreatureType.MONSTER || creatures.ContainsKey(creature.Id))
                                    continue;

                                if (creatureLocations.Contains(index))
                                    continue;

                                creatureLocations.Add(index);
                                creatures.Add(creature.Id, new OtMapCreature { Id = creature.Id, Name = creature.Name, Location = creature.Location });
                            }
                            else if (thing is Item)
                            {
                                var item = tile.GetThing(i) as Item;

                                var info = otb.GetItemBySpriteId((ushort)item.Id);
                                if (info == null)
                                {
                                    Trace.TraceWarning("Tibia item not in items.otb. Details: item id " + item.Id.ToString());
                                    continue;
                                }

                                if (item.Type.IsMoveable && !TrackMoveableItems)
                                    continue;

                                if (item.IsSplash && !TrackSplashItems)
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
                        tiles[index] = mapTile;
                    }

                    miniMap.CenterLocation = client.PlayerLocation;
                    miniMap.EndUpdate();

                    UpdateCounters(tiles.Count, creatures.Count);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Error] Unable to convert tibia tile to open tibia tile. Details: " + ex.Message);
            }
        }

        private void UpdateCounters(int tileCount, int creatureCount)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<int, int>(UpdateCounters), tileCount, creatureCount);
                return;
            }

            tileCountTextBox.Text = tileCount.ToString();
            creatureCountTextBox.Text = creatureCount.ToString();
        }

        private void LoadItems()
        {
            try
            {
                var otbReader = new OtbReader();
                otb = otbReader.Open("items.otb", false);
                Trace.WriteLine("Open Tibia items successfully loaded.");
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "Unable to load items.otb. Details: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void saveMapButton_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Map Files (*.otbm)|*.otbm";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lock (tiles)
                {
                    try
                    {
                        OtbmWriter.WriteMapTilesToFile(saveFileDialog.FileName, tiles.Values, creatures.Values, Constants.GetMapVersion(963));
                        Trace.WriteLine("Map successfully saved.");
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("[Error] Unable to save map file. Details: " + ex.Message);
                    }

                }
            }
        }

        private void trackcamButton_Click(object sender, EventArgs e)
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
                    }

                    trackcamButton.Enabled = false;
                    clearButton.Enabled = false;
                    saveMapButton.Enabled = false;
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

        private void ReadTibiaCastFilesCallback(IAsyncResult ar)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<IAsyncResult>(ReadTibiaCastFilesCallback), ar);
                return;
            }

            trackcamButton.Enabled = true;
            clearButton.Enabled = true;
            saveMapButton.Enabled = true;
            miniMap.EndUpdate();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            lock (tiles)
            {
                tiles.Clear();
                creatures.Clear();
                miniMap.Clear();
                traceTextBox.Text = "";
                tileCountTextBox.Text = "0";
                creatureCountTextBox.Text = "0";
            }
        }

        private void loadClientButton_Click(object sender, EventArgs e)
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

                client = ClientChooser.ShowBox(chooserOptions);

                if (client != null)
                {
                    client.EnableProxy();
                    client.Map.Updated += Map_Updated;
                    Trace.WriteLine("Client successfully loaded.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Error] Unable to load tibia client. Details: " + ex.Message);
            }
        }
    }
}
