using System.Collections.Concurrent;
using vATMS.NavData;

/// <summary>
/// This is a WebApplicationBuilder.Services singleton class.
/// RadarScreenManager is responsible for storing all radarScreen instances.
/// Each radarScreen instance corresponds to one connected client
/// </summary>
public class RadarScreenManager
{
    private readonly LogonData _logonData;

    // Each UserSectorController contains a dictionary of RadarScreens, with their ConnectionId as the key
    private static ConcurrentDictionary<string, UserSectorController> _userControllerScreens = []; // Connection-related Tasks are async, so concurrency is required

    public RadarScreenManager(LogonData logonData)
    {
        _logonData = logonData;
    }

    /// <summary>
    /// Adds a radarScreen to the dictionary
    /// </summary>
    /// <param name="sectorId">the sectorId</param>
    /// <param name="radarScreen">the radar screen instance to be added</param>
    public void AddRadarScreen(string sectorId, RadarScreen radarScreen)
    {
        if (_userControllerScreens.ContainsKey(sectorId))
        {
            _userControllerScreens[sectorId].RadarScreens.TryAdd(radarScreen.ConnectionId, radarScreen);
            return;
        }

        _userControllerScreens.TryAdd(sectorId, new(sectorId, radarScreen));

        _logonData.SetSectorUserController(sectorId, _userControllerScreens[sectorId]);
    }

    /// <summary>
    /// Returns the radarScreen instances associated with the sectorId
    /// </summary>
    /// <param name="sectorId">the SignalR connectionId</param>
    /// <returns>the required RadarScreen instances</returns>
    public List<RadarScreen> GetRadarScreens(string sectorId)
    {
        UserSectorController userController;

        _userControllerScreens.TryGetValue(sectorId, out userController!); // TODO: maybe don't use the ! operator here. But, it should never be null anyway.

        return userController.RadarScreens.Values.ToList();
    }

    /// <summary>
    /// Removes the radarScreen instance associated with the connectionId
    /// </summary>
    /// <param name="connectionId">the SignalR connectionId</param>
    public void RemoveRadarScreen(string connectionId)
    {
        foreach (KeyValuePair<string, UserSectorController> kvp in _userControllerScreens)
        {
            if (kvp.Value.RadarScreens.ContainsKey(connectionId))
            {
                kvp.Value.RadarScreens.Remove(connectionId);
            }
        }
    }
}