import { useState, useEffect } from 'react';
import PopupWindow from './PopupWindow';
import Clock from './Clock';
import './ifacts.css'
import SeparationMonitor from './SeparationMonitor';
import SignalRManager from '../SignalR/SignalRManager';
import type { PredictedConflictPair } from "../Types";

/*
Component for the frame surrounding an iFACTS radar screen
Also contains the container for all popup windows
*/
const RadarScreenFrame = ({ sectorId, children, setRblDraw }: any) => {
    const [conflictList, setConflictList] = useState<PredictedConflictPair[]>([]);
    const [isVisible, setIsVisible] = useState<any>({}); // stores a dictionary of {string, bool} where string is the name of the window defined below

    function toggleVisibility(window: string) {
        let newVisibleState: any = { ...isVisible };
        newVisibleState[window] = !newVisibleState[window];
        setIsVisible(newVisibleState);
    }

    const signalRManager: SignalRManager = SignalRManager.getInstance();

    useEffect(() => {
        signalRManager.on("conflictPairsUpdate", (pairs: PredictedConflictPair[]) => {
            setConflictList(pairs);
        })
    }, [signalRManager])

    return (
        <>
            <div className='ifacts-outer-frame'>
                <div className='ifacts-top-bar'>
                    <div></div>
                    <Clock></Clock>
                    <div></div>
                    <div className='ifacts-sector-display'>{sectorId}</div>
                </div>
                <div className='ifacts-left-bar'>
                    <div></div>
                    <div className='ifacts-button' onClick={() => toggleVisibility("sepn-monitor")}>Sepn Monitor</div>
                </div>
                <div className='ifacts-right-bar'>
                    <div></div>
                    <div className='ifacts-button' onClick={() => setRblDraw()}>Range and Bearing</div>
                </div>
                <div className='inner-container ifacts-components'>{children}</div>
            </div>
            <div className='popup-container'>
                <PopupWindow
                    windowTitle="Separation Monitor"
                    isVisible={isVisible["sepn-monitor"]}
                    handleClose={() => toggleVisibility("sepn-monitor")}
                    width={250}
                    height={230}
                >
                    <SeparationMonitor conflictList={conflictList}></SeparationMonitor>
                </PopupWindow>
            </div>
        </>
    )
}

export default RadarScreenFrame;