//import type { FlightPlanAssignedData } from '../RadarTarget/Types';
import { useMemo } from 'react';
import { useSearchParams } from "react-router-dom";
import SignalRManager from '../SignalR/SignalRManager';
import { useEffect, useState } from 'react';

const SelectionWindow = () => {
    const [targetList, setTargetList] = useState<any>();
    const [selectedAircraftID, setSelectedAircraftID] = useState<any>();
    // const [selectedAircraftAssignedData, setSelectedAircraftAssignedData] = useState<FlightPlanAssignedData>();

    const [searchParams] = useSearchParams();
    const initialSector = useMemo(() => searchParams.get("sector") ?? "", [searchParams]);
    const [sectorId] = useState<string>(initialSector);

    const signalRManager: SignalRManager = SignalRManager.getInstance();

    useEffect(() => {
        const AircraftPositionUpdateHandler = (ac: any) => {
            //console.log(ac);
            setTargetList(ac);
        }
        signalRManager.on("AircraftPositionUpdate", AircraftPositionUpdateHandler)

        return () => {
            signalRManager.off("AircraftPositionUpdate", AircraftPositionUpdateHandler)
        }
    }, [signalRManager])

    useEffect(() => {
        signalRManager.send("SetSectorSelectedAircraftID", sectorId, selectedAircraftID); // TODO: replace sector with the actual sector string once sectorisation logic is in place
    }, [selectedAircraftID])

    useEffect(() => {
        signalRManager.on("sectorSelectedId", (sectorId: string, selectedAircraftId: string) => {
            if (sectorId)
                setSelectedAircraftID(selectedAircraftId);
        })
    }, [signalRManager])

    const getSelectedAircraft: any = () => {
        return targetList?.find((target: any) => target.flightPlan.callsign === selectedAircraftID);
    }

    const handleSelectedAircraftAssignedDataChange: any = (aircraftId: string, data: string, value: string) => {
        signalRManager.send("SetSelectedAircraftAssignedData", aircraftId, data, value)
    }

    const getRoute = (aircraft: any) => {
        if (aircraft) {
            let routeString: string = "";
            let routeQueue: any = aircraft.flightPlan.route;
            routeQueue.forEach((e: any) => {
                routeString += ` ${e.name}`;
            });
            console.log(aircraft);
            return routeString;
        }
        return ""
    }

    return (
        <>
            <div>Selection Window {sectorId}</div>
            <div className='temp-target-list'>
                {targetList?.map((target: any) => (
                    <div>{target.flightPlan.callsign}</div>
                ))}
            </div>
            <div>Selected callsign: {selectedAircraftID}</div>
            <div>CFL: {getSelectedAircraft()?.flightPlan.flightPlanAssignedData.cfl}</div>
            <input
                type="text"
                onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                    if (e.key === "Enter") {
                        e.preventDefault();
                        handleSelectedAircraftAssignedDataChange(
                            selectedAircraftID,
                            "CFL",
                            e.currentTarget.value
                        );
                        e.currentTarget.value = ""; //temp reset anyways
                    }
                }}
            />
            <div>AHDG: {getSelectedAircraft()?.flightPlan.flightPlanAssignedData.ahdg}</div>
            <input
                type="text"
                onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                    if (e.key === "Enter") {
                        e.preventDefault();
                        handleSelectedAircraftAssignedDataChange(
                            selectedAircraftID,
                            "AHDG",
                            e.currentTarget.value
                        );
                        e.currentTarget.value = ""; //temp reset anyways
                    }
                }}
            />
            <div>ASP: {getSelectedAircraft()?.flightPlan.flightPlanAssignedData.asp}</div>
            <input
                type="text"
                onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                    if (e.key === "Enter") {
                        e.preventDefault();
                        handleSelectedAircraftAssignedDataChange(
                            selectedAircraftID,
                            "ASP",
                            e.currentTarget.value
                        );
                        e.currentTarget.value = ""; //temp reset anyways
                    }
                }}
            />
            <div>Route: {getRoute(getSelectedAircraft())}</div>
            <input
                type="text"
                onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                    if (e.key === "Enter") {
                        e.preventDefault();
                        handleSelectedAircraftAssignedDataChange(
                            selectedAircraftID,
                            "DirectToPoint",
                            e.currentTarget.value
                        );
                        e.currentTarget.value = ""; //temp reset anyways
                    }
                }}
            />
        </>
    )
}

export default SelectionWindow
