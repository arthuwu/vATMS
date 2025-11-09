using System.Collections.Concurrent;
using vATMS.Aircraft;
using vATMS.NavData;
using Newtonsoft.Json;

class TargetGenerator
{
    public int Rate { get; set; }
    public int MinimumLevel { get; init; }
    public int MaximumLevel { get; init; }
    public DecimalCoordinate SimulationCentre { get; init; }
    public int SpawnRadius { get; init; }

    private readonly IWebHostEnvironment _webEnv;
    private readonly NavigationData _navData;
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts;
    private readonly Task _spawnAircraft;

    public List<SpawnPoint> SpawnPoints { get; set; }
    private ConcurrentDictionary<string, Aircraft> _aircraft;

    private List<int> _weightedSpawnSum = new();

    public TargetGenerator(IWebHostEnvironment webEnv, int rate, ConcurrentDictionary<string, Aircraft> aircraft, NavigationData navData)
    {
        Rate = rate;
        _webEnv = webEnv;
        _navData = navData;
        _aircraft = aircraft;

        using (var fs = new FileStream(Path.Combine(_webEnv.WebRootPath, "TargetGenerator\\SpawnPoints.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sr = new StreamReader(fs))
        {
            string targetJson = sr.ReadToEnd();
            TargetGeneratorJson file = JsonConvert.DeserializeObject<TargetGeneratorJson>(targetJson)!;

            MinimumLevel = file.MinimumLevel;
            MaximumLevel = file.MaximumLevel;
            SimulationCentre = new(file.SimulationCentre.ElementAt(0), file.SimulationCentre.ElementAt(1));
            SpawnRadius = file.SpawnRadius;

            SpawnPoints = file.SpawnPoints;

            sr.Close();
            fs.Close();
        }

        foreach (SpawnPoint sp in SpawnPoints)
        {
            _weightedSpawnSum.Add(_weightedSpawnSum.LastOrDefault() + sp.Chance);
        }

        Console.WriteLine("DEBUG: new TargetGenerator");

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(10)); // initialise "paused" timer

        SetupTimer(rate);

        _cts = new CancellationTokenSource();
        _spawnAircraft = Task.Run(() => SpawnAircraft(_cts.Token));
    }

    public void SetupTimer(int rate)
    {
        Console.WriteLine($"rate {rate}");
        // rough derivation of spawn rate
        int spawnInterval = (3600000 / rate) * SpawnPoints.Count();

        _timer.Period = TimeSpan.FromMilliseconds(spawnInterval);
    }

    private async Task SpawnAircraft(CancellationToken token)
    {
        while (await _timer.WaitForNextTickAsync(token))
        {
            Console.WriteLine("spawning");
            Random random = new();

            int weightedSpawn = random.Next(1, _weightedSpawnSum.LastOrDefault());

            int requiredIndex = _weightedSpawnSum.BinarySearch(weightedSpawn);

            if (requiredIndex < 0)
            {
                requiredIndex = ~requiredIndex;
            }

            Console.WriteLine(requiredIndex);

            SpawnPoint spawnPoint = SpawnPoints[requiredIndex];

            FlightPlan fp = new FlightPlan(
                    callsign: GetRandomCallsign(spawnPoint.Callsigns, spawnPoint.CallsignPattern),
                    flightrules: "I",
                    atyp: GetRandomAircraftType(),
                    wtc: "X",
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
                    finalApchSpeed: 120,
                    minimumCleanSpeed: 230,
                    navigationData: _navData,
                    aircraftPerformance: GetAircraftPerformanceForType(fp.ATYP)
                ));
        }
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


    // The two functions below should have data included and parsed from external files. However, due to time pressure, they have been hard-coded here for the rest of the program to work
    private string GetRandomAircraftType()
    {
        Random random = new();
        string[] types = ["A20N", "B77W"];

        return types[random.Next(0, types.Length)];
    }

    private AircraftPerformance GetAircraftPerformanceForType(string atyp)
    {
        if (atyp == "A20N")
        {
            return new(290, 250, 250, 250, 290, 1500, 1000, 1000, 3000, 39000);
        }
        else
        {
            return new(300, 260, 260, 260, 300, 2000, 1500, 1000, 3000, 43000);
        }
    }
}