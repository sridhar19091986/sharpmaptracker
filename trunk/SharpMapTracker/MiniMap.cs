using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpTibiaProxy.Domain;
using SharpMapTracker.Domain;

namespace SharpMapTracker
{
    public partial class MiniMap : UserControl
    {
        private const int PIXEL_FACTOR = 2;
        private int miniMapSize = 192;

        private int updateOngoing;
        private Location centerLocation;
        private bool highlightMissingTiles;

        public bool HighlightMissingTiles
        {
            get { return highlightMissingTiles; }
            set
            {
                highlightMissingTiles = value;
                Invalidate();
            }
        }

        public OtMap Map { get; set; }

        public Location CenterLocation
        {
            get { return centerLocation; }
            set
            {
                centerLocation = value;
                Invalidate();
            }
        }

        public int Floor
        {
            get { return centerLocation != null ? centerLocation.Z : 0; }
            set
            {
                if (centerLocation == null)
                    return;
                if (value > 15)
                    value = 15;
                else if (value < 0)
                    value = 0;

                centerLocation = new Location(centerLocation.X, centerLocation.Y, value);

                Invalidate();
            }
        }

        public MiniMap()
        {
            InitializeComponent();
            //colors = new Dictionary<ulong, Color>();

            MouseMove += new MouseEventHandler(MiniMap_MouseMove);
            MouseClick += new MouseEventHandler(MiniMap_MouseClick);
        }

        void MiniMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (CenterLocation == null)
                return;

            var pos = LocalToGlobal(e.X, e.Y);

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                CenterLocation = new SharpTibiaProxy.Domain.Location(pos.X, pos.Y, CenterLocation.Z);
                Invalidate();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Clipboard.SetText(pos.ToString());
            }
        }

        void MiniMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (CenterLocation == null)
                return;

            var pos = LocalToGlobal(e.X, e.Y);
            coorLabel.Text = String.Format("[{0}, {1}, {2}]", pos.X, pos.Y, CenterLocation.Z);
        }

        protected Point LocalToGlobal(int x, int y)
        {
            int xoffset = centerLocation.X - miniMapSize / 2;
            int yoffset = centerLocation.Y - miniMapSize / 2;

            return new Point(((x * miniMapSize) / Width) + xoffset, ((y * miniMapSize) / Height) + yoffset);
        }

        public void BeginUpdate()
        {
            updateOngoing = updateOngoing + 1;
        }

        public void EndUpdate()
        {
            if (updateOngoing > 0)
            {
                updateOngoing = updateOngoing - 1;
                if (updateOngoing == 0)
                    Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (CenterLocation != null && Map != null && updateOngoing == 0)
            {
                Bitmap bitmap = new Bitmap(miniMapSize * PIXEL_FACTOR, miniMapSize * PIXEL_FACTOR);
                FastBitmap processor = new FastBitmap(bitmap);

                processor.LockImage();

                int xoffset = CenterLocation.X - miniMapSize / 2;
                int yoffset = CenterLocation.Y - miniMapSize / 2;

                for (int x = 0; x < miniMapSize; x++)
                {
                    for (int y = 0; y < miniMapSize; y++)
                    {

                        var color = Color.Black;

                        var tile = Map.GetTile(SharpTibiaProxy.Domain.Location.ToIndex(x + xoffset, y + yoffset, CenterLocation.Z));
                        if (tile != null)
                            color = tile.MapColor;
                        else if (highlightMissingTiles)
                            color = Color.Fuchsia;

                        for (int px = 0; px < PIXEL_FACTOR; px++)
                        {
                            for (int py = 0; py < PIXEL_FACTOR; py++)
                            {
                                processor.SetPixel(x * PIXEL_FACTOR + px, y * PIXEL_FACTOR + py, color);
                            }
                        }
                    }
                }

                processor.UnlockImage();

                e.Graphics.DrawImage(bitmap, 0, 0, Width, Height);

            }
            else
                base.OnPaint(e);
        }
    }
}
