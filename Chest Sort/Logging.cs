using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Chest_Sort
{
    static class Log
    {
        static public void Debug(string format, params object[] args)
        {
#if DEBUG
            Console.WriteLine(format, args);
#endif
        }

        public static void SendDebugMessage(this TSPlayer player, string message)
        {
#if DEBUG
            player.SendInfoMessage(message);
#endif
        }
        public static void SendDebugMessage(this TSPlayer player, string format, params object[] args)
        {
#if DEBUG
            player.SendInfoMessage(format, args);
#endif
        }
    }
}
