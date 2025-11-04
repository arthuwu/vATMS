import './ifacts.css'

const RadarScreenFrame = ({ sectorId, children }: any) => {
    return (
        <div className='ifacts-outer-frame'>
            <div className='ifacts-top-bar'>{sectorId}</div>
            <div className='inner-container ifacts-components'>{children}</div>
        </div>
    )
}

export default RadarScreenFrame;