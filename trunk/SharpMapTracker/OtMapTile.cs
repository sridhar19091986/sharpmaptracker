using System.Collections.Generic;
using System.Drawing;
using SharpTibiaProxy.Domain;
using SharpTibiaProxy;

namespace SharpMapTracker
{
    public class OtMapTile
    {
        public Location Location;
        public ushort TileId;
        public List<OtMapItem> Items = new List<OtMapItem>();
        public int DownItemCount { get; private set; }
        public Color MapColor;

        public void AddItem(OtMapItem item)
        {
            if (item.Info.Type == OtbItemType.Ground)
            {
                TileId = item.Info.Id;
                MapColor = Misc.GetAutomapColor(item.Info.MinimapColor);
            }
            else
            {
                if (item.Info.AlwaysOnTop)
                {
                    bool inserted = false;

                    for (int i = DownItemCount; i < Items.Count; i++)
                    {
                        if (Items[i].Info.AlwaysOnTopOrder <= item.Info.AlwaysOnTopOrder)
                        {
                            Items.Insert(i, item);
                            inserted = true;
                            break;
                        }
                    }

                    if (!inserted)
                    {
                        Items.Add(item);
                    }
                }
                else if (item.Info.IsMoveable)
                {
                    bool inserted = false;

                    for (int i = DownItemCount; i < Items.Count; i++)
                    {
                        if (Items[i].Info.AlwaysOnTopOrder < item.Info.AlwaysOnTopOrder)
                        {
                            Items.Insert(i, item);
                            inserted = true;
                            break;
                        }
                    }

                    if (!inserted)
                    {
                        Items.Add(item);
                    }
                }
                else
                {
                    Items.Insert(0, item);
                    ++DownItemCount;
                }

                Color color = Misc.GetAutomapColor(item.Info.MinimapColor);
                if (color != Color.Black)
                    MapColor = color;
            }
        }

        public void Clear()
        {
            Items.Clear();
            DownItemCount = 0;
        }
    }
}
