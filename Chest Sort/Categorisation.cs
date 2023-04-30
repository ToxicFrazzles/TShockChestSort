using Terraria;
using System.Reflection;

namespace Chest_Sort
{
    internal class Categorisation
    {
        public string? chestName { get; set; }
        public string? suffix { get; set; }
        public string? attribute { get; set; }

        public bool ItemMatches(Item item)
        {
            if (suffix != null && item.Name.ToLower().EndsWith(suffix.ToLower())) return true;
            if(attribute != null)
            {
                object? prop = item.GetType().GetField(attribute)?.GetValue(item);
                if(prop != null)
                {
                    return Convert.ToBoolean(prop);
                }
                prop = item.GetType().GetProperty(attribute)?.GetValue(item, null);
                if(prop != null)
                {
                    return Convert.ToBoolean(prop);
                }
            }
            return false;
        }

        public bool AppliesToChest(Chest chest)
        {
            return chest.name.ToLower() == chestName.ToLower();
        }
    }
}
