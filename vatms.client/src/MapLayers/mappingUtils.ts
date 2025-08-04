import { useState, useLayoutEffect } from 'react';

export function convertDDToCartesian(lat: number, lon: number): [number, number, number] {
    // convert to radians
    lat = lat * (Math.PI / 180);
    lon = lon * (Math.PI / 180);

    const x = Math.cos(lat) * Math.cos(lon);
    const y = Math.cos(lat) * Math.sin(lon);
    const z = Math.sin(lat);

    return [x, y, z];
}

export function rotateCartesianByCenter(lat: number, lon: number, cartesianCoords: [number, number, number]): [number, number, number] {
    // convert to radians
    lat = lat * (Math.PI / 180);
    lon = lon * (Math.PI / 180);

    var x: number = cartesianCoords[0];
    var y: number = cartesianCoords[1];
    var z: number = cartesianCoords[2];

    // rotation, z axis, angle (-lon) anticolockwise
    var x_rot: number = (x * Math.cos(lon)) + (y * Math.sin(lon));
    var y_rot: number = (-x * Math.sin(lon)) + (y * Math.cos(lon));
    var z_rot: number = z;

    x = x_rot;
    y = y_rot;
    z = z_rot;

    // rotation, y axis, angle (PI/2 - lat) anticlockwise
    const rotAngleY: number = (Math.PI / 2) + lat;

    x_rot = (x * Math.cos(rotAngleY)) + (z * Math.sin(rotAngleY));
    y_rot = y;
    z_rot = (-x * Math.sin(rotAngleY)) + (z * Math.cos(rotAngleY));

    return [x_rot, y_rot, z_rot];
}

export function applyZoomToProjected(x: number, y: number, zoom: number): [number, number] {
    const SCALE_FACTOR = 1000; // TODO: adjust this so it is some meaningful value instead of a random one
    x = (x * zoom * SCALE_FACTOR);
    y = (y * zoom * SCALE_FACTOR);

    return [x, y];
}

export function stereographicProjection(x: number, y: number, z: number): [number, number] {
    const x_proj = x / (1 - z);
    const y_proj = y / (1 - z);
    // return [x / (1 - z), y / (1 - z)];
    return [y_proj, -x_proj];
}

export function rotateProjection90Clockwise(x: number, y: number): [number, number] {
    return [y, -x];
}

export function useWindowSize() {
    const [size, setSize] = useState([0, 0]);
    useLayoutEffect(() => {
        function updateSize() {
            setSize([window.innerWidth, window.innerHeight]);
        }
        window.addEventListener('resize', updateSize);
        updateSize();
        return () => window.removeEventListener('resize', updateSize);
    }, []);
    return size;
}

export function getProjection(coord: [lat: number, lon: number], projectionCenter: [lat: number, lon: number], zoom: number): [number, number] {
    let [x, y, z] = convertDDToCartesian(coord[0], coord[1]);
    [x, y, z] = rotateCartesianByCenter(projectionCenter[0], projectionCenter[1], [x, y, z]);
    [x, y] = stereographicProjection(x, y, z);
    [x, y] = applyZoomToProjected(x, y, zoom);

    return [x, y];
}