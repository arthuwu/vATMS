import "./radarTarget.css"
import { getOffsetProjection } from '../MapLayers/mappingUtils';
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


    var [t_left_coord, t_top_coord] = getOffsetProjection([lat, lon], projectionCenter, zoom, screenSize);

    // this only rerenders every time the aircraft moves, or the screen is moved
    // so we need to check if the last saved position is the current position before adding to the trail

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
        if (flightPlan.flightPlanAssignedData.ahdg == 0) {
            return "h360"
        } else if (flightPlan.flightPlanAssignedData.ahdg != null) {
            return "h" + flightPlan.flightPlanAssignedData.ahdg.toString().padStart(3, "0");
        }

        if (flightPlan.flightPlanAssignedData.directToPoint)
            return flightPlan.flightPlanAssignedData.directToPoint;

        return "";
    }

    const getVerticalClearanceItem = (): string => {
        if (aircraft.pressureAltitude == flightPlan.flightPlanAssignedData.cfl) {
            return "   ";
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
                            <div className="radar-trail" style={{
                                position: "absolute",
                                top: getOffsetProjection([lat!, lon!], projectionCenter, zoom, screenSize)[1],
                                left: getOffsetProjection([lat!, lon!], projectionCenter, zoom, screenSize)[0]
                            }}>
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
                        <span className={aircraft.pressureAltitude != flightPlan.flightPlanAssignedData.xfl ? "rfl-highlight" : ""}>{Math.trunc(flightPlan.flightPlanAssignedData.xfl).toString().padStart(5, "0").substring(0, 2)}</span>
                    </div>
                    <div>{flightPlan.intentionCode.padEnd(3, " ")}&nbsp;G{Math.trunc(aircraft.groundspeed).toString().padStart(3, "0").substring(0, 3)}</div>
                    <div>
                        <span>{getVerticalClearanceItem()}</span>
                        &nbsp;
                        <span>{getLateralClearanceItem()}</span>
                    </div>
                </div>
                {
                    radarTrail.map(([lat, lon], i) =>
                        i % 10 == 0 ?
                            <div className="radar-trail" style={{
                                position: "absolute",
                                top: getOffsetProjection([lat!, lon!], projectionCenter, zoom, screenSize)[1],
                                left: getOffsetProjection([lat!, lon!], projectionCenter, zoom, screenSize)[0]
                            }}>
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
