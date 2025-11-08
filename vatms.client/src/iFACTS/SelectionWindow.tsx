//import type { FlightPlanAssignedData } from '../RadarTarget/Types';
import { useMemo } from 'react';
import { useSearchParams } from "react-router-dom";
import SignalRManager from '../SignalR/SignalRManager';
import { useEffect, useState } from 'react';
import HorizontalSelect from './HorizontalSelect';

const SelectionWindow = () => {
    const [targetList, setTargetList] = useState<any>();
    const [selectedAircraftID, setSelectedAircraftID] = useState<any>();
    // const [selectedAircraftAssignedData, setSelectedAircraftAssignedData] = useState<FlightPlanAssignedData>();

    const [searchParams] = useSearchParams();
    const initialSector = useMemo(() => searchParams.get("sector") ?? "", [searchParams]);
    const [sectorId] = useState<string>(initialSector);

    const [headingSelectList, setHeadingSelectList] = useState<string[]>(Array(9).fill(""));
    const [waypointSelectList, setWaypointSelectList] = useState<string[]>(Array(8).fill(""));

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

    useEffect(() => {
        if (getSelectedAircraft()?.flightPlan.route.map((e: any) => e.name) != waypointSelectList) {
            setWaypointSelectList(getSelectedAircraft()?.flightPlan.route.map((e: any) => e.name))
        }
    }, [targetList, selectedAircraftID])

    useEffect(() => {
        if (getSelectedAircraft()) {
            let presentHeading: number = Math.round(getSelectedAircraft().track / 5) * 5;

            let headingList: string[] = [];

            for (var i: number = -4; i <= 64; i++) {
                headingList.push(((presentHeading + (i * 5) + 360) % 360).toString().padStart(3, "0"));
            }

            setHeadingSelectList(headingList);
        } else {
            setHeadingSelectList(Array(9).fill(""));
        }

    }, [selectedAircraftID]);

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
            <div className='ifacts-selection-window'>
                <div className='left'>
                    <div className='selected-aircraft-window'></div>
                    <div className='lad'>
                        <div className='lad-display'>
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
                        </div>
                        <div className='vertical-selection'></div>
                    </div>
                </div>
                <div className='right'>
                    <div className='selected-aircraft-data'>
                        <div>CLLSIGN</div>
                        <div>TYP</div>
                        <div>CFL</div>
                        <div>XFL</div>
                        <div>HDG</div>
                        <div>ROUTE</div>
                        <div>SPD</div>
                        <div>{selectedAircraftID}</div>
                        <div>{getSelectedAircraft()?.flightPlan.atyp}</div>
                        <div>{getSelectedAircraft()?.flightPlan.flightPlanAssignedData.cfl.toString().padStart(5, "0").substring(0, 3)}</div>
                        <div>{getSelectedAircraft()?.flightPlan.flightPlanAssignedData.xfl.toString().padStart(5, "0").substring(0, 3)}</div>
                        <div>{getSelectedAircraft()?.flightPlan.flightPlanAssignedData.ahdg?.toString().padStart(3, "0")}</div>
                        <div>{getSelectedAircraft()?.flightPlan.flightPlanAssignedData.directToPoint}</div>
                        <div>{getSelectedAircraft()?.flightPlan.flightPlanAssignedData.asp}</div>
                        <div className='ifacts-button enter-button'>ENTER</div>
                    </div>
                    <div className='lateral-selection'>
                        <HorizontalSelect
                            elementList={headingSelectList}
                            elementWidth={"3em"}
                            length={9}
                            onElementSelect={(value: string) => handleSelectedAircraftAssignedDataChange(
                                selectedAircraftID,
                                "AHDG",
                                value
                            )}
                        ></HorizontalSelect>
                        <HorizontalSelect
                            elementList={waypointSelectList}
                            elementWidth={"5em"}
                            length={8}
                            onElementSelect={(value: string) => handleSelectedAircraftAssignedDataChange(
                                selectedAircraftID,
                                "DirectToPoint",
                                value
                            )}
                        ></HorizontalSelect>
                    </div>
                    <div className='fpl-bays'>
                        <div className='incomm-bay'>
                            {targetList?.map((target: any) => (
                                <div className={(selectedAircraftID == target.flightPlan.callsign) ? "bay-strip selected" : "bay-strip"}>{target.flightPlan.callsign}</div>
                            ))}
                        </div>
                        <div className='pending-bay'></div>
                    </div>
                </div>
            </div >
        </>
    )
}

export default SelectionWindow
