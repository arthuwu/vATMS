import './ifacts.css'

const RadarScreenFrame = ({ children }: any) => {
    return (
        <div className='ifacts-outer-frame'>
            <div className='inner-container ifacts-components'>{children}</div>
        </div>
    )
}

export default RadarScreenFrame;