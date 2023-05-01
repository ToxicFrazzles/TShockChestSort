using Terraria;
using System.Reflection;

namespace Chest_Sort
{
    internal class Categorisation
    {
        public string? ChestName { get; set; }
        public List<string>? Suffixes { get; set; }
        public List<string>? Attributes { get; set; }

        public List<string>? ItemNames { get; set; }

        private bool ItemHasSuffix(Item item)
        {
            string itemName = item.Name.ToLower();
            if(Suffixes == null) return false; 
            foreach(string suffix in Suffixes)
            {
                if (itemName.EndsWith(suffix.ToLower())) return true;
            }
            return false;
        }

        private bool ItemHasAttribute(Item item)
        {
            if(Attributes == null) return false;
            foreach(string attr in Attributes)
            {
                object? prop = item.GetType().GetField(attr)?.GetValue(item);
                if (prop != null && Convert.ToBoolean(prop))
                {
                    return true;
                }
                prop = item.GetType().GetProperty(attr)?.GetValue(item, null);
                if (prop != null && Convert.ToBoolean(prop))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ItemHasName(Item item)
        {
            if(ItemNames == null) return false;
            foreach(string name in ItemNames)
            {
                if(item.Name.ToLower() == name.ToLower()) return true;
            }
            return false;
        }

        public bool ItemMatches(Item item)
        {
            if(ItemHasName(item)) return true;
            if(ItemHasSuffix(item)) return true;
            if(ItemHasAttribute(item)) return true;
            return false;
        }

        public bool AppliesToChest(Chest chest)
        {
            if(ChestName == null) return false;
            return chest.name.ToLower() == ChestName.ToLower();
        }
    }
}
