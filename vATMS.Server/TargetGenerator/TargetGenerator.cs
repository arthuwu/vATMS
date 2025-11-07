using System.Collections.Concurrent;
using vATMS.Aircraft;
using vATMS.NavData;
using Newtonsoft.Json;

class TargetGenerator
{
    public int Rate { get; set; } // aircraft per hour - will be variable (not exact)
    public int MinimumLevel { get; init; }
    public int MaximumLevel { get; init; }
    public DecimalCoordinate SimulationCentre { get; init; }
    public int SpawnRadius { get; init; }

    private readonly IWebHostEnvironment _webEnv;
    private readonly NavigationData _navData;
    private readonly System.Threading.Timer _timer;

    public List<SpawnPoint> SpawnPoints { get; set; }
    private ConcurrentDictionary<string, Aircraft> _aircraft;

    private List<int> _weightedSpawnSum = new();

    public TargetGenerator(IWebHostEnvironment webEnv, int rate, ConcurrentDictionary<string, Aircraft> aircraft, NavigationData navData)
    {
        Rate = rate;
        _webEnv = webEnv;
        _navData = navData;
        _aircraft = aircraft;

        using (var fs = new FileStream(Path.Combine(_webEnv.WebRootPath, "TargetGenerator\\TargetGeneration.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
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

        SpawnAircraft(null);

        _timer = new System.Threading.Timer(SpawnAircraft, null, Timeout.Infinite, Timeout.Infinite); // initialise "paused" timer

        setupTimer(rate, _navData);
    }

    public void setupTimer(int rate, NavigationData navData)
    {
        Console.WriteLine($"rate {rate}");
        // rough derivation of spawn rate
        //int spawnInterval = (3600000 / rate) / (_navData.SectorList.Select(s => s.Value.IsUserControlled() == true).ToList().Count() / _navData.SectorList.Count());

        int spawnInterval = 1000;

        _timer.Change(spawnInterval, spawnInterval);
    }

    public void SpawnAircraft(object? state)
    {
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
                equipment: "SABDE2E3GHIJ1J2J3J4J5LM1OP2RVWXYZ/LB1D1G1",
                dep: spawnPoint.ADEP[random.Next(spawnPoint.ADEP.Count())],
                dest: spawnPoint.ADES[random.Next(spawnPoint.ADES.Count())],
                eobt: 0000,
                eet: 0000,
                altn: "ZZZZ",
                rmk: "",
                rfl: 40000, //TODO: FIX
                spd: GetRandomSpeed(),
                route: _navData.ParseRoute(spawnPoint.FlownRoute),
                displayRoute: spawnPoint.DisplayRoute,
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
                indicatedAirspeed: GetSpawnSpeed(),
                finalApchSpeed: 120,
                minimumCleanSpeed: 230,
                navigationData: _navData
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

        return callsign;
    }

    private string GetRandomAircraftType()
    {
        return "A20N";
    }

    private int GetRandomRFL(int min, int max)
    {
        Random random = new();
        return random.Next(min % 1000 / 2, max % 1000 / 2) * 2000;
    }

    private int GetRandomSpeed()
    {
        Random random = new();
        return random.Next(340, 550);
    }

    private int GetSpawnSpeed()
    {
        return 300;
    }
}