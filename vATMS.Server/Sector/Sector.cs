using vATMS.NavData;

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

    public bool IsUserControlled()
    {
        if (sectorController == null) return false;

        return sectorController.GetType().Equals(typeof(UserSectorController)) ? true : false;
    }

    public void SetSectorUserController(UserSectorController sc)
    {
        sectorController = sc;
        sc.SectorsControlled.Add(this);
    }
}