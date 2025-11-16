import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import './app.css';

/**
 * Component representing the main menu of the program
 */
const MainMenu = () => {
    let navigate = useNavigate();
    const [sectorList, setSectorList] = useState<any>([]);
    const [selectedSector, setSelectedSector] = useState<any>();
    const [spawnRate, setSpawnRate] = useState<number>();

    const startSimulation = () => {
        if (selectedSector && spawnRate) {
            if (spawnRate >= 5 && spawnRate <= 80)
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
            <input type="number" value={spawnRate} placeholder="spawn rate (5 - 80 inclusive)" onChange={e => setSpawnRate(e.target.value as unknown as number)}></input>
            <button onClick={startSimulation}>Start!</button>
        </div>
    )
}

export default MainMenu;