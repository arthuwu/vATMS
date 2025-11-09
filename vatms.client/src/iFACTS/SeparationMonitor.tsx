import SignalRManager from '../SignalR/SignalRManager';
import { useEffect, useState } from 'react';
import type { PredictedConflictPair } from "../Types";

const SeparationMonitor = () => {
    const [conflictList, setConflictList] = useState<PredictedConflictPair[]>([]);
    const WIDTH: number = 200;
    const HEIGHT: number = 160;

    const signalRManager: SignalRManager = SignalRManager.getInstance();

    useEffect(() => {
        signalRManager.on("conflictPairsUpdate", (pairs: PredictedConflictPair[]) => {
            setConflictList(pairs);
        })
    }, [signalRManager])

    const calculatePixelCoordinate = (timestamp: string, cpa: number): [number, number] => {
        let dateTime: Date = new Date(timestamp);
        let timeDiff: number = (dateTime.getDate() - new Date().getDate()) / (60 * 1000);

        let left_coord = (WIDTH / 15) * timeDiff;
        let top_coord = (HEIGHT / 15) * (15 - cpa);

        return [left_coord, top_coord]
    }

    return <div className='separation-monitor-container'>
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
    </div>
}

export default SeparationMonitor