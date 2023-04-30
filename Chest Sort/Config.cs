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
                    new Categorisation() { chestName = "melee", attribute = "melee" },
                    new Categorisation() { chestName = "money", attribute = "IsCurrency" },
                    new Categorisation() { chestName = "accessories", attribute = "accessory" },
                    new Categorisation() { chestName = "bait", attribute = "bait" },
                    new Categorisation() {chestName = "banners", suffix="banner"},
                    new Categorisation() {chestName = "pickaxes", attribute="pick"},
                    new Categorisation() {chestName="potions", suffix="potion"}
                };
                using (StreamWriter sw = File.AppendText(ConfigPath))
                {
                    sw.Write(JsonConvert.SerializeObject(list));
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
