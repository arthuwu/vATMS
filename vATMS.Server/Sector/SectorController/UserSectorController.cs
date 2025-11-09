using vATMS.NavData;

public class UserSectorController : ISectorController
{
    public string LogonId { get; set; }
    public Dictionary<string, RadarScreen> RadarScreens { get; set; }
    public List<Sector> SectorsControlled { get; set; }

    public UserSectorController(string logonId, RadarScreen radarScreen)
    {
        LogonId = logonId;

        RadarScreens = new() { { radarScreen.ConnectionId, radarScreen } };

        SectorsControlled = new List<Sector>();
    }
}