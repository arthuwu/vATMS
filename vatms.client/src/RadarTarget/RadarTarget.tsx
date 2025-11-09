import "./radarTarget.css"
import { getProjection } from '../MapLayers/mappingUtils';
import type { MapPropsInterface } from "../MapLayers/MapInterface";
import { useState } from "react";
import type { FlightPlan } from "../Types";

interface RadarTargetProps extends MapPropsInterface {
    flightPlan: FlightPlan;
    aircraft: any; //TODO: TMPY until radar correlation, because then we wouldnt have this, we'd only have the radar target
    lat: number;
    lon: number;
    datablockStatus: string;
    onClick: () => void;
}

const RadarTarget = ({ flightPlan, aircraft, lat, lon, zoom, projectionCenter, screenSize, datablockStatus, onClick }: RadarTargetProps) => {
    const [datablockOffset, setDatablockOffset] = useState<[x: number, y: number]>([0, 0]);
    const [radarTrail, setRadarTrail] = useState<[lat: number | null, lon: number | null][]>([[null, null]]);

    const calculatePixelCoordinate = ([point_lat, point_lon]: [number, number]) => {
        let [left_coord, top_coord]: [number, number] = getProjection([point_lat, point_lon], projectionCenter, zoom);
        left_coord += screenSize[0] / 2;
        top_coord += screenSize[1] / 2;

        return [left_coord, top_coord];
    }

    var [t_left_coord, t_top_coord] = calculatePixelCoordinate([lat, lon]);

    // this only rerenders every time the aircraft moves, or the screen is moved
    // so we need to check if the last saved position is the current position before adding to the trail
    // TODO: parameter for radar trail length

    if (radarTrail.length > 0) {
        //console.log(`lat: ${lat}, ${radarTrail[radarTrail.length - 1][0]} and lon: ${lon}, ${radarTrail[radarTrail.length - 1][1]}`)
        if (lat != radarTrail[radarTrail.length - 1][0] || lon != radarTrail[radarTrail.length - 1][1]) {
            let newCoord: [lat: number | null, lon: number | null] = [lat, lon];
            let newTrail = [...radarTrail, newCoord];

            if (radarTrail.length > (5 * 10))
                newTrail.shift();

            setRadarTrail(newTrail);
        }
    }

    const handleDatablockDragEnd = (e: React.DragEvent<HTMLDivElement>) => {
        e.preventDefault();
        setDatablockOffset([e.clientX - t_left_coord, e.clientY - t_top_coord])
    }

    const getLateralClearanceItem = (): string => {
        if (flightPlan.flightPlanAssignedData.ahdg)
            return "h" + flightPlan.flightPlanAssignedData.ahdg.toString().padStart(3, "0");

        if (flightPlan.flightPlanAssignedData.directtopoint)
            return flightPlan.flightPlanAssignedData.directtopoint;

        return "";
    }

    const getVerticalClearanceItem = (): string => {
        if (aircraft.pressureAltitude == flightPlan.flightPlanAssignedData.cfl) {
            return "";
        }

        return flightPlan.flightPlanAssignedData.cfl.toString().padStart(5, "0").substring(0, 3);
    }

    if (datablockStatus.includes("unconcerned")) {
        return (
            <div className={datablockStatus}>
                <div className="radar-target-symbol"
                    style={{ position: "absolute", top: t_top_coord, left: t_left_coord }}></div>
                <div className="target-data-block"
                    style={{ position: "absolute", top: t_top_coord + datablockOffset[1], left: t_left_coord + datablockOffset[0] }}
                    draggable="true"
                    onDragEnd={handleDatablockDragEnd}
                    onDragEnter={(e: React.DragEvent<HTMLDivElement>) => { e.preventDefault() }}
                    onDragOver={(e: React.DragEvent<HTMLDivElement>) => { e.preventDefault() }}
                    onClick={onClick}
                >
                    <div>{flightPlan.callsign}</div>
                    <div>{Math.trunc(aircraft.pressureAltitude).toString().padStart(5, "0").substring(0, 3)}</div>
                    <div>{flightPlan.intentionCode}</div>
                </div>
                {
                    radarTrail.map(([lat, lon], i) =>
                        i % 10 == 0 ?
                            <div className="radar-trail" style={{ position: "absolute", top: calculatePixelCoordinate([lat!, lon!])[1], left: calculatePixelCoordinate([lat!, lon!])[0] }}>
                                ⟋
                            </div>
                            :
                            null
                    )
                }
            </div>
        )
    } else {
        return (
            <div className={datablockStatus}>
                <div className="radar-target-symbol"
                    style={{ position: "absolute", top: t_top_coord, left: t_left_coord }}></div>
                <div className="target-data-block"
                    style={{ position: "absolute", top: t_top_coord + datablockOffset[1], left: t_left_coord + datablockOffset[0] }}
                    draggable="true"
                    onDragEnd={handleDatablockDragEnd}
                    onDragEnter={(e: React.DragEvent<HTMLDivElement>) => { e.preventDefault() }}
                    onDragOver={(e: React.DragEvent<HTMLDivElement>) => { e.preventDefault() }}
                    onClick={onClick}
                >
                    <div>{flightPlan.callsign}</div>
                    <div>
                        <span>{Math.trunc(aircraft.pressureAltitude).toString().padStart(5, "0").substring(0, 3)}</span>
                        &nbsp;
                        <span>{Math.trunc(flightPlan.flightPlanAssignedData.xfl).toString().padStart(5, "0").substring(0, 2)}</span>
                    </div>
                    <div>{flightPlan.intentionCode.padEnd(3, " ")}&nbsp;G{Math.trunc(aircraft.groundspeed).toString().padStart(3, "0").substring(0, 3)}</div>
                    <div>
                        <span>{getVerticalClearanceItem().padEnd(3, " ")}</span>
                        &nbsp;
                        <span>{getLateralClearanceItem()}</span>
                    </div>
                </div>
                {
                    radarTrail.map(([lat, lon], i) =>
                        i % 10 == 0 ?
                            <div className="radar-trail" style={{ position: "absolute", top: calculatePixelCoordinate([lat!, lon!])[1], left: calculatePixelCoordinate([lat!, lon!])[0] }}>
                                ⟋
                            </div>
                            :
                            null
                    )
                }
            </div>
        )
    }
}

export default RadarTarget;

// TODO: line between tag and target - to be done with other visuals
