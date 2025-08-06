using vATMS.Aircraft;

class TargetGenerator
{
    public int Rate { get; set; } // aircraft per hour - will be variable (not exact)
    public int PilotQuality { get; set; }

    public SpawnPoint[] SpawnPoints { get; set; }
    private List<Aircraft> _aircraft;

    public TargetGenerator(int rate, int pilotQuality, List<Aircraft> aircraft)
    {
        Rate = rate;
        PilotQuality = pilotQuality;
        SpawnPoints = [new SpawnPoint(new DecimalCoordinate(51.4680, 0.4551))];
        _aircraft = aircraft;

        _aircraft.Add(new Aircraft());

        Console.WriteLine("DEBUG: new TargetGenerator");
    }

}