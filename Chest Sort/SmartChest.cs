using Chest_Sort;
using System.Collections.Generic;
using Terraria;

namespace ChestSort
{
     static class SmartChest
    {
        // Find the next available empty slot in the chest
        public static int NextAvailableSlot(this Chest chest)
        {
            for (int i=0; i<chest.item.Length; ++i)
            {
                if(chest.item[i].type == 0 || chest.item[i].stack == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        // Move the provided item in a slot of one chest 
        public static bool MoveItemTo(this Chest source, int slot, Chest dest)
        {
            Item item1 = source.item[slot];
            int destSlot = dest.NextAvailableSlot();
            if (destSlot < 0) return false;
            Item item2 = dest.item[destSlot];
            if (item2.type != 0) return false;
            dest.item[destSlot] = item1;
            source.item[slot] = item2;
            return true;
        }


        public static bool HasSortRules(this Chest chest)
        {
            if (chest.name == "") return false;  // Unnamed chests can have any item
            List<Categorisation> categories = Config.categorisations();
            foreach (Categorisation category in categories)
            {
                if(category.AppliesToChest(chest)) return true;
            }
            return false;
        }

        public static bool ShouldStoreItem(this Chest chest, Item item) {
            if(!HasSortRules(chest)) return true;       // Chests without rules can accept any items
            List<Categorisation> categories = Config.categorisations();
            foreach (Categorisation category in categories)
            {
                if (category.AppliesToChest(chest) && category.ItemMatches(item))
                {
                    return true;
                }
            }
            return false;
        }

        /*public static List<SmartItem> ItemsToMove(this Chest chest)
        {
            List<SmartItem> list = new List<SmartItem>();
            if (chest.name == "") return list;
            for(int i=0; i<chest.item.Length; ++i)
            {
                SmartItem item = new SmartItem(chest, i);
                List<Categorisation> categories = Config.categorisations();
                foreach (Categorisation category in categories)
                {
                    if (category.AppliesToChest(chest) && category.ItemMatches(item.Item))
                    {
                        list.Add(item);
                        break;
                    }
                }
            }
            return list;
        }*/
    }
}
