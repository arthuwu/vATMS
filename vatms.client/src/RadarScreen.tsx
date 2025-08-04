import { useEffect, useState } from 'react';
// import * as signalR from '@microsoft/signalr';
import BaseMapLayer from './MapLayers/BaseMapLayer';
import KeyboardManager from '././KeyboardManager/keyboardManager';
import './App.css';

const keybinds = {
    "PAN_RIGHT": "ArrowRight",
    "PAN_LEFT": "ArrowLeft",
    "PAN_UP": "ArrowUp",
    "PAN_DOWN": "ArrowDown",
    "DECR_RANGE": "Equal", // TOOD: Change this to numpad add/subtract; for my testing purposes this is INCORRECT because i use a 60% 
    "INCR_RANGE": "Minus"
}

KeyboardManager.getInstance().loadKeybinds(keybinds); // TODO: move this to somewhere appropriate

const RadarScreen = () => {
    const [projectionCenter, setProjectionCenter] = useState<[number, number]>([0, 0]);
    const [zoomLevel, setZoomLevel] = useState<number>(1);

    const keyboardManager = KeyboardManager.getInstance();

    useEffect(() => {
        const handleInput = () => {
            if (keyboardManager.isActionPressed("PAN_RIGHT")) {
                setProjectionCenter(([lat, lon]) => [lat, lon + 1])
            } else if (keyboardManager.isActionPressed("PAN_LEFT")) {
                setProjectionCenter(([lat, lon]) => [lat, lon - 1])
            } else if (keyboardManager.isActionPressed("PAN_UP")) {
                setProjectionCenter(([lat, lon]) => [lat + 1, lon])
            } else if (keyboardManager.isActionPressed("PAN_DOWN")) {
                setProjectionCenter(([lat, lon]) => [lat - 1, lon])
            } else if (keyboardManager.isActionPressed("DECR_RANGE")) {
                setZoomLevel((zl) => zl + 0.2)
            } else if (keyboardManager.isActionPressed("INCR_RANGE")) {
                setZoomLevel((zl) => zl - 0.2)
            }
        };

        keyboardManager.subscribe(handleInput);
        return () => keyboardManager.unsubscribe(handleInput);
    }, []);

    return (
        <>
            <BaseMapLayer map={null} projectionCenter={projectionCenter} zoom={zoomLevel}></BaseMapLayer>
        </>
    )
}

export default RadarScreen;