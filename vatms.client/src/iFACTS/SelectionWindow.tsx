import SignalRManager from '../SignalR/SignalRManager';
import { useEffect, useState } from 'react';

const SelectionWindow = () => {
    const [targetList, setTargetList] = useState<any>();
    const [selectedAircraftID, setSelectedAircraftID] = useState<any>();

    const signalRManager: SignalRManager = SignalRManager.getInstance();

    useEffect(() => {
        const AircraftPositionUpdateHandler = (ac: any) => {
            //console.log(ac);
            setTargetList(ac);
        }
        signalRManager.on("AircraftPositionUpdate", AircraftPositionUpdateHandler)

        return () => {
            signalRManager.off("AircraftPositionUpdate", AircraftPositionUpdateHandler)
        }
    }, [signalRManager])

    useEffect(() => {
        signalRManager.send("SetSectorSelectedAircraftID", ["sector", selectedAircraftID]); // TODO: replace sector with the actual sector string once sectorisation logic is in place
    }, [selectedAircraftID])

    useEffect(() => {
        signalRManager.on("sectorSelectedId", (sectorId: string, selectedAircraftId: string) => {
            if (sectorId)
                setSelectedAircraftID(selectedAircraftId);
        })
    }, [signalRManager])

    return (
        <>
            <div>Selection Window</div>
            <div className='temp-target-list'>
                {targetList?.map((target: any) => {
                    <div>{target.flightPlan.callsign}</div>
                })}
            </div>
            <div>Selected callsign: {selectedAircraftID}</div>
        </>
    )
}

export default SelectionWindow