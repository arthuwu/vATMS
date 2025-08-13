import "./radarTarget.css"
import { getProjection } from '../MapLayers/mappingUtils';
import type { MapPropsInterface } from "../MapLayers/MapInterface";
import { useState } from "react";
import type { FlightPlan } from "./Types";

interface RadarTargetProps extends MapPropsInterface {
    flightPlan: FlightPlan;
    aircraft: any; //TODO: TMPY until radar correlation, because then we wouldnt have this, we'd only have the radar target
    lat: number;
    lon: number;
    onClick: () => void;
}

const RadarTarget = ({ flightPlan, aircraft, lat, lon, zoom, projectionCenter, screenSize, onClick }: RadarTargetProps) => {
    const [datablockOffset, setDatablockOffset] = useState<[x: number, y: number]>([0, 0]);

    let [left_coord, top_coord]: [number, number] = getProjection([lat, lon], projectionCenter, zoom);
    left_coord += screenSize[0] / 2;
    top_coord += screenSize[1] / 2;

    const handleDatablockDragEnd = (e: React.DragEvent<HTMLDivElement>) => {
        e.preventDefault();
        setDatablockOffset([e.clientX - left_coord, e.clientY - top_coord])
    }

    return (
        <>
            <div className={"radar-target-symbol"}
                style={{ position: "relative", top: top_coord, left: left_coord }}></div>
            <div className={"target-data-block"}
                style={{ position: "relative", top: top_coord + datablockOffset[1], left: left_coord + datablockOffset[0] }}
                draggable="true"
                onDragEnd={handleDatablockDragEnd}
                onDragEnter={(e: React.DragEvent<HTMLDivElement>) => { e.preventDefault() }}
                onDragOver={(e: React.DragEvent<HTMLDivElement>) => { e.preventDefault() }}
                onClick={onClick}
            >
                <div>{flightPlan.callsign}</div>
                <div>{aircraft.pressureAltitude.toString().substring(0, 3)}&nbsp;{flightPlan.flightPlanAssignedData.cfl.toString().substring(0, 2)}</div>
                <div>{"XX"}&nbsp;&nbsp;G{aircraft.groundspeed.toString().substring(0, 3)}</div>
                <div>{flightPlan.rfl.toString().substring(0, 3)}&nbsp;{"XXXXX"}</div>
            </div>
        </>
    )
}

export default RadarTarget;

// TODO: line between tag and target - to be done with other visuals

