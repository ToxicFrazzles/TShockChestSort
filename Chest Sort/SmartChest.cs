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
            if (chest.name == "") return false;
            string ln = chest.name.ToLower();
            if (ln == "banners" || ln == "melee" || ln == "money" || ln == "ranged" || ln == "summon" || ln == "magic" || ln == "accessories" || ln == "bait") return true;
            return false;
        }

        public static bool ShouldStoreItem(this Chest chest, Item item) {
            if (chest.name == "") return true;      // Unnamed chests can have any item
            if (chest.name.ToLower() == "banners" && !item.Name.EndsWith("Banner")) return false;
            else if (chest.name.ToLower() == "melee" && !item.melee) return false;
            else if (chest.name.ToLower() == "money" && !item.IsCurrency) return false;
            else if (chest.name.ToLower() == "ranged" && !item.ranged) return false;
            else if (chest.name.ToLower() == "summon" && !item.summon) return false;
            else if (chest.name.ToLower() == "magic" && !item.magic) return false;
            else if (chest.name.ToLower() == "accessories" && !item.accessory) return false;
            else if (chest.name.ToLower() == "bait" && item.bait <= 0) return false;
            return true;
        }

        public static List<SmartItem> ItemsToMove(this Chest chest)
        {
            List<SmartItem> list = new List<SmartItem>();
            if (chest.name == "") return list;
            for(int i=0; i<chest.item.Length; ++i)
            {
                SmartItem item = new SmartItem(chest, i);
                if (chest.name.ToLower() == "banners" && !item.Item.Name.EndsWith("Banner")) list.Add(item);
                else if (chest.name.ToLower() == "melee" && !item.Item.melee) list.Add(item);
                else if (chest.name.ToLower() == "money" && !item.Item.IsCurrency) list.Add(item);
                else if (chest.name.ToLower() == "ranged" && !item.Item.ranged) list.Add(item);
                else if (chest.name.ToLower() == "summon" && !item.Item.summon) list.Add(item);
                else if (chest.name.ToLower() == "magic" && !item.Item.magic) list.Add(item);
                else if (chest.name.ToLower() == "accessories" && !item.Item.accessory) list.Add(item);
                else if (chest.name.ToLower() == "bait" && item.Item.bait <= 0) list.Add(item);
            }
            return list;
        }
    }
}
