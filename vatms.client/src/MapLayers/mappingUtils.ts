// mappingUtils.ts contains functions used to transform a decimal degree coordinate to a pixel coordinate, and vice versa

/**
 * Converts decimal degrees to cartesian coordinates
 * @param lat latitude, in decimal degrees
 * @param lon longitude, in decimal degrees
 * @returns an array of 3 numbers: [x, y, z] coordinates in cartesian space
 */
function convertDDToCartesian(lat: number, lon: number): [number, number, number] {
    // convert to radians
    lat = lat * (Math.PI / 180);
    lon = lon * (Math.PI / 180);

    const x = Math.cos(lat) * Math.cos(lon);
    const y = Math.cos(lat) * Math.sin(lon);
    const z = Math.sin(lat);

    return [x, y, z];
}

/**
 * Rotates the cartesian coordinates by the viewpoint centre 
 * @param lat latitude of the viewpoint centre, in decimal degrees
 * @param lon longitude of the viewpoint centre, in decimal degrees
 * @param cartesianCoords an array of 3 numbers: [x, y, z] coordinates in cartesian space
 * @returns an array of 3 numbers: [x, y, z] coordinates in cartesian space
 */
function rotateCartesianByCenter(lat: number, lon: number, cartesianCoords: [number, number, number]): [number, number, number] {
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

/**
 * Zooms the pixel coordinates by a scalar value
 * @param x screen x-coordinate
 * @param y screen y-coordinate
 * @param zoom scalar zoom level
 * @returns an array of 2 numbers: [x, y] screen coordinates
 */
function applyZoomToProjected(x: number, y: number, zoom: number): [number, number] {
    const SCALE_FACTOR = 1000;
    x = (x * zoom * SCALE_FACTOR);
    y = (y * zoom * SCALE_FACTOR);

    return [x, y];
}

/**
 * Applies the stereographic projection algorithm
 * @param x cartesian x-coordinate
 * @param y cartesian y-coordinate
 * @param z cartesian z-coordinate
 * @returns an array of 2 numbers: [x, y] screen coordinates
 */
function stereographicProjection(x: number, y: number, z: number): [number, number] {
    const x_proj = x / (1 - z);
    const y_proj = y / (1 - z);
    // return [x / (1 - z), y / (1 - z)];
    return [y_proj, -x_proj];
}

/**
 * Applies the required stereographic projection
 * @param coord an array of 2 numbers: [lat, lon] decimal degree coordinates
 * @param projectionCenter an array of 2 numbers: [lat, lon] decimal degree coordinates
 * @param zoom a scalar zoom level
 * @returns an array of 2 numbers: [x, y] screen coordinates
 */
export function getProjection(coord: [lat: number, lon: number], projectionCenter: [lat: number, lon: number], zoom: number): [number, number] {
    let [x, y, z] = convertDDToCartesian(coord[0], coord[1]);
    [x, y, z] = rotateCartesianByCenter(projectionCenter[0], projectionCenter[1], [x, y, z]);
    [x, y] = stereographicProjection(x, y, z);
    [x, y] = applyZoomToProjected(x, y, zoom);

    return [x, y];
}

/**
 * Applies the required stereographic projection, then offsets it to the middle of the canvas to correspond with how the HTML canvas is drawn
 * @param coord an array of 2 numbers: [lat, lon] decimal degree coordinates
 * @param projectionCenter an array of 2 numbers: [lat, lon] decimal degree coordinates
 * @param zoom a scalar zoom level
 * @param screenSize the size of the screen: [height, width]
 * @returns an array of 2 numbers: [x, y] screen coordinates
 */
export function getOffsetProjection(coord: [lat: number, lon: number], projectionCenter: [lat: number, lon: number], zoom: number, screenSize: [number, number]): [number, number] {
    let [left_coord, top_coord]: [number, number] = getProjection(coord, projectionCenter, zoom);
    left_coord += screenSize[0] / 2;
    top_coord += screenSize[1] / 2;

    return [left_coord, top_coord];
}

/**
 * Converts cartesian coordinates to decimal degrees
 * @param x x-coordinate
 * @param y y-coordinate
 * @param z z-coordinate
 * @returns the required decimal degrees coordinate
 */
function convertCartesianToDD(x: number, y: number, z: number): [number, number] {
    // convert to radians
    var lat = Math.asin(z);
    var lon = Math.atan2(y, x);

    lat = lat * (180 / Math.PI);
    lon = lon * (180 / Math.PI);

    return [lat, lon];
}

/**
 * Rotates the cartesian coordinates by the viewpoint centre, in reverse
 * @param lat latitude of the viewpoint centre, in decimal degrees
 * @param lon longitude of the viewpoint centre, in decimal degrees
 * @param cartesianCoords an array of 3 numbers: [x, y, z] coordinates in cartesian space
 * @returns an array of 3 numbers: [x, y, z] coordinates in cartesian space
 */
function rotateCartesianByCenterReverse(lat: number, lon: number, cartesianCoords: [number, number, number]): [number, number, number] {
    // convert to radians
    lat = lat * (Math.PI / 180);
    lon = lon * (Math.PI / 180);

    var x: number = cartesianCoords[0];
    var y: number = cartesianCoords[1];
    var z: number = cartesianCoords[2];

    // rotation, y axis, angle (PI/2 - lat) clockwise
    const rotAngleY: number = (Math.PI / 2) + lat;

    x_rot = (x * Math.cos(rotAngleY)) + (-z * Math.sin(rotAngleY));
    y_rot = y;
    z_rot = (x * Math.sin(rotAngleY)) + (z * Math.cos(rotAngleY));

    x = x_rot;
    y = y_rot;
    z = z_rot;

    // rotation, z axis, angle (-lon) clockwise
    var x_rot: number = (x * Math.cos(lon)) + (-y * Math.sin(lon));
    var y_rot: number = (x * Math.sin(lon)) + (y * Math.cos(lon));
    var z_rot: number = z;

    return [x_rot, y_rot, z_rot];
}

/**
 * Un-zooms the pixel coordinates by a scalar value
 * @param x screen x-coordinate
 * @param y screen y-coordinate
 * @param zoom scalar zoom level
 * @returns an array of 2 numbers: [x, y] screen coordinates
 */
function applyZoomToProjectedReverse(x: number, y: number, zoom: number): [number, number] {
    const SCALE_FACTOR = 1000;
    x = (x / zoom / SCALE_FACTOR);
    y = (y / zoom / SCALE_FACTOR);

    return [x, y];
}

/**
 * Applies the inverse stereographic projection algorithm
 * @param x screen x-coordinate
 * @param y screen y-coordinate
 * @returns an array of 2 numbers: [x, y, z] cartesian coordinates
 */
function inverseStereographicProjection(x: number, y: number): [number, number, number] {
    const original_x = 2 * -y / (1 + Math.pow(-y, 2) + Math.pow(x, 2));
    const original_y = 2 * x / (1 + Math.pow(-y, 2) + Math.pow(x, 2));
    const original_z = (-1 + Math.pow(-y, 2) + Math.pow(x, 2)) / (1 + Math.pow(-y, 2) + Math.pow(x, 2));

    return [original_x, original_y, original_z]
}

/**
 * Applies the required inverse stereographic projection
 * @param coord an array of 2 numbers:[x, y] screen coordinates
 * @param projectionCenter an array of 2 numbers: [lat, lon] decimal degree coordinates
 * @param zoom scalar zoom level
 * @returns an array of 2 numbers: [lat, lon] decimal degree coordinates
 */
export function getInverseProjection(coord: [x: number, y: number], projectionCenter: [lat: number, lon: number], zoom: number): [lat: number, lon: number] {
    var [x, y] = applyZoomToProjectedReverse(coord[0], coord[1], zoom);
    var [x, y, z] = inverseStereographicProjection(x, y);
    [x, y, z] = rotateCartesianByCenterReverse(projectionCenter[0], projectionCenter[1], [x, y, z]);
    var [lat, lon] = convertCartesianToDD(x, y, z);

    return [lat, lon];
}

/**
 * Offsets screen coordinates by the middle of the canvas to correspond with how the HTML canvas is drawn, then applies the required inverse stereographic projection
 * @param coord an array of 2 numbers: [x, y] screen coordinates
 * @param projectionCenter an array of 2 numbers: [lat, lon] decimal degree coordinates
 * @param zoom a scalar zoom level
 * @param screenSize the size of the screen: [height, width]
 * @returns an array of 2 numbers: [lat, lon] decimal degree coordinates
 */
export function getInverseOffsetProjection(coord: [x: number, y: number], projectionCenter: [lat: number, lon: number], zoom: number, screenSize: [number, number]): [lat: number, lon: number] {
    let [left_coord, top_coord] = coord;
    left_coord -= screenSize[0] / 2;
    top_coord -= screenSize[1] / 2;

    return getInverseProjection([left_coord, top_coord], projectionCenter, zoom);
}
