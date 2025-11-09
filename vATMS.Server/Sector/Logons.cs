using Newtonsoft.Json;
using vATMS.NavData;

public class LogonList
{
    public required List<Logon> Logons { get; set; }
}

public class Logon
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public required string IfactsName { get; set; }
    public required List<string> SectorVolumes { get; set; }
}

public class LogonData
{
    public LogonList Logons { get; set; }
    private readonly IWebHostEnvironment _webEnv;
    private readonly NavigationData _navigationData;

    public LogonData(IWebHostEnvironment webEnv, NavigationData navigationData)
    {
        _webEnv = webEnv;
        _navigationData = navigationData;

        using (var fs = new FileStream(Path.Combine(_webEnv.WebRootPath, "Sector\\SectorLogon.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sr = new StreamReader(fs))
        {
            string logonJson = sr.ReadToEnd();
            Logons = JsonConvert.DeserializeObject<LogonList>(logonJson)!;
            sr.Close();
            fs.Close();
        }
    }

    public void SetSectorUserController(string SectorId, UserSectorController sc)
    {
        List<string> sectorVolumes = Logons.Logons.FirstOrDefault(l => l.Id == SectorId)!.SectorVolumes;

        if (sectorVolumes == null) return;

        foreach (string sectorVolume in sectorVolumes)
        {
            _navigationData.SectorList[sectorVolume].SetSectorUserController(sc);
            Console.WriteLine($"Sector volume {sectorVolume} controlled by {sc.SectorId}");
        }
    }
}