using vATMS.NavData;

/// <summary>
/// A sector object contains information related to a specific sector volume (physical piece of airspace)
/// </summary>
public class Sector
{
    public string Id { get; set; }
    public AirspaceVolume SectorVolume { get; set; }
    private ISectorController? sectorController = null;

    public Sector(
        string id,
        AirspaceVolume sectorVolume
    )
    {
        Id = id;
        SectorVolume = sectorVolume;
    }

    /// <summary>
    /// Whether the sector is currently being controlled by a user
    /// </summary>
    /// <returns>true if it is</returns>
    public bool IsUserControlled()
    {
        if (sectorController == null) return false;

        return sectorController.GetType().Equals(typeof(UserSectorController)) ? true : false;
    }

    /// <summary>
    /// Labels this sector as being controlled by a particular user
    /// </summary>
    /// <param name="sc">the required user</param>
    public void SetSectorUserController(UserSectorController sc)
    {
        sectorController = sc;
        sc.SectorsControlled.Add(this);
    }
}