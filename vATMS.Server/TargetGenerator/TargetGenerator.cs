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
        SpawnPoints = [new SpawnPoint(
            coordinate: new DecimalCoordinate(51.4775, -0.461389),
            availableLevels: [130],
            initialTrack: 270
        )];
        _aircraft = aircraft;

        FlightPlan fp = new FlightPlan(
            callsign: "CPA253",
            flightrules: "I",
            atyp: "A35K",
            wtc: "H",
            equipment: "SABDE2E3GHIJ1J2J3J4J5LM1OP2RVWXYZ/LB1D1G1",
            dep: "VHHH",
            dest: "EGLL",
            eobt: 0655,
            eet: 1245,
            altn: "EHAM",
            rmk: "",
            rfl: 34000,
            spd: 543
        );

        _aircraft.Add(new Aircraft(
            flightPlan: fp,
            position: SpawnPoints[0].Coordinate,
            track: SpawnPoints[0].InitialTrack,
            pressureAltitude: SpawnPoints[0].getSpawnLevel(),
            indicatedAirspeed: 260,
            finalApchSpeed: 120,
            minimumCleanSpeed: 230
        ));

        Console.WriteLine("DEBUG: new TargetGenerator");
    }

}