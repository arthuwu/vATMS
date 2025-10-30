using vATMS.Aircraft;

class TargetGenerator
{
    public int Rate { get; set; } // aircraft per hour - will be variable (not exact)
    public int PilotQuality { get; set; }

    public SpawnPoint[] SpawnPoints { get; set; }
    private List<Aircraft> _aircraft;

    public TargetGenerator(int rate, int pilotQuality, List<Aircraft> aircraft, NavigationData navData)
    {
        Rate = rate;
        PilotQuality = pilotQuality;
        SpawnPoints = [new SpawnPoint(
            coordinate: new DecimalCoordinate(51.4775, -0.461389),
            availableLevels: [130],
            initialTrack: 270
        ), new SpawnPoint(
            coordinate: new DecimalCoordinate(51.885, 0.235),
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
            spd: 543,
            route: navData.ParseRoute("ULTIB BNN UTAVA BKY LAM")
        );

        _aircraft.Add(new Aircraft(
            flightPlan: fp,
            position: SpawnPoints[0].Coordinate,
            track: SpawnPoints[0].InitialTrack,
            pressureAltitude: SpawnPoints[0].getSpawnLevel(),
            indicatedAirspeed: 260,
            finalApchSpeed: 120,
            minimumCleanSpeed: 230,
            navigationData: navData
        ));

        FlightPlan fp2 = new FlightPlan(
            callsign: "BAW123N",
            flightrules: "I",
            atyp: "A320",
            wtc: "H",
            equipment: "SABDE2E3GHIJ1J2J3J4J5LM1OP2RVWXYZ/LB1D1G1",
            dep: "VHHH",
            dest: "EGLL",
            eobt: 0655,
            eet: 1245,
            altn: "EHAM",
            rmk: "",
            rfl: 34000,
            spd: 543,
            route: navData.ParseRoute("BPK BNN")
        );

        _aircraft.Add(new Aircraft(
            flightPlan: fp2,
            position: SpawnPoints[1].Coordinate,
            track: SpawnPoints[1].InitialTrack,
            pressureAltitude: SpawnPoints[1].getSpawnLevel(),
            indicatedAirspeed: 260,
            finalApchSpeed: 120,
            minimumCleanSpeed: 230,
            navigationData: navData
        ));

        FlightPlan fp3 = new FlightPlan(
            callsign: "SHT3N",
            flightrules: "I",
            atyp: "A21N",
            wtc: "M",
            equipment: "SABDE2E3GHIJ1J2J3J4J5LM1OP2RVWXYZ/LB1D1G1",
            dep: "VHHH",
            dest: "EGLL",
            eobt: 0655,
            eet: 1245,
            altn: "EHAM",
            rmk: "",
            rfl: 34000,
            spd: 543,
            route: navData.ParseRoute("UMLAT WOBUN DTY SILVA")
        );

        _aircraft.Add(new Aircraft(
            flightPlan: fp3,
            position: SpawnPoints[1].Coordinate,
            track: SpawnPoints[1].InitialTrack,
            pressureAltitude: SpawnPoints[1].getSpawnLevel(),
            indicatedAirspeed: 260,
            finalApchSpeed: 120,
            minimumCleanSpeed: 230,
            navigationData: navData
        ));

        Console.WriteLine("DEBUG: new TargetGenerator");
    }

}