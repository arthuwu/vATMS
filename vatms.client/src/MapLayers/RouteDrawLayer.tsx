import { useRef, useEffect } from 'react'
import { getProjection, getOffsetProjection } from './mappingUtils';
import type { MapPropsInterface } from './MapInterface';
import './maps.css'

interface RouteDrawMapProps extends MapPropsInterface {
    selectedAircraft: any,
    routeDraw: boolean,
}

/**
 * Draws the route draw layers (toggled by pressing R) of a RadarScreen
 */
const RouteDrawLayer = ({ projectionCenter, zoom, screenSize, selectedAircraft, routeDraw }: RouteDrawMapProps) => {
    const canvasRef = useRef<HTMLCanvasElement>(null);

    const DrawMap = async () => {
        if (!selectedAircraft)
            return;

        const canvas = canvasRef.current;
        if (!canvas) return;

        const ctx = canvas.getContext("2d");
        if (!ctx) return;

        canvas.width = screenSize[0];
        canvas.height = screenSize[1];

        // center canvas
        ctx.translate(canvas.width / 2, canvas.height / 2);

        var points: [number, number][] = [];

        selectedAircraft.trajectory.predictedPoints.forEach((point: any) => {
            let [x, y] = getProjection([point.position.lat, point.position.lon], projectionCenter, zoom)
            points.push([x, y]);
        });

        if (points.length > 1) {
            ctx.beginPath();
            const [startX, startY] = points[0];
            ctx.moveTo(startX, startY);

            for (let i = 1; i < points.length; i++) {
                const [x, y] = points[i];
                ctx.lineTo(x, y);
            }

            ctx.strokeStyle = "#2da32dff";
            ctx.lineWidth = 1;
            ctx.stroke();
        }
    };

    useEffect(() => {
        DrawMap();

        const handleResize = () => {
            DrawMap();
        }

        window.addEventListener('resize', handleResize);

        return () => {
            window.removeEventListener('resize', handleResize);
        };
    }, [projectionCenter, zoom, screenSize]);


    const formatDateTime = (dateTime: Date): string => {
        return `${dateTime.getUTCHours().toString().padStart(2, "0")}:${dateTime.getUTCMinutes().toString().padStart(2, "0")}:${dateTime.getUTCSeconds().toString().padStart(2, "0")}`
    }


    if (routeDraw) {
        return (
            <>
                <canvas className="route-draw-layer" ref={canvasRef} />
                <div className="route-draw-text-layer">
                    {selectedAircraft && selectedAircraft.flightPlan.route.map((point: any) => (
                        <div style={{
                            position: "absolute",
                            top: getOffsetProjection([point.coordinate.lat, point.coordinate.lon], projectionCenter, zoom, screenSize)[1],
                            left: getOffsetProjection([point.coordinate.lat, point.coordinate.lon], projectionCenter, zoom, screenSize)[0]
                        }}>
                            {point.name}
                        </div>
                    )
                    )}
                </div>
                <div className="route-draw-time-layer">
                    {selectedAircraft && selectedAircraft.trajectory.predictedPoints.map((point: any) => (
                        <div style={{
                            position: "absolute",
                            top: getOffsetProjection([point.position.lat, point.position.lon], projectionCenter, zoom, screenSize)[1],
                            left: getOffsetProjection([point.position.lat, point.position.lon], projectionCenter, zoom, screenSize)[0]
                        }}>
                            {formatDateTime(new Date(point.time))}
                        </div>
                    )
                    )}
                </div>
            </>
        )
    }
    else {
        return <></>
    }
}

export default RouteDrawLayer;