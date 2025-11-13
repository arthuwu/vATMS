import { useRef, useEffect, useState } from 'react'
import { getProjection, getOffsetProjection, getInverseOffsetProjection } from './mappingUtils';
import type { MapPropsInterface } from './MapInterface';
import './maps.css'

interface RangeBearingMapProps extends MapPropsInterface {
    rblDraw: boolean,
}

const RangeBearingLayer = ({ projectionCenter, zoom, screenSize, rblDraw }: RangeBearingMapProps) => {
    const [corner1, setCorner1] = useState<[lat: number, lon: number] | null>(null);
    const [corner2, setCorner2] = useState<[lat: number, lon: number] | null>(null);
    const canvasRef = useRef<HTMLCanvasElement>(null);

    const DrawMap = async () => {
        const canvas = canvasRef.current;
        if (!canvas) return;

        const ctx = canvas.getContext("2d");
        if (!ctx) return;

        canvas.width = screenSize[0];
        canvas.height = screenSize[1];

        // center canvas
        ctx.translate(canvas.width / 2, canvas.height / 2);

        if (!corner1 || !corner2) {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            return;
        }

        ctx.beginPath();
        const [startX, startY] = getProjection(corner1, projectionCenter, zoom);
        const [endX, endY] = getProjection(corner2, projectionCenter, zoom);

        ctx.moveTo(startX, startY);
        ctx.lineTo(endX, endY);

        ctx.closePath();

        ctx.strokeStyle = "#ffffffff";
        ctx.lineWidth = 1;
        ctx.stroke();
    };

    const handleMouseDown = (e: React.MouseEvent<HTMLElement>) => {
        let point: [number, number] = getInverseOffsetProjection([e.clientX - e.currentTarget.getBoundingClientRect().left, e.clientY - e.currentTarget.getBoundingClientRect().top], projectionCenter, zoom, screenSize);

        if (corner1 && corner2) {
            setCorner1(null);
            setCorner2(null);
        } else if (corner1) { // in the middle of a selection, so set corner2
            setCorner2(point);
        } else { // finished selecting, so reset and set corner 1 again OR haven't selected anything, so selecr corner 1
            setCorner1(point);
        }
    }

    const getBearingBetweenTwoPoints = (coord1: [number, number], coord2: [number, number]) => {
        const lat1: number = coord1[0] * Math.PI / 180;
        const lon1: number = coord1[1] * Math.PI / 180;

        const lat2: number = coord2[0] * Math.PI / 180;
        const lon2: number = coord2[1] * Math.PI / 180;

        const a: number = Math.sin(lon2 - lon1) * Math.cos(lat2);
        const b: number = Math.cos(lat1) * Math.sin(lat2) - Math.sin(lat1) * Math.cos(lat2) * Math.cos(lon2 - lon1);

        const radians: number = Math.atan2(a, b);
        const brng: number = (radians * (180 / Math.PI) + 360) % 360;

        return brng;
    }

    const getDistanceBetweenTwoPoints = (coord1: [number, number], coord2: [number, number]) => {
        const R: number = 6371e+003;
        const M_TO_NM: number = 0.000539957;

        const lat1: number = coord1[0] * Math.PI / 180;
        const lat2: number = coord2[0] * Math.PI / 180;

        const latDiff = (coord1[0] - coord2[0]) * Math.PI / 180;
        const lonDiff = (coord1[1] - coord2[1]) * Math.PI / 180;

        const a = Math.sin(latDiff / 2) * Math.sin(latDiff / 2) + Math.cos(lat1) * Math.cos(lat2) * Math.sin(lonDiff / 2) * Math.sin(lonDiff / 2);

        const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

        return R * c * M_TO_NM;
    }

    useEffect(() => {
        DrawMap();

        const handleResize = () => {
            DrawMap();
        }

        window.addEventListener('resize', handleResize);

        return () => {
            window.removeEventListener('resize', handleResize);
        };
    }, [projectionCenter, zoom, screenSize, corner1, corner2]);


    if (rblDraw) {
        return <>
            <canvas className="range-bearing-layer" ref={canvasRef} />
            <div className='range-bearing-clickspot' onMouseDown={handleMouseDown}>
                {corner1 && corner2 && <div className='rbl-label' style={{
                    position: "absolute",
                    top: getOffsetProjection(corner2, projectionCenter, zoom, screenSize)[1],
                    left: getOffsetProjection(corner2, projectionCenter, zoom, screenSize)[0]
                }}>{getBearingBetweenTwoPoints(corner1, corner2).toFixed(1).padStart(5, "0")}°&nbsp;{getDistanceBetweenTwoPoints(corner1, corner2).toFixed(1)}nm</div>}
            </div>
        </>
    }
}

export default RangeBearingLayer;
