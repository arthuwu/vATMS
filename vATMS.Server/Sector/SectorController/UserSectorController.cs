using vATMS.Sector;

public class UserSectorController : ISectorController
{
    public List<Sector> SectorsControlled { get; set; }

    public UserSectorController()
    {
        SectorsControlled = new List<Sector>();
    }
}