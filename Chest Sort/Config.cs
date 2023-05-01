using Chest_Sort;
using Newtonsoft.Json;
using System.IO;
using Terraria.ID;
using TShockAPI;
using TShockAPI.Configuration;

namespace ChestSort
{
    internal class Config
    {
        public static string DirectoryPath = Path.Combine(TShock.SavePath, "ChestSort");
        public static string ConfigPath = Path.Combine(DirectoryPath, "config.json");

        public static string FilePath = Path.Combine(DirectoryPath, "trash.json");

        public static List<int> TrashItems = new List<int>()
        {
            // Tombstones
            321, 1173, 1174, 1175, 1176, 1177,
            3230, 3231, 3229, 3233, 3232,
            
            // Trash
            2337, 2338, 2339, 1922,
        };

        static private void PrimeDirectory()
        {
            if (!Directory.Exists(DirectoryPath)) {
                Directory.CreateDirectory(DirectoryPath);
                Console.WriteLine("Created config directory for the ChestSort plugin: {0}", DirectoryPath);
            }

            if (!File.Exists(ConfigPath))
            {
                List<Categorisation> list = new List<Categorisation>
                {
                    new Categorisation() { ChestName = "pickaxes", Attributes=new List<string>(){"pick" } },
                    new Categorisation() { ChestName = "axes", Attributes=new List<string>(){"axe"}},
                    new Categorisation() { ChestName = "hammers", Attributes=new List<string>(){ "hammer"} },
                    new Categorisation() {ChestName = "ammo", Attributes=new List<string>(){"ammo"} },
                    new Categorisation() { ChestName = "melee", Attributes = new List<string>(){"melee"} },
                    new Categorisation() { ChestName = "ranged", Attributes=new List<string>(){"ranged"} },
                    new Categorisation() { ChestName = "magic", Attributes=new List<string>(){"magic"} },
                    new Categorisation() { ChestName = "summon", Attributes=new List<string>(){"summon", "sentry"} },
                    new Categorisation() { ChestName = "money", Attributes = new List <string>() { "IsCurrency" } },
                    new Categorisation() { ChestName = "accessories", Attributes = new List <string>() { "accessory" } },
                    new Categorisation() { ChestName = "bait", Attributes = new List <string>() { "bait" } },
                    new Categorisation() { ChestName = "banners", Suffixes=new List <string>() { "banner" }},
                    new Categorisation() { ChestName="potions", Suffixes=new List <string>() { "potion" }},
                    new Categorisation() { ChestName = "blocks", Suffixes=new List <string>() {"block"}},
                    new Categorisation() {ChestName="walls", Suffixes=new List<string>(){"wall", "fence"}},
                    new Categorisation() {ChestName="wood", Suffixes=new List<string>(){"wood"}, ItemNames=new List<string>(){"rich mahogany"} },
                    new Categorisation() {ChestName="platforms", Suffixes=new List<string>(){"platform"} },
                    new Categorisation() {ChestName="seeds", Suffixes=new List<string>(){"seeds" } },
                    new Categorisation() {ChestName="ingots", Suffixes=new List<string>() {"bar"}},
                    new Categorisation() {ChestName="ores", Suffixes=new List<string>(){"ore"}, ItemNames=new List<string>(){"obsidian"} },
                    new Categorisation() {ChestName="chests", Suffixes=new List<string>(){"chest"} },
                    new Categorisation() {ChestName="gems", ItemNames=new List<string>(){"ruby", "diamond", "sapphire", "emerald", "topaz", "amber", "amethyst", "amber gemcorn", "amethyst gemcorn", "diamond gemcorn", "emerald gemcorn", "ruby gemcorn", "sapphire gemcorn", "topaz gemcorn"}}
                };
                using (StreamWriter sw = File.AppendText(ConfigPath))
                {
                    sw.Write(JsonConvert.SerializeObject(
                        list, 
                        Formatting.Indented, 
                        new JsonSerializerSettings{
                        NullValueHandling = NullValueHandling.Ignore
                        }
                    ));
                }
            }
        }

        public static void Reload()
        {
            PrimeDirectory();
            using (StreamReader r = new StreamReader(ConfigPath))
            {
                string json = r.ReadToEnd();
                try
                {
                    Categories = JsonConvert.DeserializeObject<List<Categorisation>>(json);
                }catch(Newtonsoft.Json.JsonSerializationException e)
                {
                    Categories = new List<Categorisation>();
                    Console.WriteLine("Error parsing categories config file. Treating it as blank...");
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static List<Categorisation> Categories { get; private set; }
    }
}
