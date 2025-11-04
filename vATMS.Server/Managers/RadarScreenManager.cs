using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// This is a WebApplicationBuilder.Services singleton class.
/// RadarScreenManager is responsible for storing all radarScreen instances.
/// Each radarScreen instance corresponds to one connected client
/// </summary>
public class RadarScreenManager
{
    private static ConcurrentDictionary<string, List<RadarScreen>> _radarScreens = []; // Connection-related Tasks are async, so concurrency is required

    /// <summary>
    /// Adds a radarScreen to the dictionary
    /// </summary>
    /// <param name="sectorId">the sectorId</param>
    /// <param name="radarScreen">the radar screen instance to be added</param>
    public void AddRadarScreen(string sectorId, RadarScreen radarScreen)
    {
        if (_radarScreens.ContainsKey(sectorId))
        {
            _radarScreens[sectorId].Append(radarScreen);
            return;
        }

        _radarScreens.TryAdd(sectorId, [radarScreen]);
    }

    /// <summary>
    /// Returns the radarScreen instances associated with the sectorId
    /// </summary>
    /// <param name="sectorId">the SignalR connectionId</param>
    /// <returns>the required RadarScreen instances</returns>
    public List<RadarScreen> GetRadarScreen(string sectorId)
    {
        List<RadarScreen> radarScreens;

        _radarScreens.TryGetValue(sectorId, out radarScreens!); // TODO: maybe don't use the ! operator here. But, it should never be null anyway.
        return radarScreens;
    }

    /// <summary>
    /// Removes the radarScreen instance associated with the connectionId
    /// </summary>
    /// <param name="connectionId">the SignalR connectionId</param>
    public void RemoveRadarScreen(string connectionId)
    {
        foreach (KeyValuePair<string, List<RadarScreen>> kvp in _radarScreens)
        {
            RadarScreen rs = kvp.Value.FirstOrDefault(rs => rs.ConnectionId == connectionId) ?? throw new ArgumentException($"Connection does not exist in dictionary: ConnectionId {connectionId}");

            kvp.Value.Remove(rs);
        }
    }
}