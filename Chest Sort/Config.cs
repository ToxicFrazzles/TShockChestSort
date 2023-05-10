using Chest_Sort;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using Terraria.ID;
using TShockAPI;
using TShockAPI.Configuration;

namespace ChestSort
{
    internal class Config
    {
        public static string DirectoryPath = Path.Combine(TShock.SavePath, "ChestSort");
        public static string ConfigPath = Path.Combine(DirectoryPath, "categories.json");

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
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("defaultCategories.json"));

                List<Categorisation>? list = null;

                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string r = reader.ReadToEnd(); //Make string equal to full file
                        try
                        {
                            list = JsonConvert.DeserializeObject<List<Categorisation>>(r);
                        }
                        catch (Newtonsoft.Json.JsonSerializationException e)
                        {
                            Categories = new List<Categorisation>();
                            Console.WriteLine("Error parsing default config file. Treating it as blank...");
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                
                
                using (StreamWriter sw = File.AppendText(ConfigPath))
                {
                    using(JsonTextWriter jw = new JsonTextWriter(sw))
                    {
                        jw.Formatting = Formatting.Indented;
                        jw.IndentChar = '\t';
                        jw.Indentation = 1;

                        JsonSerializer serializer = new JsonSerializer();
                        if(list == null)
                        {
                            list = new List<Categorisation>();
                        }
                        serializer.NullValueHandling = NullValueHandling.Ignore;
                        serializer.Serialize(jw, list);
                    }
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
