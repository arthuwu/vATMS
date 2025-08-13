/* This is a fake canvas - it is a div that overlays the canvas with the exact same dimensions, so we can put some absolute divs on it */

import './maps.css';
import SignalRManager from '../SignalR/SignalRManager';
import { useEffect, useState } from 'react';
import RadarTarget from '../RadarTarget/RadarTarget';
import type { MapPropsInterface } from './MapInterface';
//import RadarTarget from '../RadarTarget/RadarTarget';


const TargetLayer = ({ projectionCenter, zoom, screenSize }: MapPropsInterface) => {
    const [targetList, setTargetList] = useState<any>();
    const [selectedAircraftID, setSelectedAircraftID] = useState<any>();

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
        signalRManager.send("SetSectorSelectedAircraftID", "sector", selectedAircraftID); // TODO: replace sector with the actual sector string once sectorisation logic is in place
    }, [selectedAircraftID])

    useEffect(() => {
        signalRManager.on("sectorSelectedId", (sectorId: string, selectedAircraftId: string) => {
            if (sectorId)
                setSelectedAircraftID(selectedAircraftId);
        })
    }, [signalRManager])

    /*
    useEffect(() => {
        console.log(targetList);
    }, [targetList])
    */

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
                        onClick={() => { setSelectedAircraftID(target.flightPlan.callsign) }}
                    ></RadarTarget>
                ))}
            </div>
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