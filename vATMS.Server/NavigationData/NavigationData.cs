namespace vATMS.NavData;

/// <summary>
/// This is an ASP.NET singleton class responsible for all navigation data used in the simulation
/// </summary>
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

    /// <summary>
    /// Parses a route string separated bu spaces
    /// </summary>
    /// <param name="route">the route string</param>
    /// <returns>a queue of waypoints</returns>
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