using Newtonsoft.Json;
using vATMS.NavData;

// LogonList, Logon, and LogonData are classes used for JSON deserialisation
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

/// <summary>
/// Provides data required for the selection and management of user controlled sectors
/// </summary>
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

    /// <summary>
    /// Labels a specific sector as being controlled by a particular user
    /// </summary>
    /// <param name="SectorId">the sector to be labelled</param>
    /// <param name="sc">the user controller</param>
    public void SetSectorUserController(string SectorId, UserSectorController sc)
    {
        List<string> sectorVolumes = Logons.Logons.FirstOrDefault(l => l.Id == SectorId)!.SectorVolumes;

        if (sectorVolumes == null) return;

        foreach (string sectorVolume in sectorVolumes)
        {
            _navigationData.SectorList[sectorVolume].SetSectorUserController(sc);
            //Console.WriteLine($"Sector volume {sectorVolume} controlled by {sc.LogonId}");
        }
    }
}