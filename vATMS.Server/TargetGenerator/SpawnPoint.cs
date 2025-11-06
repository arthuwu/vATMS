// TODO: TYPES

public class StandingAgreement
{
    public string Point { get; init; }
    public int Level { get; init; }
}
public class SpawnPoint
{
    public int Chance { get; init; }
    public List<string> Callsigns { get; init; }
    public string CallsignPattern { get; init; }
    public List<string> ADEP { get; init; }
    public List<string> ADES { get; init; }
    public string DisplayRoute { get; init; }
    public string FlownRoute { get; init; }
    public List<int> SpawnLevels { get; init; }
    public int MinimumLevel { get; init; }
    public int MaximumLevel { get; init; }
    public List<StandingAgreement> StandingAgreements { get; init; }
    public string IntentionCode { get; init; }
}

public class TargetGeneratorJson
{
    public int MinimumLevel { get; init; }
    public int MaximumLevel { get; init; }
    public List<double> SimulationCentre { get; init; }
    public int SpawnRadius { get; init; }
    public List<SpawnPoint> SpawnPoints { get; init; }
}