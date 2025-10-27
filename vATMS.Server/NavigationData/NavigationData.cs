public class NavigationData
{
    public List<AirspaceVolume> AirspaceList { get; set; }
    public List<AirspaceVolume> AIPAirspaceList { get; set; }
    public Dictionary<string, Waypoint> WpList { get; set; }


    public NavigationData()
    {
        AirspaceList = AIPParser.ParseXMLSectors();

        AIPAirspaceList = AIPParser.ParseAIPAirspace();

        WpList = AIPParser.ParseDesignatedPoints();
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