using System.Collections.Generic;
using System.Drawing;
using SharpTibiaProxy.Domain;
using SharpTibiaProxy.Util;
using SharpTibiaProxy;

namespace SharpMapTracker.Domain
{
    public class OtTile
    {
        public Location Location { get; private set; }
        public ushort TileId { get; private set; }
        
        public List<OtItem> Items { get; private set; }

        public Color MapColor { get; private set; }

        private int downItemCount;

        public OtTile(Location location)
        {
            Location = location;
            Items = new List<OtItem>();
        }

        public void AddItem(OtItem item)
        {
            if (item.Type.Group == OtItemGroup.Ground)
            {
                TileId = item.Type.Id;
                MapColor = Misc.GetAutomapColor(item.Type.MinimapColor);
            }
            else
            {
                if (item.Type.AlwaysOnTop)
                {
                    bool inserted = false;

                    for (int i = downItemCount; i < Items.Count; i++)
                    {
                        if (Items[i].Type.AlwaysOnTopOrder <= item.Type.AlwaysOnTopOrder)
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
                else if (item.Type.IsMoveable)
                {
                    bool inserted = false;

                    for (int i = downItemCount; i < Items.Count; i++)
                    {
                        if (Items[i].Type.AlwaysOnTopOrder < item.Type.AlwaysOnTopOrder)
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

                Color color = Misc.GetAutomapColor(item.Type.MinimapColor);
                if (color != Color.Black)
                    MapColor = color;
            }
        }

        public void Clear()
        {
            Items.Clear();
            downItemCount = 0;
        }

        public uint HouseId { get; set; }

        public uint Flags { get; set; }
    }
}
