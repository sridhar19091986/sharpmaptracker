using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpTibiaProxy.Domain;

namespace SharpMapTracker
{
    public partial class MiniMap : UserControl
    {
        private const int PIXEL_FACTOR = 2;
        private int miniMapSize = 192;
        private Dictionary<ulong, Color> colors;
        private int updateOngoing;
        private Location currentLocation;

        public MiniMap()
        {
            InitializeComponent();
            colors = new Dictionary<ulong, Color>();

            MouseMove += new MouseEventHandler(MiniMap_MouseMove);
            MouseClick += new MouseEventHandler(MiniMap_MouseClick);
            KeyDown += new KeyEventHandler(MiniMap_KeyDown);
        }

        void MiniMap_KeyDown(object sender, KeyEventArgs e)
        {
            var newZ = currentLocation.Z;
            if (e.KeyCode == Keys.PageDown)
                newZ++;
            else if (e.KeyCode == Keys.PageUp)
                newZ--;
            if (newZ > 15)
                newZ = 15;
            else if (newZ < 0)
                newZ = 0;

            currentLocation = new Location(currentLocation.X, currentLocation.Y, newZ);

            if (updateOngoing == 0)
                Invalidate();
        }

        void MiniMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (currentLocation == null)
                return;

            var pos = LocalToGlobal(e.X, e.Y);
            currentLocation = new Location(pos.X, pos.Y, currentLocation.Z);

            if (updateOngoing == 0)
                Invalidate();
        }

        void MiniMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentLocation == null)
                return;

            var pos = LocalToGlobal(e.X, e.Y);
            coorLabel.Text = String.Format("[{0}, {1}, {2}]", pos.X, pos.Y, currentLocation.Z);
        }

        protected Point LocalToGlobal(int x, int y)
        {
            int xoffset = currentLocation.X - miniMapSize / 2;
            int yoffset = currentLocation.Y - miniMapSize / 2;

            return new Point(((x * miniMapSize) / Width) + xoffset, ((y * miniMapSize) / Height) + yoffset);
        }

        public void BeginUpdate()
        {
            updateOngoing++;
        }

        public void EndUpdate()
        {
            updateOngoing--;
            if (updateOngoing == 0)
                Invalidate();
        }

        public void SetColor(Location location, Color color)
        {
            colors[location.ToIndex()] = color;

            if (updateOngoing == 0)
                Invalidate();
        }

        public void SetLocation(Location location)
        {
            currentLocation = location;
            if (updateOngoing == 0)
                Invalidate();
        }

        public void Clear()
        {
            colors.Clear();
            if (updateOngoing == 0)
                Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (currentLocation != null)
            {
                Bitmap bitmap = new Bitmap(miniMapSize * PIXEL_FACTOR, miniMapSize * PIXEL_FACTOR);
                FastBitmap processor = new FastBitmap(bitmap);

                processor.LockImage();

                int xoffset = currentLocation.X - miniMapSize / 2;
                int yoffset = currentLocation.Y - miniMapSize / 2;

                for (int x = 0; x < miniMapSize; x++)
                {
                    for (int y = 0; y < miniMapSize; y++)
                    {
                        var color = Color.Black;

                        var index = SharpTibiaProxy.Domain.Location.ToIndex(x + xoffset, y + yoffset, currentLocation.Z);
                        if (colors.ContainsKey(index))
                            color = colors[index];

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

        private void MiniMap_Load(object sender, EventArgs e)
        {
        }
    }
}
