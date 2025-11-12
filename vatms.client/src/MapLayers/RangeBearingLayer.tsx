import { useRef, useEffect, useState } from 'react'
import { getProjection, getInverseOffsetProjection } from './mappingUtils';
import type { MapPropsInterface } from './MapInterface';
import './maps.css'

const RangeBearingLayer = ({ projectionCenter, zoom, screenSize }: MapPropsInterface) => {
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

        ctx.beginPath();
        const [startX, startY] = getProjection(corner1!, projectionCenter, zoom);
        const [endX, endY] = getProjection(corner2!, projectionCenter, zoom);

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
            console.log("set corner2")
            setCorner2(point);
        } else { // finished selecting, so reset and set corner 1 again OR haven't selected anything, so selecr corner 1
            console.log("set corner1")
            setCorner1(point);
        }
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


    return <>
        <canvas className="range-bearing-layer" ref={canvasRef} />
        <div className='range-bearing-clickspot' onMouseDown={handleMouseDown}>
        </div>
    </>
}

export default RangeBearingLayer;
