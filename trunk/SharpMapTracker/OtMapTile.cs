using System.Collections.Generic;
using System.Drawing;
using SharpTibiaProxy.Domain;
using SharpTibiaProxy.Util;
using SharpTibiaProxy;

namespace SharpMapTracker
{
    public class OtMapTile
    {
        public Location Location { get; private set; }
        public ushort TileId { get; private set; }
        
        public List<OtMapItem> Items { get; private set; }
        public Creature Creature { get; set; }

        public Color MapColor { get; private set; }

        private int downItemCount;

        public OtMapTile(Location location)
        {
            Location = location;
            Items = new List<OtMapItem>();
        }

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

                    for (int i = downItemCount; i < Items.Count; i++)
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
                else if (item.Info.IsMoveable || item.Info.Type == OtbItemType.Splash)
                {
                    bool inserted = false;

                    for (int i = downItemCount; i < Items.Count; i++)
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
                    ++downItemCount;
                }

                Color color = Misc.GetAutomapColor(item.Info.MinimapColor);
                if (color != Color.Black)
                    MapColor = color;
            }
        }

        public void Clear()
        {
            Items.Clear();
            downItemCount = 0;
        }
    }
}
