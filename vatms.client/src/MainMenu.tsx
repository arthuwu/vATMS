import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import './App.css';

const MainMenu = () => {
    let navigate = useNavigate();
    const [sectorList, setSectorList] = useState<any>([]);
    const [selectedSector, setSelectedSector] = useState<any>();

    const startSimulation = () => {
        navigate(`/radar-screen?sector=${selectedSector}`)
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
                    (<option value={sector.displayName}>{sector.displayName}</option>)
                )}
            </select>
            <button onClick={startSimulation}>Start!</button>
        </div>
    )
}

export default MainMenu;