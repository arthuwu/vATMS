namespace vATMS.NavData;

public class NavigationData
{
    private readonly IWebHostEnvironment _webEnv;
    public Dictionary<string, Sector> SectorList { get; set; }
    public List<AirspaceVolume> AIPAirspaceList { get; set; }
    public Dictionary<string, Waypoint> WpList { get; set; }


    public NavigationData(IWebHostEnvironment webEnv)
    {
        _webEnv = webEnv;

        SectorList = AIPParser.ParseSectors(webEnv);

        AIPAirspaceList = AIPParser.ParseAIPAirspace(webEnv);

        WpList = AIPParser.ParseDesignatedPoints(webEnv);
    }

    public Queue<Waypoint> ParseRoute(string route)
    {
        Queue<Waypoint> parsedRoute = new();

        foreach (string wp in route.Split(' '))
        {
            try
            {
                parsedRoute.Enqueue(WpList[wp]);
            }
            catch (Exception)
            {
                Console.WriteLine($"Waypoint {wp} not found, skipping...");
            }
        }

        return parsedRoute;
    }
}