using Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChestSort;
using Mono.Cecil;
using System.Reflection;

namespace Chest_Sort
{
    internal static class ItemExtensions
    {
        public static bool IsFish(this Item item)
        {
            //Console.WriteLine("{0}: {1}",item, item.type);
            if (item.type == 2290) return true;
            else if (item.type >= 2297 && item.type <= 2321) return true;
            else if (item.type == 4401) return true;
            else if (item.type >= 2450 && item.type <= 2488) return true;
            return false;
        }

        public static IEnumerable<MethodInfo> GetExtensionMethods(this Item item)
        {
            Assembly assembly = typeof(ItemExtensions).Assembly;
            var isGenericTypeDefinition = typeof(Item).IsGenericType && typeof(Item).IsTypeDefinition;
            var query = from type in assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where isGenericTypeDefinition
                            ? method.GetParameters()[0].ParameterType.IsGenericType && method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Item)
                            : method.GetParameters()[0].ParameterType == typeof(Item)
                        select method;
            return query;
        }

        public static MethodInfo? GetExtensionMethod(this Item item, string name)
        {
            var methods = GetExtensionMethods(item);
            foreach( MethodInfo method in methods)
            {
                if (method.Name == name) return method;
            }
            return null;
        }
    }

    internal class SmartItem
    {
        public SmartItem(Chest chest, int slot)
        {
            Chest = chest;
            Slot = slot;
        }
        public Chest Chest { get; private set; }
        public int Slot { get; private set; }

        public Chest? NewChest { get; set; } = null;

        public Item Item { get
            {
                return Chest.item[Slot];
            } 
        }

        public bool MoveTo(Chest chest)
        {
            int destSlot = chest.NextAvailableSlot();
            if (destSlot < 0) return false;     // No available slot, return false to indicate failure
            Item item2 = chest.item[destSlot];
            if (!item2.IsAir) return false;     // The available slot is not empty for some reason... return false to indicate failure

            // Populate the destination slot with the item
            chest.item[destSlot] = Item;
            // Populate the originating slot with the "item" that was in the destination (saves trying to create a new item object)
            Chest.item[Slot] = item2;

            // Update the chest and slot of the smart item
            Chest = chest;
            Slot = destSlot;
            return true;
        }

        public void SwapItems(SmartItem other)
        {
            // Store enough info temporarily so we can swap the items
            Item item1 = Item;
            int oldSlot = Slot;
            Chest oldChest = Chest;
            Item item2 = other.Item;

            // Put the items in their new locations
            Chest.item[Slot] = item2;
            other.Chest.item[other.Slot] = item1;
            
            // Update the Chest and Slot of each smart item
            Chest = other.Chest;
            Slot = other.Slot;
            other.Chest = oldChest;
            other.Slot = oldSlot;
        }

        public bool CanRemain { get
            {
                return Chest.ShouldStoreItem(Item);
            }
        }


    }
}
