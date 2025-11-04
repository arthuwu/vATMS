using vATMS.NavData;

internal class NPSectorController : ISectorController
{
    public List<Sector> SectorsControlled { get; set; }

    public NPSectorController()
    {
        SectorsControlled = new List<Sector>();
    }
}