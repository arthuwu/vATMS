//import type { FlightPlanAssignedData } from '../RadarTarget/Types';
import { useMemo } from 'react';
import { useSearchParams } from "react-router-dom";
import SignalRManager from '../SignalR/SignalRManager';
import { useEffect, useState } from 'react';
import HorizontalSelect from './HorizontalSelect';

/*
Component representing the iFACTS selection window
*/
const SelectionWindow = () => {
    const [targetList, setTargetList] = useState<any>([]);
    const [selectedAircraftID, setSelectedAircraftID] = useState<any>();

    const [searchParams] = useSearchParams();
    const initialSector = useMemo(() => searchParams.get("sector") ?? "", [searchParams]);
    const [sectorId] = useState<string>(initialSector);

    const [sectorProps, setSectorProps] = useState<any>([]);

    const [headingSelectList, setHeadingSelectList] = useState<string[]>(Array(9).fill(""));;
    const [waypointSelectList, setWaypointSelectList] = useState<string[]>(Array(8).fill(""));
    const [levelSelectList, setLevelSelectList] = useState<number[]>([]);

    const [speedToggle, setSpeedToggle] = useState<boolean>(false);

    const [incommList, setIncommList] = useState<any>([]);
    const [pendingList, setPendingList] = useState<any>([]);

    const signalRManager: SignalRManager = SignalRManager.getInstance();

    useEffect(() => {
        const AircraftPositionUpdateHandler = (ac: any) => {
            //console.log(ac);
            setTargetList(ac);

            //console.log(ac.filter((e: any) => sectorProps.sectorVolumes?.includes(e.trajectory.currentSector)))
            setIncommList(ac.filter((e: any) => sectorProps.sectorVolumes?.includes(e.trajectory.currentSector?.id)))
            setPendingList(ac.filter((e: any) => sectorProps.sectorVolumes?.includes(e.trajectory.nextSector?.id)))

        }
        signalRManager.on("AircraftPositionUpdate", AircraftPositionUpdateHandler)

        return () => {
            signalRManager.off("AircraftPositionUpdate", AircraftPositionUpdateHandler)
        }
    }, [signalRManager, sectorProps])

    useEffect(() => {
        signalRManager.send("SetSectorSelectedAircraftID", sectorId, selectedAircraftID);
    }, [selectedAircraftID])

    useEffect(() => {
        signalRManager.on("sectorSelectedId", (sectorId: string, selectedAircraftId: string) => {
            if (sectorId)
                setSelectedAircraftID(selectedAircraftId);
        })
    }, [signalRManager])

    useEffect(() => { // generates levels from FL190 to FL430 
        const levels: number[] = [];
        for (var i: number = 43000; i >= 19000; i -= 1000) {
            levels.push(i);
        }

        setLevelSelectList(levels);
    }, [])

    useEffect(() => {
        const fetchSectors = async () => {
            const res = await fetch(`${window.location.origin}/api/get-available-sectors`);
            const json = await res.json();
            setSectorProps(json.logons.find((e: any) => e.id == sectorId));
        }

        fetchSectors();
    }, [sectorId])

    const getSelectedAircraft: any = () => {
        return targetList?.find((target: any) => target.flightPlan.callsign === selectedAircraftID);
    }

    const handleSelectedAircraftAssignedDataChange: any = (aircraftId: string, data: string, value: string) => {
        if (aircraftId && value) {
            if (sectorProps.sectorVolumes.includes(getSelectedAircraft()?.trajectory.currentSector?.id)) // only allow assumed aircraft to be controlled
                signalRManager.send("SetSelectedAircraftAssignedData", aircraftId, data, value)
        }
    }

    const handleSpeedToggle = (): void => {
        setSpeedToggle((prev: any) => !prev)
    }

    useEffect(() => {
        if (getSelectedAircraft() != null) {
            let newString: string = getSelectedAircraft()?.flightPlan.route.map((e: any) => e.name).toString();
            let oldString: string = waypointSelectList.toString()
            if (newString != oldString) { // comparing without .toString() gives an incorrect answer
                setWaypointSelectList(getSelectedAircraft()?.flightPlan.route.map((e: any) => e.name))
            }
        }
    }, [targetList, selectedAircraftID])

    useEffect(() => { // generates headings which "wrap around" the present heading of the aircraft
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

    return (
        <>
            <div className='ifacts-selection-window'>
                <div className='left'>
                    <div className='selected-aircraft-window'></div>
                    <div className='lad'>
                        <div className='lad-display'>
                        </div>
                        <div className='vertical-selection'>
                            {
                                levelSelectList.map((e: number) => (
                                    <div onClick={() => handleSelectedAircraftAssignedDataChange(
                                        selectedAircraftID,
                                        "CFL",
                                        e.toString())
                                    }>{e.toString().substring(0, 3)}</div>
                                ))
                            }
                        </div>
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
                        {speedToggle ? <input
                            type="text"
                            onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                                if (e.key === "Enter") {
                                    e.preventDefault();

                                    if (e.currentTarget.value == "") {
                                        handleSelectedAircraftAssignedDataChange(
                                            selectedAircraftID,
                                            "ASP",
                                            "0"
                                        );
                                    } else {
                                        handleSelectedAircraftAssignedDataChange(
                                            selectedAircraftID,
                                            "ASP",
                                            e.currentTarget.value
                                        );
                                    }

                                    e.currentTarget.value = ""; //temp reset anyways
                                    handleSpeedToggle();
                                } else if (e.key === "Escape") {
                                    e.preventDefault();
                                    handleSpeedToggle();
                                }
                            }}
                        />
                            : <div onClick={handleSpeedToggle}>{getSelectedAircraft()?.flightPlan.flightPlanAssignedData.asp}</div>
                        }
                        <div className='ifacts-button enter-button'></div>
                        {/*<div className='ifacts-button enter-button'>ENTER</div>*/}
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
                            {incommList?.map((target: any) => (
                                <div className={(selectedAircraftID == target.flightPlan.callsign) ? "bay-strip selected" : "bay-strip"}
                                    onClick={() => { setSelectedAircraftID(target.flightPlan.callsign) }}>
                                    <div className='bay-strip-callsign'>{target.flightPlan.callsign}</div>
                                    <div>{target.flightPlan.flightPlanAssignedData.xfl.toString().padStart(5, "0").substring(0, 3)}</div>
                                    <div></div>
                                    <div></div>
                                    <div>{target.flightPlan.intentionCode}</div>
                                </div>)
                            )}
                        </div>
                        <div className='pending-bay'>
                            {pendingList?.map((target: any) => (
                                <div className={(selectedAircraftID == target.flightPlan.callsign) ? "bay-strip-short selected" : "bay-strip-short"}
                                    onClick={() => { setSelectedAircraftID(target.flightPlan.callsign) }}>
                                    <div className='bay-strip-callsign'>{target.flightPlan.callsign}</div>
                                    <div>{target.flightPlan.flightPlanAssignedData.xfl.toString().padStart(5, "0").substring(0, 3)}</div>
                                    <div></div>
                                    <div>{target.flightPlan.intentionCode}</div>
                                </div>)
                            )}
                        </div>
                    </div>
                </div>
            </div >
        </>
    )
}

export default SelectionWindow
