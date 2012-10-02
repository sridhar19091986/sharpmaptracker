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
        private Location mapLocation;

        public Location CenterLocation
        {
            get { return mapLocation; }
            set
            {
                mapLocation = value;
                Invalidate();
            }
        }

        public int Floor
        {
            get { return mapLocation != null ? mapLocation.Z : 0; }
            set
            {
                if (mapLocation == null)
                    return;
                if (value > 15)
                    value = 15;
                else if (value < 0)
                    value = 0;

                mapLocation = new Location(mapLocation.X, mapLocation.Y, value);

                Invalidate();
            }
        }

        public MiniMap()
        {
            InitializeComponent();
            colors = new Dictionary<ulong, Color>();

            MouseMove += new MouseEventHandler(MiniMap_MouseMove);
            MouseClick += new MouseEventHandler(MiniMap_MouseClick);
        }

        void MiniMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (CenterLocation == null)
                return;

            var pos = LocalToGlobal(e.X, e.Y);
            CenterLocation = new SharpTibiaProxy.Domain.Location(pos.X, pos.Y, CenterLocation.Z);

            Invalidate();
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
            int xoffset = mapLocation.X - miniMapSize / 2;
            int yoffset = mapLocation.Y - miniMapSize / 2;

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

        public void SetColor(Location location, Color color)
        {
            colors[location.ToIndex()] = color;

            Invalidate();
        }

        public void Clear()
        {
            colors.Clear();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (CenterLocation != null && updateOngoing == 0)
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

                        var index = SharpTibiaProxy.Domain.Location.ToIndex(x + xoffset, y + yoffset, CenterLocation.Z);
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
    }
}
