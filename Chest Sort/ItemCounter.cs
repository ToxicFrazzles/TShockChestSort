using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chest_Sort
{
    internal class ItemCounter
    {
        public ItemCounter(List<SmartItem> smartItems) {
            SmartItems = smartItems;
        }

        public List<SmartItem> SmartItems { get; set; }

        public int TotalCount { get {
                int total = 0;
                foreach (SmartItem smartItem in SmartItems)
                {
                    total += smartItem.Item.stack;
                }
                return total;
            }}

        public int TotalStacks { get
            {
                return TotalCount / SmartItems.First().Item.maxStack;
            } }
    }
}
