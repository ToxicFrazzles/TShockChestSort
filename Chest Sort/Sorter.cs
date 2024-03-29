﻿using Chest_Sort;
using NuGet.Protocol.Plugins;
using System.Diagnostics;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace ChestSort
{
    internal class Sorter
    {
        public Region Region { get; private set; }
        private ChestSortPlugin Plugin;
        public Sorter(ChestSortPlugin plugin, Region region)
        {
            Region = region;
            Plugin = plugin;
            Plugin.ChestClose += ChestCloseHandler;
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

        public void Delete()
        {
            // Unregister the event handler
            Plugin.ChestClose -= ChestCloseHandler;
            
        }

        public bool sorting { get; private set; }
        public bool paused { get; set; }

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

        public async Task sortCmd()
        {
            if(paused) return;
            sorting = true;
            foreach(TSPlayer player in TShock.Players)
            {
                if(player == null) continue;
                if (!handlesChest(player.ActiveChest)) continue;
                player.SendInfoMessage("The chest you were in is being sorted.");
                player.SendData(PacketTypes.ChestOpen, "", -1, 0, 0, 0);
            }
            await Task.Factory.StartNew(sort);
            sorting = false;
        }

        private async void ChestCloseHandler(object sender, ChestCloseEventArgs args)
        {
            if (paused) return;
            if(!handlesChest(args.ChestID)) return;
            foreach(TSPlayer player in TShock.Players)
            {
                if (player == null || player == args.Player) continue;
                if(handlesChest(player.ActiveChest)) return;
            }
            sorting = true;
            args.Player.SendDebugMessage("Sorting the chest you were just in :)");
            await Task.Factory.StartNew(sort);
            sorting = false;
        }

        private static int AllocatedChestFreeSlots(List<SmartItem> allocatedItems, Chest chest)
        {
            int result = chest.item.Length;
            foreach(SmartItem item in allocatedItems)
            {
                if (item.NewChest == chest) result -= 1;
            }
            //Log.Debug("Chest \"{0}\" has {1} free slots", chest, result);
            return result;
        }

        private List<Categorisation> ApplicableCategories { get
            {
                List<Categorisation> result = new List<Categorisation>();
                foreach(Categorisation cat in Config.Categories)
                {
                    foreach(Chest chest in Chests)
                    {
                        if (cat.AppliesToChest(chest))
                        {
                            result.Add(cat);
                            break;
                        }
                    }
                }
                return result;
            } }

        private void sort()
        {
            SellExcess();
            ConsolidateItemStacks();
            ConsolidateItems();
            MainSort();
        }

        private void SellExcess()
        {
            List<SmartItem> items = new List<SmartItem>();
            foreach (Chest chest in Chests)
            {
                for (int i = 0; i < chest.item.Length; ++i)
                {
                    if (chest.item[i].IsAir) continue;
                    items.Add(new SmartItem(chest, i));
                }
            }

            List<ItemCounter> itemCounters = new List<ItemCounter>();
            while(items.Count > 0)
            {
                List<SmartItem> list = new List<SmartItem>();
                SmartItem mItem = items.First();
                foreach(SmartItem item in items)
                {
                    if(item.Item.type == mItem.Item.type && item.Item.prefix == mItem.Item.prefix)
                    {
                        list.Add(item);
                    }
                }
                items.RemoveAll(x => list.Contains(x));
                itemCounters.Add(new ItemCounter(list));
            }

            foreach(Categorisation cat in ApplicableCategories)
            {
                // If the items in this category cannot be sold, continue
                if(cat.CanSell == null || !cat.CanSell.Value) continue;
                if(cat.SellThreshold == null || cat.SellThreshold.Value <= 0) continue;


                List<ItemCounter> done = new List<ItemCounter>();
                foreach(ItemCounter counter in itemCounters) {
                    if (!cat.ItemMatches(counter.SmartItems.First().Item)) { 
                        done.Add(counter);
                        continue; 
                    }
                    if (cat.SellThreshold <= counter.TotalStacks)
                    {
                        done.Add(counter);
                        continue;
                    }
                    // TODO: Find space to put coins, delete excess, add coins to chest.
                }
                itemCounters.RemoveAll(x => done.Contains(x));
            }
        }

        /// <summary>
        /// Sorts items into the chests which they belong in
        /// </summary>
        private void MainSort()
        {
            Log.Debug("Sorting {0} chests!", Chests.Count);


            List<SmartItem> remainingItems = new List<SmartItem>();
            List<SmartItem> allocatedItems = new List<SmartItem>();
            List<SmartItem> movedItems = new List<SmartItem>();
            foreach(Chest chest in Chests) { 
                for(int i=0; i<chest.item.Length; ++i)
                {
                    if (chest.item[i].IsAir) continue;
                    remainingItems.Add(new SmartItem(chest, i));
                }
            }

            List<Chest> sortedChests = new List<Chest>();
            List<Chest> unsortedChests = new List<Chest>();
            foreach(Chest chest in Chests)
            {
                if (chest.HasSortRules()) sortedChests.Add(chest);
                else unsortedChests.Add(chest);
            }

            Log.Debug("Items to allocate: {0}", remainingItems.Count);

            // Avoid moving items from perfectly valid chests
            // Let items already in chests with sort rules to stay where they are
            foreach (Categorisation cat in ApplicableCategories)
            {
                foreach (SmartItem item in remainingItems)
                {
                    if (cat.ItemMatches(item.Item) && cat.AppliesToChest(item.Chest))
                    {
                        item.NewChest = item.Chest;
                        allocatedItems.Add(item);
                    }
                }
                remainingItems.RemoveAll(x => x.NewChest != null);
            }

            // Allocate items to the chests with sort rules first
            foreach(Categorisation category in ApplicableCategories)
            {
                foreach(Chest chest in sortedChests)
                {
                    if (!category.AppliesToChest(chest)) continue;
                    //Log.Debug("Chest: {0}", chest.name);
                    foreach (SmartItem item in remainingItems)
                    {
                        if (AllocatedChestFreeSlots(allocatedItems, chest) == 0) break;
                        if (!chest.ShouldStoreItem(item.Item)) continue;
                        item.NewChest = chest;
                        Log.Debug("Item allocated: {0}", item.Item);
                        allocatedItems.Add(item);
                    }
                    remainingItems.RemoveAll(x => x.NewChest != null);
                }
            }

            Log.Debug("Allocated items to chests with sort rules. Items left to allocate: {0}", remainingItems.Count);


            // Prevent other items already in suitable locations from being moved
            foreach (SmartItem item in remainingItems)
            {
                if (item.CanRemain)
                {
                    item.NewChest = item.Chest;
                    allocatedItems.Add(item);
                }
            }
            remainingItems.RemoveAll(x => x.NewChest != null);


            // Allocate remaining items to the chests without sort rules
            foreach (Chest chest in unsortedChests)
            {
                //Log.Debug("Chest: {0}", chest);
                foreach (SmartItem item in remainingItems)
                {
                    if (AllocatedChestFreeSlots(allocatedItems, chest) == 0) break;
                    if (item.CanRemain)     // Avoid moving items from a perfectly viable location otherwise items could be in a different place every time you open a chest...
                    {
                        item.NewChest = item.Chest;
                    }
                    else
                    {
                        item.NewChest = chest;
                        Log.Debug("Item allocated: {0}", item.Item);
                    }
                    allocatedItems.Add(item);
                }
                remainingItems.RemoveAll(x => x.NewChest != null);
            }

            Log.Debug("Allocated remaining items to chests without sort rules. Items left to allocate: {0}", remainingItems.Count);

            foreach (SmartItem item in allocatedItems)
            {
                if (item.NewChest == item.Chest)     
                {
                    movedItems.Add(item);
                    //Log.Debug("Item: {0} is staying in chest: {1}", item.Item, item.Chest.name);
                    continue;
                }

                if (item.MoveTo(item.NewChest)) {
                    movedItems.Add(item);
                    //Log.Debug("Moved item: {0}", item.Item);
                    continue; 
                }

                foreach(SmartItem other in allocatedItems)
                {
                    if (item == other) continue;
                    if (other.Chest != item.NewChest) continue;
                    if (movedItems.Contains(other)) continue;
                    item.SwapItems(other);
                    //Log.Debug("Swapped item: {0} with: {1}", item.Item, other.Item);
                    movedItems.Add(item);
                    break;
                }
                Log.Debug("Failed to move item: {0}", item.Item);
            }

        }

        /// <summary>
        /// Put small stacks onto larger stacks that still have space
        /// Should free up slots in the chests
        /// </summary>
        private void ConsolidateItemStacks()
        {
            bool again = true;
            while (again)
            {
                again = false;
                List<Item> items = new List<Item>();
                foreach (Chest chest in Chests)
                {
                    for (int i = 0; i < chest.item.Length; ++i)
                    {
                        Item item = chest.item[i];
                        if (item == null || item.IsAir) continue;    // Item is an empty slot (no type or no quantity)
                        if (Config.TrashItems.Contains(item.type))
                        {
                            // If the item is a trash item, delete it
                            chest.item[i].TurnToAir();
                            continue;
                        }
                        // Add the item to the list of all items in the region
                        items.Add(item);
                    }
                }

                // Attempt to put the smaller stacks of items onto the larger stacks
                // Sort item stacks highest to lowest
                items.Sort((y, x) => x.stack.CompareTo(y.stack));
                for (int i = 0; i < items.Count - 1; ++i)
                {
                    Item item1 = items[i];      // Bigger item stack
                    if(item1.IsACoin && item1.stack == 100 && item1.type != 74)     // If the item is 100 of a coin other than platinum
                    {
                        item1.type += 1;        // Turn it into the next coin
                        item1.netID += 1;
                        item1.SetDefaults(item1.type);
                        item1.stack = 1;        // Make sure there's only 1 of them
                        again = true;           // Make the whole stack consolidation run again to potentially reduce the newly created coin stack
                        continue;               // Don't try to stack anything onto this
                    }
                    for (int j = items.Count - 1; j > i; --j)
                    {
                        // Work back to front through the sorted items list (ascending order)
                        Item item2 = items[j];
                        if (item2.type != item1.type) continue;     // If the bigger stack and smaller stack are different items, skip further processing

                        if (item1.stack + item2.stack <= item1.maxStack)
                        {
                            // If the smaller stack can fit into the bigger stack, add the small stack quantity to the large stack and set the small stack quantity to 0
                            item1.stack += item2.stack;
                            item2.TurnToAir();
                        }
                        else
                        {
                            // If the smaller stack cannot entirely fit into the bigger stack, set the bigger stack to the stack limit and remove the difference from the smaller stack
                            item2.stack -= item1.maxStack - item1.stack;
                            item1.stack = item1.maxStack;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to place stacks of the same item in 1 chest
        /// </summary>
        private void ConsolidateItems()
        {
            // Attempt to move all of the stacks of the same item to the chest with the most of that item in it.
            foreach (Chest chest in Chests)
            {
                for (int i = 0; i < chest.item.Length; ++i)
                {
                    Item item = chest.item[i];
                    if(item.IsAir) continue;
                    Chest? dest = ChestWithMostOf(chest.item[i]);
                    //Log.Debug("{0} has a value of {1} and a shop price of {2}",item.Name, item.value, item.GetStoreValue());
                    if (dest == null || dest == chest) continue;
                    chest.MoveItemTo(i, dest);
                }
            }
        }

        private Chest? ChestWithMostOf(Item item)
        {
            if (item == null || item.type == 0) return null;
            Chest? result = null;
            int maxQty = 0;
            foreach(Chest chest in Chests)
            {
                int thisQty = 0;
                for(int i=0; i<chest.item.Length; ++i)
                {
                    Item cItem = chest.item[i];
                    if(cItem.IsAir || cItem.type != item.type) continue;
                    thisQty += cItem.stack;
                }
                if(thisQty > maxQty)
                {
                    maxQty = thisQty;
                    result = chest;
                }
            }

            return result;
        }
    }
}
