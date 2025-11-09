import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import './App.css';

const MainMenu = () => {
    let navigate = useNavigate();
    const [sectorList, setSectorList] = useState<any>([]);
    const [selectedSector, setSelectedSector] = useState<any>();
    const [spawnRate, setSpawnRate] = useState<number>();

    const startSimulation = () => {
        if (selectedSector && spawnRate) {
            if (spawnRate >= 5 && spawnRate <= 50)
                navigate(`/radar-screen?sector=${selectedSector}&rate=${spawnRate}`)
        }
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
            <div className="title">vATMS</div>
            <select value={selectedSector} onChange={e => setSelectedSector(e.target.value)}>
                <option value="" disabled selected hidden>select a sector</option>
                {sectorList?.logons?.map((sector: any) =>
                    (<option value={sector.id}>{sector.displayName}</option>)
                )}
            </select>
            <input type="text" pattern="[0-9]*" value={spawnRate} placeholder="spawn rate (5 - 50 inclusive)" onChange={e => setSpawnRate(e.target.value as unknown as number)}></input>
            <button onClick={startSimulation}>Start!</button>
        </div>
    )
}

export default MainMenu;