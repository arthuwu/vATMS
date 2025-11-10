namespace vATMS.NavData;

/// <summary>
/// This is a static class which provides methods for miscellaneous calulcations to do with coordinates
/// </summary>
public static class NavTools
{
    /// <summary>
    /// Calcualtes the great circle bearing between two coordinates (from prev to next)
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="next"></param>
    /// <returns>the required bearing</returns>
    public static double BearingBetweenTwoPoints(DecimalCoordinate prev, DecimalCoordinate next)
    {
        //haversine formula
        double lat1 = prev.Lat * Math.PI / 180;
        double lon1 = prev.Lon * Math.PI / 180;

        double lat2 = next.Lat * Math.PI / 180;
        double lon2 = next.Lon * Math.PI / 180;

        double a = Math.Sin(lon2 - lon1) * Math.Cos(lat2);
        double b = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1);

        double radians = Math.Atan2(a, b);
        double brng = (radians * (180 / Math.PI) + 360) % 360;

        return brng;
    }

    /// <summary>
    /// Calcualtes the great circle distance between two coordinates
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="next"></param>
    /// <returns>the required distance</returns>
    public static double DistanceBetweenTwoPoints(DecimalCoordinate prev, DecimalCoordinate next)
    {
        //haversine formula
        const double R = 6371e+003;
        const double M_TO_NM = 0.000539957;

        double lat1 = prev.Lat * Math.PI / 180;
        double lat2 = next.Lat * Math.PI / 180;

        double latDiff = (next.Lat - prev.Lat) * Math.PI / 180;
        double lonDiff = (next.Lon - prev.Lon) * Math.PI / 180;

        double a = Math.Sin(latDiff / 2) * Math.Sin(latDiff / 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(lonDiff / 2) * Math.Sin(lonDiff / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c * M_TO_NM;
    }

    /// <summary>
    /// Translates a point by parameters
    /// </summary>
    /// <param name="position">the point to be translated</param>
    /// <param name="direction">great circle beaing</param>
    /// <param name="distance">great circle distance</param>
    /// <returns>the new position</returns>
    public static DecimalCoordinate TranslatePoint(DecimalCoordinate position, double direction, double distance)
    {
        const double NM_TO_M = 1852;
        const double R = 6371e+003;

        double old_lat = position.Lat * Math.PI / 180;
        double old_lon = position.Lon * Math.PI / 180;
        double brng = direction * Math.PI / 180; // rad
        double magnitude = distance * NM_TO_M; // ms^-1

        double new_lat = Math.Asin(Math.Sin(old_lat) * Math.Cos(magnitude / R) + Math.Cos(old_lat) * Math.Sin(magnitude / R) * Math.Cos(brng));
        double new_lon = old_lon + Math.Atan2(Math.Sin(brng) * Math.Sin(magnitude / R) * Math.Cos(old_lat), Math.Cos(magnitude / R) - Math.Sin(old_lat) * Math.Sin(new_lat));


        return new() { Lat = new_lat * 180 / Math.PI, Lon = new_lon * 180 / Math.PI };
    }
}