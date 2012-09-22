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
using SharpTibiaProxy.Memory;
using SharpTibiaProxy;
using SharpTibiaProxy.Network;
using System.Threading;

namespace SharpMapTracker
{
    public partial class MainForm : Form
    {
        private Client client;
        private Otb otb;

        private Dictionary<Location, OtMapTile> tiles;
        private Dictionary<uint, OtMapCreature> creatures;

        public bool TrackMoveableItems { get; set; }
        public bool TrackSplashItems { get; set; }
        public bool TrackCreatures { get; set; }

        public MainForm()
        {
            InitializeComponent();

            Text = "SharpMapTracker v" + Constants.MAP_TRACKER_VERSION;

            tiles = new Dictionary<Location, OtMapTile>();
            creatures = new Dictionary<uint, OtMapCreature>();

            DataBindings.Add("TopMost", topMostCheckBox, "Checked");
            DataBindings.Add("TrackMoveableItems", trackMoveableItemCheckBox, "Checked");
            DataBindings.Add("TrackSplashItems", trackSplashItemsCheckBox, "Checked");
            DataBindings.Add("TrackCreatures", trackCreaturesCheckBox, "Checked");

            Trace.Listeners.Add(new TextBoxTraceListener(traceTextBox));
            Trace.AutoFlush = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadItems();
            LoadClient();
        }

        private void LoadClient()
        {
            if (client != null)
                return;

            try
            {
                foreach (Process process in Process.GetProcesses())
                {
                    StringBuilder classname = new StringBuilder();
                    WinApi.GetClassName(process.MainWindowHandle, classname, 12);

                    if (classname.ToString().Equals("TibiaClient", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (process.MainModule.FileVersionInfo.FileVersion == "9.6.3.0")
                            client = new Client(process);
                    }
                }

                if (client != null)
                {
                    client.EnableProxy();
                    client.Map.Updated += Map_Updated;
                    Trace.WriteLine("Client successfully loaded.");
                }
                else
                    Trace.WriteLine("[Error] Tibia client not found.");
            }
            catch (Exception e)
            {
                Trace.WriteLine("[Error] Unable to locate tibia client. Details: " + e.Message);
            }
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
                        OtMapTile mapTile = null;
                        tiles.TryGetValue(tile.Location, out mapTile);
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
                        tiles[mapTile.Location] = mapTile;
                    }

                    miniMap.SetLocation(client.PlayerLocation);
                    miniMap.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[Error] Unable to convert tibia tile to open tibia tile. Details: " + ex.Message);
            }
        }

        private void LoadItems()
        {
            if (File.Exists("items.otb"))
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
            else
            {
                MessageBox.Show(this, "File items.otb not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        client = new Client("Tibia.dat");
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
            }
        }
    }
}
