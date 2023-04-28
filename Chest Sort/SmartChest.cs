﻿using Terraria;

namespace ChestSort
{
     static class SmartChest
    {
        public static int NextAvailableSlot(this Chest chest)
        {
            for(int i=0; i<chest.item.Length; ++i)
            {
                if(chest.item[i].type == 0 || chest.item[i].stack == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void MoveItemTo(this Chest source, int slot, Chest dest)
        {
            Item item1 = source.item[slot];
            int destSlot = dest.NextAvailableSlot();
            if (destSlot < 0) return;
            Item item2 = dest.item[destSlot];
            if (item2.type != 0) return;
            dest.item[destSlot] = item1;
            source.item[slot] = item2;
        }

        public static List<int> ItemsToMove(this Chest chest)
        {
            List<int> list = new List<int>();
            if (chest.name == "") return list;
            for(int i=0; i<chest.item.Length; ++i)
            {
                Item item = chest.item[i];
                if (chest.name.ToLower() == "banners" && !item.Name.EndsWith("Banner")) list.Add(i);
                else if (chest.name.ToLower() == "melee" && !item.melee) list.Add(i);
                else if (chest.name.ToLower() == "money" && !item.IsCurrency) list.Add(i);
                else if (chest.name.ToLower() == "ranged" && !item.ranged) list.Add(i);
                else if (chest.name.ToLower() == "summon" && !item.summon) list.Add(i);
                else if (chest.name.ToLower() == "magic" && !item.magic) list.Add(i);
                else if (chest.name.ToLower() == "accessories" && !item.accessory) list.Add(i);
                else if (chest.name.ToLower() == "bait" && item.bait <= 0) list.Add(i);
            }
            return list;
        }
    }
}