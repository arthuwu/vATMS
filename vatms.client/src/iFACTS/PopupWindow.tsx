import './ifacts.css'
import { useRef } from 'react';
import Draggable from 'react-draggable';

const PopupWindow = ({ windowTitle, isVisible, handleClose, width, height, children }: any) => {
    const nodeRef = useRef(null); // workaround for draggable (findDOMNode deprecated)

    return <>
        {
            isVisible && (
                <Draggable nodeRef={nodeRef} handle=".ifacts-popup-handle">
                    <div className='ifacts-popup' ref={nodeRef} style={{ width: width, height: height }}>
                        <div className='ifacts-popup-handle'>
                            <div className="close-button" onClick={handleClose}>&nbsp;</div>
                            {windowTitle}
                        </div>
                        <div className='ifacts-popup-content'>
                            {children}
                        </div>
                    </div>
                </Draggable>
            )
        }
    </>
}

export default PopupWindow