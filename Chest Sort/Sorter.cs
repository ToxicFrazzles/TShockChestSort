using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace ChestSort
{
    internal class Sorter
    {
        public Region Region { get; private set; }
        public Sorter(ChestSortPlugin plugin, Region region)
        {
            Region = region;
            plugin.ChestClose += ChestCloseHandler;
        }

        List<Chest> Chests
        {
            get
            {
                List<Chest> list = new List<Chest>();
                for(int i=0; i<Main.chest.Length; i++)
                {
                    Chest chest = Main.chest[i];
                    if(chest == null) continue;
                    if (Region.InArea(chest.x, chest.y))
                    {
                        list.Add(chest);
                    }
                }
                return list;
            }
        }

        public bool sorting { get; private set; }

        public bool handlesChest(int chestID)
        {
            if (chestID < 0) return false;      // A -1 indicates no chest and -2 and below indicate personal chests e.g. piggy bank, safe, defenders forge etc.
            Chest chest = Main.chest[chestID];
            if (chest == null) return false;
            return Region.InArea(chest.x, chest.y);
        }

        public bool handlesChest(int x, int y)
        {
            return Region.InArea(x, y);
        }

        public async Task sort()
        {
            sorting = true;
            foreach(TSPlayer player in TShock.Players)
            {
                if(player == null) continue;
                if (!handlesChest(player.ActiveChest)) continue;
                player.SendInfoMessage("The chest you were in is being sorted.");
                player.SendData(PacketTypes.ChestOpen, "", -1, 0, 0, 0);
            }
            await Task.Factory.StartNew(innerSort);
            sorting = false;
        }

        private async void ChestCloseHandler(object sender, ChestCloseEventArgs args)
        {
            foreach(TSPlayer player in TShock.Players)
            {
                if (player == null || player == args.Player) continue;
                if(handlesChest(player.ActiveChest)) return;
            }
            sorting = true;
            args.Player.SendWarningMessage("Sorting the chest you were just in :)");
            await Task.Factory.StartNew(innerSort);
            sorting = false;
        }

        private void innerSort()
        {
            List<Item> items = new List<Item>();
            foreach(Chest chest in Chests)
            {
                for(int i=0; i<chest.item.Length; ++i)
                {
                    Item item = chest.item[i];
                    if (item == null || item.type == 0 || item.stack == 0) continue;
                    if (Config.TrashItems.Contains(item.type)) { 
                        chest.item[i].type = 0;
                        chest.item[i].stack = 0;
                        chest.item[i].netID = 0;
                        continue;
                    }
                    items.Add(chest.item[i]);
                }
            }

            // Attempt to put the smaller stacks of items onto the larger stacks
            items.Sort((y, x) =>  x.stack.CompareTo(y.stack));
            for(int i=0; i<items.Count -1; ++i)
            {
                Item item1 = items[i];
                for (int j = items.Count - 1; j > i; --j)
                {
                    Item item2 = items[j];
                    if (item2.type != item1.type) continue;

                    if (item1.stack + item2.stack <= item1.maxStack)
                    {
                        item1.stack += item2.stack;
                        item2.stack = 0;
                        item2.type = 0;
                        item2.netID = 0;
                    }
                    else
                    {
                        item2.stack -= item1.maxStack - item1.stack;
                        item1.stack = item1.maxStack;
                        break;
                    }
                }
            }


        }
    }
}
