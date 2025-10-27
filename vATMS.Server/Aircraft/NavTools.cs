public static class NavTools
{
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

    public static double DistanceBetweenTwoPoints(DecimalCoordinate prev, DecimalCoordinate next)
    {
        //haversine formula, again
        const double R = 6371e+003;

        double lat1 = prev.Lat * Math.PI / 180;
        double lat2 = next.Lat * Math.PI / 180;

        double latDiff = (next.Lat - prev.Lat) * Math.PI / 180;
        double lonDiff = (next.Lon - prev.Lon) * Math.PI / 180;

        double a = Math.Sin(latDiff / 2) * Math.Sin(latDiff / 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(lonDiff / 2) * Math.Sin(lonDiff / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
}