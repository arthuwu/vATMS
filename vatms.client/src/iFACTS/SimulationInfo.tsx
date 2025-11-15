const SimulationInfo = ({ spawnRate, lossesOfSeperation, missedStandingAgreements }: any) => {

    return <div className="simulation-info-container">
        <div>Current spawn rate: {spawnRate}</div>
        <div>Missed standing agreements: {missedStandingAgreements}</div>
        <div>Losses of separation: {lossesOfSeperation}</div>
    </div>
}

export default SimulationInfo