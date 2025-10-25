using System.Xml;
using System.Linq;
using System.Xml.Serialization;
using System.Xml.Linq;

public static class AIPParser
{
    public static List<Waypoint> ParseDesignatedPoints()
    {
        XDocument doc = XDocument.Load("./EG_AIP_DS_FULL.xml");
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

        foreach (Waypoint wp in dpData)
        {
            Console.WriteLine(wp.Name);
        }

        return dpData.ToList();
    }

    public static DecimalCoordinate? parseSingleCoordinate(XElement location)
    {
        XNamespace gml = "http://www.opengis.net/gml/3.2";

        var node = location.Element(gml + "pos");
        string val = node.Value;
        DecimalCoordinate? coord = TryParseCoordinates(val);

        if (coord != null)
            return coord;

        return null;
    }

    public static List<AirspaceVolume> ParseAIPAirspace()
    {
        XDocument doc = XDocument.Load("./EG_AIP_DS_FULL.xml");
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
                          lowerLimit: Convert.ToInt32(airspaceVolume?.Element(aixm + "lowerLimit")?.Value) * 100,
                          upperLimit: Convert.ToInt32(airspaceVolume?.Element(aixm + "upperLimit")?.Value) * 100
                      );

        foreach (AirspaceVolume av in aipData)
        {
            Console.WriteLine($"Name: {av.Name}\nCoordinates:{string.Join("\n", av.Polygon)}\nVertical limits: {av.LowerLimit} - {av.UpperLimit}");
        }

        return aipData.ToList();
    }
    public static List<AirspaceVolume> ParseXMLSectors()
    {
        XDocument doc = XDocument.Load("./EG_LAC_PC_SECTOR.xml");
        XNamespace aixm = "http://www.aixm.aero/schema/5.1";

        var airspaceNodes = doc.Descendants(aixm + "Airspace");

        var sectorData = from airspace in airspaceNodes
                         let timeSliceNode = airspace.Element(aixm + "timeSlice")
                         let airspaceTimeSliceNode = timeSliceNode?.Element(aixm + "AirspaceTimeSlice")
                         let geometryComponent = airspaceTimeSliceNode?.Element(aixm + "geometryComponent")
                         let airspaceVolume = geometryComponent?.Element(aixm + "AirspaceGeometryComponent")?.Element(aixm + "theAirspaceVolume")?.Element(aixm + "AirspaceVolume")
                         let horizontalProjection = airspaceVolume?.Element(aixm + "horizontalProjection")

                         // if any of these "?"s are null, we carry on to the next item
                         select new AirspaceVolume
                         (
                            name: airspaceTimeSliceNode?.Element(aixm + "name")?.Value!, // the sector dataset shouldn't break this rule
                            polygonCoordinates: ParseXMLCoordinates(horizontalProjection),
                            lowerLimit: Convert.ToInt32(airspaceVolume?.Element(aixm + "lowerLimit")?.Value) * 100,
                            upperLimit: Convert.ToInt32(airspaceVolume?.Element(aixm + "upperLimit")?.Value) * 100
                         );

        return sectorData.ToList();
    }

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