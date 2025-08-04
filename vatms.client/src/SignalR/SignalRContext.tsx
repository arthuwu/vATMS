import { createContext, useEffect, useContext } from "react";
import SignalRManager from './SignalRManager';


const SignalRContext = createContext<SignalRManager | null>(null);

const SignalRProvider = ({ children }: any) => { // exporting this allows us to be very sure we are using the same SignalR connection throughout
    const signalRManager = SignalRManager.getInstance();

    useEffect(() => {
        signalRManager.startConnection();

        return () => {
            signalRManager.stopConnection();
        };
    }, [])

    return (
        <SignalRContext value={signalRManager}>
            {children}
        </SignalRContext>
    )
}

const useSignalR = () => useContext(SignalRContext); // exporting this allows us to be very sure we are using the same SignalRContext throughout

export { SignalRProvider, useSignalR };