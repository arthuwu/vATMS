using System.Collections.Concurrent;

/// <summary>
/// This is a WebApplicationBuilder.Services singleton class.
/// RadarScreenManager is responsible for storing all radarScreen instances.
/// Each radarScreen instance corresponds to one connected client
/// </summary>
public class RadarScreenManager
{
    private static ConcurrentDictionary<string, RadarScreen> _radarScreens = []; // Connection-related Tasks are async, so concurrency is required

    /// <summary>
    /// Adds a radarScreen to the dictionary
    /// </summary>
    /// <param name="connectionId">the SignalR connectionId</param>
    /// <param name="radarScreen">the radar screen instance to be added</param>
    public void AddRadarScreen(string connectionId, RadarScreen radarScreen)
    {
        _radarScreens.TryAdd(connectionId, radarScreen);
    }

    /// <summary>
    /// Returns the radarScreen instance associated with the connectionId
    /// </summary>
    /// <param name="connectionId">the SignalR connectionId</param>
    /// <returns>the required RadarScreen instance</returns>
    public RadarScreen GetRadarScreen(string connectionId)
    {
        RadarScreen radarScreen;

        _radarScreens.TryGetValue(connectionId, out radarScreen!); // TODO: maybe don't use the ! operator here. But, it should never be null anyway.
        return radarScreen;
    }

    /// <summary>
    /// Removes the radarScreen instance associated with the connectionId
    /// </summary>
    /// <param name="connectionId">the SignalR connectionId</param>
    public void RemoveRadarScreen(string connectionId)
    {
        _radarScreens.TryRemove(connectionId, out _);
    }
}