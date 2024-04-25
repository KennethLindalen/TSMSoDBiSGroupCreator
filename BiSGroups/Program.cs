using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace BiSGroups;

static class Program
{
    private static readonly DatabaseManager dbm = new("Data Source=C:\\TSM\\v3\\data.db;Version=3;");
    private static bool _isBoP = false;
    public static void Main(string[] args)
    {
        dbm.CreateTableIfNotExists();
        ParseLoonsData();
    }


    public static async void ParseLoonsData()
    {
        var files = Directory.GetFiles(
            @"E:\Spill\World of Warcraft\_classic_era_\Interface\AddOns\LoonBestInSlot\Guides");
        Dictionary<string, Dictionary<string, StreamWriter>> phaseFiles =
            new Dictionary<string, Dictionary<string, StreamWriter>>();

        foreach (string file in files)
        {
            string[] lines = File.ReadAllLines(file);

            foreach (string line in lines)
            {
                string itemStr = line.Replace("LBIS:AddItem(", "").Replace(")", "").Replace("\"", "");
                string[] itemArr = itemStr.Split(",");

                if (itemArr.Length < 4 || itemArr[1].Contains("LBIS") || itemArr[0].Contains("LBIS"))
                    continue;

                // Comment out the next line to get all items if you don't care if BoP or BoE
                // This makes the process take a lot longer
                if (await GetBoPStatus(itemArr[1]))
                    continue;


                itemArr[0] = itemArr[0].Replace("spec", "");
                string phase = itemArr[0];
                string type = itemArr[3].Split("--")[0].TrimEnd();
                type = type.Replace("/", "-");
                if (type.Contains("Mit") || type.Contains("Thrt") || type.Contains("Melee") ||
                    type.Contains("Ranged") || type.Contains("Stam") || type.Contains("BIS-Alt"))
                {
                    type = type.Replace(" Mit", "").Replace(" Thrt", "")
                        .Replace(" Melee", "").Replace(" Ranged", "")
                        .Replace(" Stam", "").Replace(" BIS-Alt", " BIS");
                }

                if (!phaseFiles.ContainsKey(phase))
                {
                    phaseFiles[phase] = new Dictionary<string, StreamWriter>();
                }

                if (!phaseFiles[phase].ContainsKey(type))
                {
                    phaseFiles[phase][type] =
                        File.AppendText($"C:\\TSM\\v3\\phase-{phase}-{type.TrimStart()}-{(_isBoP ? "NoBoP" : "WithBoP")}.txt");
                }
                phaseFiles[phase][type].Write($"{itemArr[1]},");
            }
        }

        foreach (var phaseDict in phaseFiles.Values)
        {
            foreach (var file in phaseDict.Values)
            {
                file.Close();
            }
        }

        var processedFiles = Directory.GetFiles(@"C:\TSM\v3");
        foreach (string file in processedFiles)
        {
            string fileContent = File.ReadAllText(file);
            string cleanedFileContent = Regex.Replace(fileContent, @"[^a-zA-Z0-9,:]", "").TrimStart();
            if (cleanedFileContent.EndsWith(","))
                cleanedFileContent = cleanedFileContent.Remove(cleanedFileContent.Length - 1);
            
            File.WriteAllText(file, cleanedFileContent);
        }
    }

    public static async Task<bool> GetBoPStatus(string itemId)
    {
        _isBoP = true;
        string url = $"https://www.wowhead.com/classic/item={itemId}";

        if (await dbm.CheckIfItemIdExists(itemId))
            return dbm.CheckEntryInDatabase(itemId)["isBoP"] == "1";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response.Content.ReadAsStringAsync().Result);

                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//td");
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {
                        if (node.InnerText.Contains("Binds when picked up"))
                        {
                            dbm.WriteToDatabase(itemId, true);
                            return true;
                        }

                        if (node.InnerText.Contains("Binds when eqiupped"))
                        {
                            dbm.WriteToDatabase(itemId, false);
                            return false;
                        }
                    }
                }
            }
        }
        return false;
    }
}