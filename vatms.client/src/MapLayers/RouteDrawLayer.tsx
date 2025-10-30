import { useRef, useEffect, useState } from 'react'
import { getProjection } from './mappingUtils';
import type { MapPropsInterface } from './MapInterface';
import './maps.css'

interface RouteDrawMapProps extends MapPropsInterface {
    selectedAircraft: any,
    routeDraw: boolean,
}

const RouteDrawLayer = ({ projectionCenter, zoom, screenSize, selectedAircraft, routeDraw }: RouteDrawMapProps) => {
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const [predictedPoints, setPredictedPoints] = useState<any>([]);
    const [routePoints, setRoutePoints] = useState<any>([]);

    const calculatePixelCoordinate = ([point_lat, point_lon]: [number, number]) => {
        let [left_coord, top_coord]: [number, number] = getProjection([point_lat, point_lon], projectionCenter, zoom);
        left_coord += screenSize[0] / 2;
        top_coord += screenSize[1] / 2;

        return [left_coord, top_coord];
    }

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

        predictedPoints.forEach((point: any) => {
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

            ctx.closePath();
            ctx.strokeStyle = "#2da32dff";
            ctx.lineWidth = 1;
            ctx.stroke();
        }
    };

    useEffect(() => {
        if (selectedAircraft) {
            setPredictedPoints(selectedAircraft.trajectory.predictedPoints);
            setRoutePoints(selectedAircraft.flightPlan.route);
            console.log(predictedPoints);
            console.log(routePoints);
        }
    })

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


    if (routeDraw) {
        console.log("drawing route");
        return (
            <>
                <canvas className="route-draw-layer" ref={canvasRef} />
                <div className="route-draw-text-layer">
                    {routePoints.map((point: any) => (
                        <div style={{ position: "absolute", top: calculatePixelCoordinate([point.coordinate.lat, point.coordinate.lon])[1], left: calculatePixelCoordinate([point.coordinate.lat, point.coordinate.lon])[0] }}>
                            {point.name}
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