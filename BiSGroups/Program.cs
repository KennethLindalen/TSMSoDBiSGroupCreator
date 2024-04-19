using System.Text.RegularExpressions;

namespace BiSGroups;

static class Program
{
    public static void Main(string[] args)
    {
        Loons();
    }


    public static void Loons()
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

                itemArr[0] = itemArr[0].Replace("spec", "");
                string phase = itemArr[0];
                string type = itemArr[3].Split("--")[0].TrimEnd();
                type = type.Replace("/", "-");
                if (type.Contains("Mit") || type.Contains("Thrt") || type.Contains("Melee") ||
                    type.Contains("Ranged") || type.Contains("Stam") || type.Contains("BIS-Alt"))
                {
                    type = type.Replace(" Mit", "");
                    type = type.Replace(" Thrt", "");
                    type = type.Replace(" Melee", "");
                    type = type.Replace(" Ranged", "");
                    type = type.Replace(" Stam", "");
                    type = type.Replace(" BIS-Alt", " BIS");
                }
                
                if (!phaseFiles.ContainsKey(phase))
                {
                    phaseFiles[phase] = new Dictionary<string, StreamWriter>();
                }
                
                if (!phaseFiles[phase].ContainsKey(type))
                {
                    phaseFiles[phase][type] = File.AppendText($"C:\\TSM\\v3\\phase-{phase}-{type.TrimStart()}.txt");
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
            string cleanedFileContent = Regex.Replace(fileContent, @"[^a-zA-Z0-9,:]", "");
            File.WriteAllText(file, cleanedFileContent);
        }
    }
}