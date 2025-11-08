import { useState, useEffect } from "react";

interface HorizontalSelectInterface {
    elementList: string[];
    elementWidth: string;
    length: number;
    onElementSelect: (value: string) => {}
}
const HorizontalSelect = ({ elementList, elementWidth, length, onElementSelect }: HorizontalSelectInterface) => {
    const [offsetList, setOffsetList] = useState<string[]>(Array(length).fill(""));

    useEffect(() => {
        setOffsetList(elementList);
    }, [elementList])

    useEffect(() => {
        console.log(offsetList)
    }, [offsetList])

    const handleShiftLeft = () => {
        if (offsetList)
            setOffsetList((prev: string[]) => prev.slice(-1).concat(prev.slice(0, -1)));
    }

    const handleShiftRight = () => {
        if (offsetList)
            setOffsetList((prev: string[]) => prev.slice(1).concat(prev.slice(0, 1)));
    }

    return <div className="horizontal-select-container">
        <button onClick={handleShiftLeft}>◀</button>
        {offsetList && Array.isArray(offsetList) && offsetList.slice(0, length).map((e: any) => (
            <div className="select-element" style={{ width: elementWidth }} onClick={() => onElementSelect(e)}>{e}</div>
        ))}
        <button onClick={handleShiftRight}>▶</button>
    </div>
}

export default HorizontalSelect;