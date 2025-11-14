import type { PredictedConflictPair } from "../Types";

/*
Component representing the iFACTS separation monitor
*/
const SeparationMonitor = ({ conflictList }: any) => {
    const WIDTH: number = 200;
    const HEIGHT: number = 160;

    const calculatePixelCoordinate = (timestamp: string, cpa: number): [number, number] => {
        let dateTime: Date = new Date(timestamp);
        let timeDiff: number = (dateTime.getMinutes() - new Date().getMinutes())

        let left_coord = (WIDTH / 15) * timeDiff; // left => quickest to occur
        let top_coord = (HEIGHT / 15) * (15 - cpa); // top => furthest distance

        return [left_coord, top_coord]
    }

    return <div className='separation-monitor-container'>
        <div>
            <div className="left-scale">
                <div style={{ position: "absolute", top: 0 }}>15</div>
                <div style={{ position: "absolute", top: HEIGHT / 3 }}>10</div>
                <div style={{ position: "absolute", top: (HEIGHT / 3) * 2 }}>5</div>
                <div style={{ position: "absolute", top: HEIGHT }}>0</div>
            </div>
            <div className='separation-monitor-background'>
                {
                    conflictList.map((e: PredictedConflictPair) => (
                        <>
                            <div style={{
                                position: "absolute",
                                top: calculatePixelCoordinate(e.startTime, e.closestPointOfApproach)[1],
                                left: calculatePixelCoordinate(e.startTime, e.closestPointOfApproach)[0]
                            }}
                                className='separation-monitor-target'>▶</div>
                            <div style={{
                                position: "absolute",
                                top: calculatePixelCoordinate(e.startTime, e.closestPointOfApproach)[1] - 30,
                                left: calculatePixelCoordinate(e.startTime, e.closestPointOfApproach)[0] + 10
                            }}
                                className='separation-monitor-callsign-pair'>
                                <div>{e.callsign1}</div>
                                <div>{e.callsign2}</div>
                            </div>
                        </>
                    ))
                }
            </div>
            <div className="bottom-scale">
                <div style={{ position: "absolute", left: 0 }}>0</div>
                <div style={{ position: "absolute", left: WIDTH / 3 }}>5</div>
                <div style={{ position: "absolute", left: (WIDTH / 3) * 2 }}>10</div>
                <div style={{ position: "absolute", left: WIDTH }}>15</div>
            </div>
        </div>
    </div>
}

export default SeparationMonitor