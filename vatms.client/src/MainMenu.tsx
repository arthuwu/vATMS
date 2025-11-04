import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import './App.css';

const MainMenu = () => {
    let navigate = useNavigate();
    const [sectorList, setSectorList] = useState<any>([]);
    const [selectedSector, setSelectedSector] = useState<any>();
    const [spawnRate, setSpawnRate] = useState<number>();

    const startSimulation = () => {
        navigate(`/radar-screen?sector=${selectedSector}&rate=${spawnRate}`)
    }

    useEffect(() => {
        const fetchSectors = async () => {
            const res = await fetch(`${window.location.origin}/api/get-available-sectors`);
            const json = await res.json();
            setSectorList(json);
        }

        fetchSectors();
    }, [])

    return (
        <div className="main-menu">
            <div>vATMS</div>
            <select value={selectedSector} onChange={e => setSelectedSector(e.target.value)}>
                {sectorList?.logons?.map((sector: any) =>
                    (<option value={sector.id}>{sector.displayName}</option>)
                )}
            </select>
            <div>Spawn Rate</div>
            <input type='number' value={spawnRate} onChange={e => setSpawnRate(e.target.value as unknown as number)}></input>
            <button onClick={startSimulation}>Start!</button>
        </div>
    )
}

export default MainMenu;