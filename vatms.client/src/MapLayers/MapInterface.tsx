
/**
 * Every rendered map based on geographical coordinates implements this interface
 */
export interface MapPropsInterface {
    projectionCenter: [lat: number, lon: number];
    zoom: number;
    screenSize: [width: number, height: number]
}