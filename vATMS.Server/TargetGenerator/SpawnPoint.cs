// TODO: TYPES

public class StandingAgreement
{
    public required string Point { get; init; }
    public required int Level { get; init; }
}
public class SpawnPoint
{
    public required int Chance { get; init; }
    public required List<string> Callsigns { get; init; }
    public required string CallsignPattern { get; init; }
    public required List<string> ADEP { get; init; }
    public required List<string> ADES { get; init; }
    public required string DisplayRoute { get; init; }
    public required string FlownRoute { get; init; }
    public required List<int> SpawnLevels { get; init; }
    public required int MinimumLevel { get; init; }
    public required int MaximumLevel { get; init; }
    public required List<StandingAgreement> StandingAgreements { get; init; }
    public required string IntentionCode { get; init; }
}

public class TargetGeneratorJson
{
    public required int MinimumLevel { get; init; }
    public required int MaximumLevel { get; init; }
    public required List<double> SimulationCentre { get; init; }
    public required int SpawnRadius { get; init; }
    public required List<SpawnPoint> SpawnPoints { get; init; }
}