export type FlightPlan = {
    callsign: string,
    flightRules: string,
    atyp: string,
    wtc: string,
    equipment: string,
    dep: string,
    dest: string,
    eobt: number,
    eet: number,
    altn: string,
    rmk: string,
    rfl: number,
    spd: number,
    flightPlanAssignedData: {
        cfl: number,
        asp: number,
        ahdg: number,
        avs: number,
    },
    testingValue: number
}

