using vATMS.NavData;

/// <summary>
/// Represents a non-user controller of a sector
/// Not used, as its intended use case was not implemented
/// </summary>
internal class NPSectorController : ISectorController
{
    public List<Sector> SectorsControlled { get; set; }

    public NPSectorController()
    {
        SectorsControlled = new List<Sector>();
    }
}