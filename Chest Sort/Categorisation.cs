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


        // TODO: Add functionality for CanSell and SellThreshold
        public bool? CanSell { get; set; }

        public int? SellThreshold { get; set; }

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
                // Try getting a field first
                object? field = item.GetType().GetField(attr)?.GetValue(item);
                if(field != null)
                {
                    return (field is int && (int)field > 0) || (!(field is int) && Convert.ToBoolean(field));
                }

                // No field with that name? Try for a property
                object? prop = item.GetType().GetProperty(attr)?.GetValue(item, null);
                if (prop != null)
                {
                    return (prop is int && (int)prop > 0) || (!(prop is int) && Convert.ToBoolean(prop));
                }

                // No field or property? Try for a method with no args that returns an int or a bool
                var meth = typeof(Item).GetMethod(attr);
                if (meth != null && meth.GetGenericArguments().Length == 0 &&(meth.ReturnType == typeof(bool) || meth.ReturnType == typeof(int)))
                {
                    var res = meth.Invoke(item, null);
                    return (res is int && (int)res > 0) || (!(res is int) && Convert.ToBoolean(res));
                }

                // No field, property or method? Try for an extension method
                meth = item.GetExtensionMethod(attr);
                if (meth != null && meth.GetGenericArguments().Length == 0 && (meth.ReturnType == typeof(bool) || meth.ReturnType == typeof(int)))
                {
                    var res = meth.Invoke(null, new object[]{item});
                    return (res is int && (int)res > 0) || (!(res is int) && Convert.ToBoolean(res));
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
