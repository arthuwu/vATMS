import { useRef, useEffect, useState } from 'react'
import { getProjection } from './mappingUtils';
import type { MapPropsInterface } from './MapInterface';
import './maps.css'
import SignalRManager from '../SignalR/SignalRManager';

/**
 * Draws the sector maps of a RadarScreen
 */
const SectorMapLayer = ({ projectionCenter, zoom, screenSize }: MapPropsInterface) => {
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const [sectorJson, setSectorJson] = useState<any>(null);

    const signalRManager: SignalRManager = SignalRManager.getInstance();

    const DrawMap = async () => {
        const canvas = canvasRef.current;
        if (!canvas) return;

        const ctx = canvas.getContext("2d");
        if (!ctx) return;

        canvas.width = screenSize[0];
        canvas.height = screenSize[1];

        // center canvas
        ctx.translate(canvas.width / 2, canvas.height / 2);

        const geojson = sectorJson;

        for (const feature of geojson.features) {
            const geometry = feature.geometry;

            let polygons: number[][][] = [];

            if (geometry.type === "Polygon") {
                polygons = geometry.coordinates;
            } else if (geometry.type === "MultiPolygon") {
                geometry.coordinates.forEach((poly: number[][][]) => {
                    polygons.push(...poly);
                });
            }

            for (const ring of polygons) {
                const points: [number, number][] = [];

                for (const coord of ring) {
                    let [x, y] = getProjection([coord[1], coord[0]], projectionCenter, zoom)
                    //[x, y] = rotateProjection90Clockwise(x, y);

                    points.push([x, y]);
                }

                if (points.length > 1) {
                    ctx.beginPath();
                    const [startX, startY] = points[0];
                    ctx.moveTo(startX, startY);

                    for (let i = 1; i < points.length; i++) {
                        const [x, y] = points[i];
                        ctx.lineTo(x, y);
                    }

                    ctx.closePath();
                    ctx.strokeStyle = "#414141";
                    ctx.lineWidth = 1;
                    ctx.stroke();
                }
            }
        }
    };

    useEffect(() => {
        const SetupMapHandler = (map: string) => {
            setSectorJson(JSON.parse(map));
        }
        signalRManager.on("SetupSectorMap", SetupMapHandler)

        return () => {
            signalRManager.off("SetupSectorMap", SetupMapHandler)
        }
    }, [signalRManager])

    useEffect(() => {
        DrawMap();

        const handleResize = () => {
            DrawMap();
        }

        window.addEventListener('resize', handleResize);

        return () => {
            window.removeEventListener('resize', handleResize);
        };
    }, [projectionCenter, zoom, screenSize, sectorJson]);


    return <canvas className="sector-map-layer" ref={canvasRef} />
}

export default SectorMapLayer;