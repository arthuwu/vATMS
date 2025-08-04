/* This is a fake canvas - it is a div that overlays the canvas with the exact same dimensions, so we can put some absolute divs on it */

/*
import { convertDDToCartesian, rotateCartesianByCenter, applyZoomToProjected, stereographicProjection } from './mappingUtils';
import 'maps.css';

interface TargetMapProps {
    projectionCenter: [lat: number, long: number];
    zoom: number;
}


const TargetLayer = ({ projectionCenter, zoom }: TargetMapProps) => {
    const convertTargets = (lat: number, lon: number) => {
        let [x, y, z] = convertDDToCartesian(lat, lon);
        [x, y, z] = rotateCartesianByCenter(projectionCenter[0], projectionCenter[1], [x, y, z]);
        [x, y] = stereographicProjection(x, y, z);
        [x, y] = applyZoomToProjected(x, y, zoom);
    }

    return (
        <>
            <div className="target-layer">
            </div>
        </>
    )
}

export default TargetLayer
*/