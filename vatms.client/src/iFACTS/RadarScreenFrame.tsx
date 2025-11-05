import { useState } from 'react';
import PopupWindow from './PopupWindow';
import Clock from './Clock';
import './ifacts.css'

const RadarScreenFrame = ({ sectorId, children }: any) => {
    const [isVisible, setIsVisible] = useState<any>({});

    function toggleVisibility(window: string) {
        let newVisibleState: any = { ...isVisible };
        newVisibleState[window] = !newVisibleState[window];
        setIsVisible(newVisibleState);
    }

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
                    <div className='ifacts-button'>Offered</div>
                    <div className='ifacts-button'>Accept'd</div>
                    <div className='ifacts-button'>Full Flight Plan</div>
                    <div className='ifacts-button' onClick={() => toggleVisibility("sepn-monitor")}>Sepn Monitor</div>
                </div>
                <div className='ifacts-right-bar'>
                    <div></div>
                    <div className='ifacts-button'>Range and Bearing</div>
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
                    <div>TEST</div>
                </PopupWindow>
            </div>
        </>
    )
}

export default RadarScreenFrame;