using System.Collections.Generic;

namespace SharpTibiaProxy.Domain
{
    public class ItemList : List<Item>
    {
        public int DownItemCount { get; set; }

        public int GetBeginDownItem()
        {
            return 0;
        }

        public int GetEndDownItem()
        {
            return DownItemCount;
        }

        public int GetBeginTopItem()
        {
            return DownItemCount;
        }

        public int GetEndTopItem()
        {
            return Count;
        }

        public int GetTopItemCount()
        {
            return Count - DownItemCount;
        }

        public int GetDownItemCount()
        {
            return DownItemCount;
        }

        public int Find(int start, int end, Item item)
        {
            for (int i = start; i < end; i++)
            {
                if (this[i] == item)
                    return i;
            }

            return -1;
        }
    }
}