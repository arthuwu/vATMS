using vATMS.NavData;

public class Sector
{
    public string Id { get; set; }
    public AirspaceVolume SectorVolume { get; set; }
    private ISectorController? _sectorController = null;

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
        if (_sectorController == null) return false;

        return _sectorController.GetType().Equals(typeof(UserSectorController)) ? true : false;
    }

    public string? GetUserController()
    {
        if (_sectorController == null) return null;

        if (_sectorController.GetType().Equals(typeof(UserSectorController)))
        {
            UserSectorController usc = (UserSectorController)_sectorController;
            return usc.LogonId;
        }

        return null;
    }

    public void SetSectorUserController(UserSectorController sc)
    {
        _sectorController = sc;
        sc.SectorsControlled.Add(this);
    }
}