import SignalRManager from '../SignalR/signalRManager';

const SimulationInfo = ({ spawnRate, lossesOfSeperation, missedStandingAgreements }: any) => {
    const signalRManager: SignalRManager = SignalRManager.getInstance();

    return <div className="simulation-info-container">
        <div>Current spawn rate: {spawnRate}</div>
        <div>Missed standing agreements: {missedStandingAgreements}</div>
        <div>Losses of separation: {lossesOfSeperation}</div>
        <input
            type="number"
            placeholder="update rate (5 - 80 inclusive)"
            onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                if (e.key === "Enter") {
                    e.preventDefault();

                    if (e.currentTarget.value && e.currentTarget.value as unknown as number >= 5 && e.currentTarget.value as unknown as number <= 80) { // number input only allows number, so this should be safe
                        signalRManager.send("setSimulationRate", e.currentTarget.value);
                    }

                    e.currentTarget.value = "";
                }
            }}
        />
    </div>
}

export default SimulationInfo