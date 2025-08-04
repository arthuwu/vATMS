namespace vATMS.TargetGenerator;

public class TargetGenerator
{
    public int Rate { get; set; } // aircraft per hour - will be variable (not exact)
    public int PilotQuality { get; set; }

    public required SpawnPoint[] SpawnPoints { get; set; }


    public TargetGenerator(int rate, int pilotQuality)
    {
        Rate = rate;
        PilotQuality = pilotQuality;
    }
}