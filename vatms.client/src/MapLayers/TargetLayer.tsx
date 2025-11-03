/* This is a fake canvas - it is a div that overlays the canvas with the exact same dimensions, so we can put some absolute divs on it */

import './maps.css';
import SignalRManager from '../SignalR/SignalRManager';
import { useEffect, useState } from 'react';
import RadarTarget from '../RadarTarget/RadarTarget';
import type { MapPropsInterface } from './MapInterface';
import RouteDrawLayer from './RouteDrawLayer';
//import RadarTarget from '../RadarTarget/RadarTarget';

interface TargetLayerMapProps extends MapPropsInterface {
    routeDraw: boolean;
}

const TargetLayer = ({ projectionCenter, zoom, screenSize, routeDraw }: TargetLayerMapProps) => {
    const [targetList, setTargetList] = useState<any>();
    const [selectedAircraftID, setSelectedAircraftID] = useState<string>();
    const [conflictList, setConflictList] = useState<string[]>([]);

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
        if (selectedAircraftID)
            signalRManager.send("SetSectorSelectedAircraftID", "sector", selectedAircraftID); // TODO: replace sector with the actual sector string once sectorisation logic is in place
    }, [selectedAircraftID])

    useEffect(() => {
        signalRManager.on("sectorSelectedId", (sectorId: string, selectedAircraftId: string) => {
            if (sectorId)
                setSelectedAircraftID(selectedAircraftId);
        })
    }, [signalRManager])

    useEffect(() => {
        signalRManager.on("conflictPairsUpdate", (pairs: [any, any][]) => {
            const newConflictList: string[] = [];
            pairs.forEach((pair: any) => {
                const { item1: ac1, item2: ac2 } = pair;
                if (ac1.flightPlan != null)
                    newConflictList.push(ac1.flightPlan.callsign);
                if (ac2.flightPlan != null)
                    newConflictList.push(ac2.flightPlan.callsign);
            })

            setConflictList(newConflictList);
        })
    }, [signalRManager])

    const getDatablockStatus = (target: any): string => {
        var datablockStatus: string[] = [];

        if (selectedAircraftID == target.flightPlan.callsign) datablockStatus.push("selected");

        if (conflictList.includes(target.flightPlan.callsign)) datablockStatus.push("predicted-conflict");

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

/*
[
    {
        "radarTarget": {
            "id": "eec16941-9abe-440c-8934-fd0152e22e65",
            "correlatedFlightPlan": null
        },
        "flightPlan": {
            "callsign": "CPA253",
            "flightRules": "I",
            "atyp": "A35K",
            "wtc": "H",
            "equipment": "SABDE2E3GHIJ1J2J3J4J5LM1OP2RVWXYZ/LB1D1G1",
            "dep": "VHHH",
            "dest": "EGLL",
            "eobt": 655,
            "eet": 1245,
            "altn": "EHAM",
            "rmk": "",
            "flightPlanAssignedData": {
                "cfl": 0,
                "asp": 0,
                "ahdg": 0,
                "avs": null
            },
            "testingValue": 0
        },
        "position": {
            "lat": 51.4775,
            "lon": -0.461389
        },
        "heading": 270,
        "pressureAltitude": 130,
        "indicatedAirspeed": 260,
        "verticalSpeed": 0
    }
]
*/