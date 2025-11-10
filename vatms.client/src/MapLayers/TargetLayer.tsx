/* This is a fake canvas - it is a div that overlays the canvas with the exact same dimensions, so we can put some absolute divs on it */

import './maps.css';
import SignalRManager from '../SignalR/SignalRManager';
import { useEffect, useState } from 'react';
import RadarTarget from '../RadarTarget/RadarTarget';
import type { MapPropsInterface } from './MapInterface';
import RouteDrawLayer from './RouteDrawLayer';
import type { PredictedConflictPair } from "../Types";

interface TargetLayerMapProps extends MapPropsInterface {
    routeDraw: boolean;
    sectorId: string;
    sectorVolumes: string[];
}

const TargetLayer = ({ projectionCenter, zoom, screenSize, routeDraw, sectorId, sectorVolumes }: TargetLayerMapProps) => {
    const [targetList, setTargetList] = useState<any>();
    const [selectedAircraftID, setSelectedAircraftID] = useState<string>();
    const [conflictList, setConflictList] = useState<PredictedConflictPair[]>([]);
    const [shortTermConflictList, setShortTermConflictList] = useState<string[]>([]);
    const [aircraftThroughSector, setAircraftThroughSector] = useState<string[]>([]);

    const signalRManager: SignalRManager = SignalRManager.getInstance();

    useEffect(() => {
        const AircraftPositionUpdateHandler = (ac: any) => {
            setTargetList(ac);
            ac.forEach((e: any) => {
                if (sectorVolumes.includes(e.trajectory.currentSector))
                    setAircraftThroughSector((prev: any) => [...prev, e.flightPlan.callsign])
            });
        }
        signalRManager.on("AircraftPositionUpdate", AircraftPositionUpdateHandler)

        return () => {
            signalRManager.off("AircraftPositionUpdate", AircraftPositionUpdateHandler)
        }
    }, [signalRManager])

    useEffect(() => {
        if (selectedAircraftID)
            signalRManager.send("SetSectorSelectedAircraftID", sectorId, selectedAircraftID);
    }, [selectedAircraftID])

    useEffect(() => {
        signalRManager.on("sectorSelectedId", (sectorId: string, selectedAircraftId: string) => {
            if (sectorId)
                setSelectedAircraftID(selectedAircraftId);
        })
    }, [signalRManager])

    useEffect(() => {
        signalRManager.on("conflictPairsUpdate", (pairs: PredictedConflictPair[]) => {
            setConflictList(pairs);
        })
    }, [signalRManager])

    useEffect(() => {
        signalRManager.on("shortTermConflictPairsUpdate", (pairs: string[]) => {
            setShortTermConflictList(pairs);
        })
    }, [signalRManager])

    const getDatablockStatus = (target: any): string => {
        var datablockStatus: string[] = [];

        if (selectedAircraftID == target.flightPlan.callsign)
            datablockStatus.push("selected");

        if (shortTermConflictList.includes(target.flightPlan.callsign)) {
            datablockStatus.push("short-term-conflict");
        } else if (conflictList.map((e: PredictedConflictPair) => e.callsign1).includes(target.flightPlan.callsign) ||
            conflictList.map((e: PredictedConflictPair) => e.callsign2).includes(target.flightPlan.callsign)
        ) {
            datablockStatus.push("predicted-conflict");
        }

        if (sectorVolumes.includes(target.trajectory.currentSector)) {
            datablockStatus.push("assumed");
        } else if (sectorVolumes.includes(target.trajectory.nextSector)) {
            datablockStatus.push("concerned");
        } else if (aircraftThroughSector.includes(target.callsign) && !sectorVolumes.includes(target.trajectory.currentSector)) {
            datablockStatus.push("redundant");
        } else {
            datablockStatus.push("unconcerned")
        }

        return datablockStatus.join(" ")
    }

    return (
        <>
            <div className="target-layer">
                {targetList?.map((target: any) => (
                    <RadarTarget
                        key={target.radarTarget.id}
                        flightPlan={target.flightPlan}
                        aircraft={target}
                        lat={target.position.lat}
                        lon={target.position.lon}
                        projectionCenter={projectionCenter}
                        zoom={zoom}
                        screenSize={screenSize}
                        datablockStatus={getDatablockStatus(target)}
                        onClick={() => { setSelectedAircraftID(target.flightPlan.callsign) }}
                    ></RadarTarget>
                ))}
            </div>
            <RouteDrawLayer projectionCenter={projectionCenter} zoom={zoom} screenSize={[screenSize[0], screenSize[1]]}
                selectedAircraft={targetList ? targetList.find((ac: any) => ac.flightPlan.callsign === selectedAircraftID) : null}
                routeDraw={routeDraw}></RouteDrawLayer >
        </>
    )
}

export default TargetLayer