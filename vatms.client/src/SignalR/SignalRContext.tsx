import { createContext, useEffect, useContext, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import SignalRManager from './signalRManager';

const SignalRContext = createContext<SignalRManager | null>(null);

/**
 * Provides context of the SignalRManager instance to react components
 */
const SignalRProvider = ({ children }: any) => {
    const [searchParams] = useSearchParams();
    const initialSector = useMemo(() => searchParams.get("sector") ?? "", [searchParams]);
    const [sector] = useState(initialSector);
    const signalRManager = SignalRManager.createInstance(sector);

    useEffect(() => {
        signalRManager.startConnection();

        return () => {
            signalRManager.stopConnection();
        };
    }, [])

    useEffect(() => { // this can only be sent once the server Simulation instance itself is ready
        signalRManager.on("simulationReady", () => {
            if (searchParams.has("rate"))
                signalRManager.send("setSimulationRate", searchParams.get("rate"))
        })
    }, [signalRManager])

    return (
        <SignalRContext value={signalRManager}>
            {children}
        </SignalRContext>
    )
}

const useSignalR = () => useContext(SignalRContext); // exporting this allows us to be very sure we are using the same SignalRContext throughout

export { SignalRProvider, useSignalR };