import { useRef, useEffect, useState, useMemo } from 'react';
import { useSearchParams } from "react-router-dom";
import BaseMapLayer from './MapLayers/BaseMapLayer';
import TargetLayer from './MapLayers/TargetLayer';
import SectorMapLayer from './MapLayers/SectorMapLayer';
import KeyboardManager from '././KeyboardManager/keyboardManager';
import RadarScreenFrame from './iFACTS/RadarScreenFrame';
import SignalRManager from './SignalR/SignalRManager';
import './App.css';

const keybinds = {
    "PAN_RIGHT": "ArrowRight",
    "PAN_LEFT": "ArrowLeft",
    "PAN_UP": "ArrowUp",
    "PAN_DOWN": "ArrowDown",
    "DECR_RANGE": "Equal", // TOOD: Change this to numpad add/subtract; for my testing purposes this is INCORRECT because i use a 60% 
    "INCR_RANGE": "Minus",
    "ROUTE_DRAW": "KeyR",
}


KeyboardManager.getInstance().loadKeybinds(keybinds); // TODO: move this to somewhere appropriate

const RadarScreen = () => {
    const [projectionCenter, setProjectionCenter] = useState<[number, number]>([51.4775, -0.461389]);
    const [zoomLevel, setZoomLevel] = useState<number>(60);
    const [screenSize, setScreenSize] = useState({ width: 0, height: 0 });
    const [routeDraw, setRouteDraw] = useState<boolean>(false);

    const [searchParams] = useSearchParams();
    const initialSector = useMemo(() => searchParams.get("sector") ?? "", [searchParams]);
    const [sectorId] = useState<string>(initialSector);

    const keyboardManager = KeyboardManager.getInstance();
    const containerRef = useRef<HTMLDivElement>(null);

    const signalRManager: SignalRManager = SignalRManager.getInstance();

    useEffect(() => {
        const handleInput = () => {
            if (keyboardManager.isActionPressed("PAN_RIGHT")) {
                setProjectionCenter(([lat, lon]) => [lat, lon + 0.01])
            } else if (keyboardManager.isActionPressed("PAN_LEFT")) {
                setProjectionCenter(([lat, lon]) => [lat, lon - 0.01])
            } else if (keyboardManager.isActionPressed("PAN_UP")) {
                setProjectionCenter(([lat, lon]) => [lat + 0.01, lon])
            } else if (keyboardManager.isActionPressed("PAN_DOWN")) {
                setProjectionCenter(([lat, lon]) => [lat - 0.01, lon])
            } else if (keyboardManager.isActionPressed("DECR_RANGE")) {
                setZoomLevel((zl) => zl + 1)
            } else if (keyboardManager.isActionPressed("INCR_RANGE")) {
                setZoomLevel((zl) => zl - 1)
            } else if (keyboardManager.isActionPressed("ROUTE_DRAW")) {
                setRouteDraw(prev => !prev);
                console.log(routeDraw);
            }
        };

        keyboardManager.subscribe(handleInput);
        return () => keyboardManager.unsubscribe(handleInput);
    }, []);

    useEffect(() => {
        const handleUpdateScreenSize = () => {
            if (containerRef.current) {
                setScreenSize({ width: containerRef.current.clientWidth, height: containerRef.current.clientHeight })
            }
        }

        handleUpdateScreenSize();

        window.addEventListener('resize', handleUpdateScreenSize);

        return () => {
            window.removeEventListener('resize', handleUpdateScreenSize);
        };
    }, [])

    useEffect(() => {
        // TODO: move this to iFACTS
        const openSelectionWindow = (sectorId: string) => {
            signalRManager.onReady(() => {
                signalRManager.send("OpenSelectionWindow", sectorId);
            })
        }

        openSelectionWindow(sectorId);
    }, [])

    return (
        <RadarScreenFrame sectorId={sectorId}>
            <div className='radar-screen-container' ref={containerRef}>
                <TargetLayer projectionCenter={projectionCenter} zoom={zoomLevel} screenSize={[screenSize.width, screenSize.height]} routeDraw={routeDraw} sectorId={sectorId}></TargetLayer>
                <SectorMapLayer projectionCenter={projectionCenter} zoom={zoomLevel} screenSize={[screenSize.width, screenSize.height]}></SectorMapLayer>
                <BaseMapLayer projectionCenter={projectionCenter} zoom={zoomLevel} screenSize={[screenSize.width, screenSize.height]}></BaseMapLayer>
            </div>
        </RadarScreenFrame>
    )
}

export default RadarScreen;