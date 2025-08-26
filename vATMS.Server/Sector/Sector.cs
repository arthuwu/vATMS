namespace vATMS.Sector;

public class Sector
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<DecimalCoordinate> SectorPolygon { get; set; }
    private ISectorController? sectorController = null;

    public Sector(
        string name,
        List<DecimalCoordinate> sectorPolygon
    )
    {
        Name = name;
        SectorPolygon = sectorPolygon;

        Id = Guid.NewGuid();
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