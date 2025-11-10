namespace vATMS.NavData;

using System.Linq;
using System.Xml.Linq;

/// <summary>
/// This is a static class containing all parsing of navigation data required by the simulation
/// </summary>
public static class AIPParser
{
    /// <summary>
    /// Parses all designated points and navaids from the AIP XML
    /// </summary>
    /// <param name="webEnv">IWebHostEnvironment</param>
    /// <returns>A dictionary with key: waypoint identifier, value: Waypoint instance</returns>
    public static Dictionary<string, Waypoint> ParseDesignatedPoints(IWebHostEnvironment webEnv)
    {
        XDocument doc = XDocument.Load(Path.Combine(webEnv.WebRootPath, "NavigationData\\EG_AIP_DS_FULL.xml"));
        XNamespace aixm = "http://www.aixm.aero/schema/5.1";

        var dpNodes = doc.Descendants(aixm + "DesignatedPoint");

        var dpData = from dp in dpNodes
                     let timeSliceNode = dp.Element(aixm + "timeSlice")
                     let dpTimeSliceNode = timeSliceNode?.Element(aixm + "DesignatedPointTimeSlice")
                     let name = dpTimeSliceNode?.Element(aixm + "designator")?.Value
                     let location = dpTimeSliceNode?.Element(aixm + "location")?.Element(aixm + "Point")

                     select new Waypoint
                     (
                        name: name,
                        coord: parseSingleCoordinate(location)
                     );

        var dpDict = dpData.DistinctBy(item => item.Name).ToDictionary(item => item.Name, item => item);

        var naNodes = doc.Descendants(aixm + "Navaid");

        var naData = from na in naNodes
                     let timeSliceNode = na.Element(aixm + "timeSlice")
                     let dpTimeSliceNode = timeSliceNode?.Element(aixm + "NavaidTimeSlice")
                     let name = dpTimeSliceNode?.Element(aixm + "designator")?.Value
                     let location = dpTimeSliceNode?.Element(aixm + "location")?.Element(aixm + "ElevatedPoint")

                     where location != null

                     select new Waypoint
                     (
                        name: name,
                        coord: parseSingleCoordinate(location)
                     );

        var naDict = naData.DistinctBy(item => item.Name).ToDictionary(item => item.Name, item => item);

        foreach (KeyValuePair<string, Waypoint> point in naDict) // some DPs are also in the Navaid section
        {
            if (dpDict.ContainsKey(point.Key))
            {
                continue;
            }
            else
            {
                dpDict[point.Key] = point.Value;
            }
        }

        return dpDict;
    }

    /// <summary>
    /// Parses a single coordinate into DecimalCoordinate format
    /// </summary>
    /// <param name="location">the location element to be parsed</param>
    /// <returns>the required coordinate</returns>
    private static DecimalCoordinate parseSingleCoordinate(XElement location)
    {
        XNamespace gml = "http://www.opengis.net/gml/3.2";

        var node = location.Element(gml + "pos");
        string val = node!.Value;

        DecimalCoordinate? coord = TryParseCoordinates(val);

        if (coord != null)
            return (DecimalCoordinate)coord;
        else
        {
            throw new FormatException("Invalid coordinate");
        }
    }

    /// <summary>
    /// Parses all airspace volumes from the AIP XML
    /// This is NOT USED, as its intended use case was never implemented
    /// </summary>
    /// <param name="webEnv">IWebHostEnvironment</param>
    /// <returns>A list of airspace volumes</returns>
    public static List<AirspaceVolume> ParseAIPAirspace(IWebHostEnvironment webEnv)
    {
        XDocument doc = XDocument.Load(Path.Combine(webEnv.WebRootPath, "NavigationData\\EG_AIP_DS_FULL.xml"));
        XNamespace aixm = "http://www.aixm.aero/schema/5.1";

        string[] airspaceTypes = ["UIR", "FIR", "TMA", "CTA", "CTR"];

        var airspaceNodes = doc.Descendants(aixm + "Airspace");

        var aipData = from airspace in airspaceNodes
                      let timeSliceNode = airspace.Element(aixm + "timeSlice")
                      let airspaceTimeSliceNode = timeSliceNode?.Element(aixm + "AirspaceTimeSlice")
                      let geometryComponent = airspaceTimeSliceNode?.Element(aixm + "geometryComponent")
                      let airspaceVolume = geometryComponent?.Element(aixm + "AirspaceGeometryComponent")?.Element(aixm + "theAirspaceVolume")?.Element(aixm + "AirspaceVolume")
                      let horizontalProjection = airspaceVolume?.Element(aixm + "horizontalProjection")

                      let name = airspaceTimeSliceNode.Element(aixm + "name")?.Value

                      where name != null && airspaceTypes.Any(p => name.Contains(p))

                      // if any of these "?"s are null, we carry on to the next item
                      select new AirspaceVolume
                      (
                          name: airspaceTimeSliceNode?.Element(aixm + "name")?.Value!, // the sector dataset shouldn't break this rule
                          polygonCoordinates: ParseXMLCoordinates(horizontalProjection),
                          lowerLimit: Convert.ToInt32(airspaceVolume?.Element(aixm + "lowerLimit")?.Value) * 100, // TODO: feet/FL conversions
                          upperLimit: Convert.ToInt32(airspaceVolume?.Element(aixm + "upperLimit")?.Value) * 100
                      );

        /*
        foreach (AirspaceVolume av in aipData)
        {
            Console.WriteLine($"Name: {av.Name}\nCoordinates:{string.Join("\n", av.Polygon)}\nVertical limits: {av.LowerLimit} - {av.UpperLimit}");
        }
        */

        return aipData.ToList();
    }

    /// <summary>
    /// Parses all sector volumes from the Sectors XML
    /// </summary>
    /// <param name="webEnv">IWebHostEnvironment</param>
    /// <returns>A dictionary with key: sector identifier, value: Sector instance</returns>
    public static Dictionary<string, Sector> ParseSectors(IWebHostEnvironment webEnv)
    {
        XDocument doc = XDocument.Load(Path.Combine(webEnv.WebRootPath, "NavigationData\\EG_LAC_PC_SECTOR.xml"));
        XNamespace aixm = "http://www.aixm.aero/schema/5.1";

        var airspaceNodes = doc.Descendants(aixm + "Airspace");

        var sectorData = from airspace in airspaceNodes
                         let timeSliceNode = airspace.Element(aixm + "timeSlice")
                         let airspaceTimeSliceNode = timeSliceNode?.Element(aixm + "AirspaceTimeSlice")
                         let geometryComponent = airspaceTimeSliceNode?.Element(aixm + "geometryComponent")
                         let airspaceVolume = geometryComponent?.Element(aixm + "AirspaceGeometryComponent")?.Element(aixm + "theAirspaceVolume")?.Element(aixm + "AirspaceVolume")
                         let horizontalProjection = airspaceVolume?.Element(aixm + "horizontalProjection")

                         // if any of these "?"s are null, we carry on to the next item
                         select new Sector
                         (
                            id: airspaceTimeSliceNode?.Element(aixm + "designator")?.Value!,
                            sectorVolume: new(
                                name: airspaceTimeSliceNode?.Element(aixm + "designator")?.Value!, // the sector dataset shouldn't break this rule
                                polygonCoordinates: ParseXMLCoordinates(horizontalProjection),
                                lowerLimit: Convert.ToInt32(airspaceVolume?.Element(aixm + "lowerLimit")?.Value) * 100,
                                upperLimit: Convert.ToInt32(airspaceVolume?.Element(aixm + "upperLimit")?.Value) * 100
                            )
                         );

        return sectorData.DistinctBy(item => item.Id).ToDictionary(item => item.Id, item => item);
    }

    /// <summary>
    /// Attempts to pare a DecimalCoordinate from a string
    /// </summary>
    /// <param name="val">string in the format "lat lon", where lat and lon are in decimal</param>
    /// <returns>null if it cannot be parsed</returns>
    public static DecimalCoordinate? TryParseCoordinates(string val)
    {
        try
        {
            string[] splitVal = val.Split(' ');

            var coord = (double.TryParse(splitVal[0], out double lat) && double.TryParse(splitVal[1], out double lon))
                                ? new DecimalCoordinate(latitude: lat, longitude: lon) : (DecimalCoordinate?)null;

            return coord;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a list of coordinates into DecimalCoordinate format
    /// </summary>
    /// <param name="horizontalProjection">The element containing the original coordinates</param>
    /// <returns></returns>
    public static List<DecimalCoordinate> ParseXMLCoordinates(XElement horizontalProjection)
    {
        XNamespace gml = "http://www.opengis.net/gml/3.2";

        var posData = from node in horizontalProjection.Descendants(gml + "pos")
                      let val = node.Value

                      let coord = TryParseCoordinates(val)
                      where coord != null

                      select coord.Value;

        return posData.ToList();
    }
}