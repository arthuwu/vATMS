import { useState, useEffect } from 'react';
import PopupWindow from './PopupWindow';
import Clock from './Clock';
import './ifacts.css'
import SeparationMonitor from './SeparationMonitor';
import SignalRManager from '../SignalR/signalRManager';
import type { PredictedConflictPair } from "../Types";
import SimulationInfo from './SimulationInfo';

/*
Component for the frame surrounding an iFACTS radar screen
Also contains the container for all popup windows
*/
const RadarScreenFrame = ({ sectorId, children, setRblDraw }: any) => {
    const [conflictList, setConflictList] = useState<PredictedConflictPair[]>([]);
    const [simulationInfo, setSimulationInfo] = useState<{ rate: number, lossesOfSeperation: number, missedStandingAgreements: number }>();

    const [isVisible, setIsVisible] = useState<any>({}); // stores a dictionary of {string, bool} where string is the name of the window defined below
    //const [spawnRate, setSpawnRate] = useState<number>();

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

    useEffect(() => {
        signalRManager.on("simulationInfoUpdate", (rate: number, lossesOfSeperation: number, missedStandingAgreements: number) => {
            setSimulationInfo({ rate, lossesOfSeperation, missedStandingAgreements });
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
                    <div className='ifacts-button' onClick={() => toggleVisibility("sim-info")}>Simul Info</div>
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
                <PopupWindow
                    windowTitle="Simulation Info"
                    isVisible={isVisible["sim-info"]}
                    handleClose={() => toggleVisibility("sim-info")}
                    width={250}
                    height={230}
                >
                    <SimulationInfo
                        spawnRate={simulationInfo?.rate}
                        lossesOfSeperation={simulationInfo?.lossesOfSeperation}
                        missedStandingAgreements={simulationInfo?.missedStandingAgreements}
                    ></SimulationInfo>
                </PopupWindow>
            </div>
        </>
    )
}

export default RadarScreenFrame;