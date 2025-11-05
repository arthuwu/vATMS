import Clock from './Clock';
import './ifacts.css'

const RadarScreenFrame = ({ sectorId, children }: any) => {
    return (
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
                <div className='ifacts-button'>Sepn Monitor</div>
            </div>
            <div className='ifacts-right-bar'>
                <div></div>
                <div className='ifacts-button'>Range and Bearing</div>
            </div>
            <div className='inner-container ifacts-components'>{children}</div>
        </div>
    )
}

export default RadarScreenFrame;