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
    flightPlanAssignedData: FlightPlanAssignedData,
    intentionCode: string
}

export type FlightPlanAssignedData = {
    cfl: number,
    asp: number,
    ahdg: number,
    avs: number,
    directtopoint: string,
}
