import "./ifacts.css";
import { useState } from "react";

export default function Clock(this: any) {
    const [time, setTime] = useState<string>("");
    const [seconds, setSeconds] = useState<string>("");

    setInterval(() => {
        const time = new Date();
        const hours: string = time.getUTCHours().toString();
        const minutes: string = time.getUTCMinutes().toString();
        const seconds: string = time.getUTCSeconds().toString();

        const formattedTime =
            hours.padStart(2, "0") + " " + minutes.padStart(2, "0");

        const formattedSeconds = ":" + seconds.padStart(2, "0");

        setTime(formattedTime);
        setSeconds(formattedSeconds)
    }, 1000);

    return <div className="ifacts-clock">
        <span style={{ fontSize: "18px" }}>{time}</span>
        <span style={{ fontSize: "13px" }}>{seconds}</span>
    </div>
}
