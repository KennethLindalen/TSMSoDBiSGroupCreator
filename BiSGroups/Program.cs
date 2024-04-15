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
        Dictionary<string, StreamWriter> phaseFiles = new Dictionary<string, StreamWriter>();
        foreach (string file in files)
        {
            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                string itemStr = line.Replace("LBIS:AddItem(", "")
                    .Replace(")", "")
                    .Replace("\"", "");
                string[] itemArr = itemStr.Split(",");

                if (itemArr.Length < 2 || itemArr[1].Contains("LBIS") || itemArr[0].Contains("LBIS"))
                    continue;

                itemArr[0] = itemArr[0].Replace("spec", "");
                string phase = itemArr[0];
                
                if (!phaseFiles.ContainsKey(phase))
                {
                    phaseFiles[phase] = File.AppendText($"C:\\TSM\\v3\\master-{phase}-{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
                }
                phaseFiles[phase].Write($"{itemArr[1]},");
            }
        }
        
        foreach (var file in phaseFiles.Values)
        {
            file.Close();
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