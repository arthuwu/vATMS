using vATMS.NavData;

public class UserSectorController : ISectorController
{
    public string SectorId { get; set; }
    public Dictionary<string, RadarScreen> RadarScreens { get; set; }
    public List<Sector> SectorsControlled { get; set; }

    public UserSectorController(string sectorId, RadarScreen radarScreen)
    {
        SectorId = sectorId;

        // sector allocation logic here

        RadarScreens = new() { { radarScreen.ConnectionId, radarScreen } };

        SectorsControlled = new List<Sector>();
    }
}