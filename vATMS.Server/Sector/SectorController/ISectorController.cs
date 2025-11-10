using vATMS.NavData;

/// <summary>
/// Interface for user and non user controlled sectors
/// Not used, as its intended use case was not implemented
/// </summary>
public interface ISectorController
{
    public List<Sector> SectorsControlled { get; set; }
}

