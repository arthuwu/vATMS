using System.Collections.Concurrent;
using vATMS.Aircraft;
using vATMS.NavData;
using Newtonsoft.Json;

/// <summary>
/// Responsible for the spawning of all aircraft within a Simulation instance
/// </summary>
class TargetGenerator
{
    public int MinimumLevel { get; init; }
    public int MaximumLevel { get; init; }
    public DecimalCoordinate SimulationCentre { get; init; }
    public int SpawnRadius { get; init; }

    private readonly IWebHostEnvironment _webEnv;
    private readonly NavigationData _navData;
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts;
    private readonly Task _spawnAircraftLoop;

    private List<AircraftType> _aircraftTypes;

    public List<SpawnPoint> SpawnPoints { get; set; }
    private ConcurrentDictionary<string, Aircraft> _aircraft;

    private List<int> _weightedSpawnSum = new();

    public TargetGenerator(IWebHostEnvironment webEnv, ConcurrentDictionary<string, Aircraft> aircraft, NavigationData navData)
    {
        _webEnv = webEnv;
        _navData = navData;
        _aircraft = aircraft;

        using (var fs1 = new FileStream(Path.Combine(_webEnv.WebRootPath, "TargetGenerator\\SpawnPoints.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sr1 = new StreamReader(fs1))
        {
            string targetJson = sr1.ReadToEnd();
            TargetGeneratorJson file = JsonConvert.DeserializeObject<TargetGeneratorJson>(targetJson)!;

            MinimumLevel = file.MinimumLevel;
            MaximumLevel = file.MaximumLevel;
            SimulationCentre = new(file.SimulationCentre.ElementAt(0), file.SimulationCentre.ElementAt(1));
            SpawnRadius = file.SpawnRadius;

            SpawnPoints = file.SpawnPoints;

            sr1.Close();
            fs1.Close();
        }

        using (var fs2 = new FileStream(Path.Combine(_webEnv.WebRootPath, "TargetGenerator\\Aircraft.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sr2 = new StreamReader(fs2))
        {
            string aircraftJson = sr2.ReadToEnd();
            _aircraftTypes = JsonConvert.DeserializeObject<AircraftJson>(aircraftJson)!.AircraftTypes;

            foreach (AircraftType ac in _aircraftTypes)
            {
                Console.WriteLine(ac.Type);
            }
            sr2.Close();
            fs2.Close();
        }

        foreach (SpawnPoint sp in SpawnPoints) // cumulative distribution function
        {
            _weightedSpawnSum.Add(_weightedSpawnSum.LastOrDefault() + sp.Chance);
        }

        Console.WriteLine("DEBUG: new TargetGenerator");

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(10)); // initialise "paused" timer

        SetupTimer(25); // The default spawn rate is 25. This value will be (re)set once loading has complete - the user is able to change the spawn rate during their session

        _cts = new CancellationTokenSource();
        _spawnAircraftLoop = Task.Run(() => SpawnAircraftLoop(_cts.Token));
    }

    /// <summary>
    /// Changes the spawn rate 
    /// </summary>
    /// <param name="rate">the required rate</param>
    public void SetupTimer(int rate)
    {
        Console.WriteLine($"rate {rate}");
        // rough derivation of spawn rate
        int spawnInterval = 60 / rate * (SpawnPoints.Count() / 4);

        _timer.Period = TimeSpan.FromMinutes(spawnInterval);
    }

    /// <summary>
    /// Spawns a new aircraft
    /// </summary>
    private async Task SpawnAircraftLoop(CancellationToken token)
    {
        SpawnAircraft(); // initial spawn on load, before awaiting

        while (await _timer.WaitForNextTickAsync(token))
        {
            SpawnAircraft();
        }
    }

    /// <summary>
    /// Gets the spawn rate of the TargetGenerator's timer
    /// </summary>
    /// <returns>The required spawn rate</returns>
    public int GetSpawnRate()
    {
        return Convert.ToInt32(_timer.Period.TotalSeconds);
    }

    private void SpawnAircraft()
    {
        Random random = new();

        int weightedSpawn = random.Next(1, _weightedSpawnSum.LastOrDefault());

        int requiredIndex = _weightedSpawnSum.BinarySearch(weightedSpawn);

        if (requiredIndex < 0)
        {
            requiredIndex = ~requiredIndex; // BinarySearch returns the bitwise complement
        }

        SpawnPoint spawnPoint = SpawnPoints[requiredIndex];

        AircraftType aircraftType = _aircraftTypes[random.Next(0, _aircraftTypes.Count())];

        FlightPlan fp = new FlightPlan(
                callsign: GetRandomCallsign(spawnPoint.Callsigns, spawnPoint.CallsignPattern),
                flightrules: "I",
                atyp: aircraftType.Type,
                wtc: aircraftType.WTC, // not in use
                equipment: "SDGIJ1LRWXY/LB1", // not in use
                dep: spawnPoint.ADEP[random.Next(spawnPoint.ADEP.Count())],
                dest: spawnPoint.ADES[random.Next(spawnPoint.ADES.Count())],
                eobt: 0000, // not in use
                eet: 0000, // not in use
                altn: "ZZZZ", // not in use
                rmk: "", // not in use
                rfl: GetRandomRFL(spawnPoint.MinimumLevel, spawnPoint.MaximumLevel),
                spd: GetRandomSpeed(),
                route: _navData.ParseRoute(spawnPoint.FlownRoute),
                displayRoute: spawnPoint.DisplayRoute,
                standingAgreements: spawnPoint.StandingAgreements,
                intentionCode: spawnPoint.IntentionCode
            );


        _aircraft.TryAdd(fp.Callsign, new Aircraft(
                flightPlan: fp,
                position: NavTools.TranslatePoint(
                    fp.Route.Peek().Coordinate,
                    NavTools.BearingBetweenTwoPoints(fp.Route.ElementAt(1).Coordinate, fp.Route.Peek().Coordinate),
                    15),
                track: NavTools.BearingBetweenTwoPoints(fp.Route.Peek().Coordinate, fp.Route.ElementAt(1).Coordinate),
                pressureAltitude: spawnPoint.SpawnLevels[random.Next(spawnPoint.SpawnLevels.Count())],
                navigationData: _navData,
                aircraftPerformance: aircraftType.AircraftPerformance
            ));
    }

    private string GetRandomCallsign(List<string> ICAO, string type)
    {
        Random random = new();
        string callsign = ICAO[random.Next(ICAO.Count())];

        if (type == "alphaNumeric")
        {
            char[] alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray(); // "I" and "O" are excluded intentionally
            callsign += random.Next(0, 99);
            callsign += alphabet[random.Next(alphabet.Length)];
        }
        else
        {
            callsign += random.Next(0, 9999);
        }

        if (_aircraft.ContainsKey(callsign)) //duplicate
            GetRandomCallsign(ICAO, type);

        return callsign;
    }

    private int GetRandomRFL(int min, int max)
    {
        Random random = new();
        return random.Next(min / 1000, max / 1000) * 1000;
    }

    private int GetRandomSpeed()
    {
        Random random = new();
        return random.Next(340, 550);
    }

    /// <summary>
    /// Despawns aircraft if they are outside the range defined in the JSON file
    /// </summary>
    public void DespawnIfOutsideRange()
    {
        foreach (Aircraft ac in _aircraft.Values.ToList())
        {
            if (ac.PressureAltitude < MinimumLevel ||
                ac.PressureAltitude > MaximumLevel ||
                NavTools.DistanceBetweenTwoPoints(ac.Position, SimulationCentre) > SpawnRadius)
            {
                _aircraft.TryRemove(ac.FlightPlan.Callsign, out _);
            }
        }
    }
}