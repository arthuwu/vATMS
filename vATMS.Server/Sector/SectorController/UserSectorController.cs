using vATMS.NavData;

/// <summary>
/// Stores information related to a specific user's sectors
/// </summary>
public class UserSectorController : ISectorController
{
    public string LogonId { get; set; }
    public Dictionary<string, RadarScreen> RadarScreens { get; set; }
    public List<Sector> SectorsControlled { get; set; }

    public UserSectorController(string sectorId, RadarScreen radarScreen)
    {
        LogonId = sectorId;

        RadarScreens = new() { { radarScreen.ConnectionId, radarScreen } };

        SectorsControlled = new List<Sector>();
    }
}