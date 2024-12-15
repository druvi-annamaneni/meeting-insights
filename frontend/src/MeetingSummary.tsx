import  { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { API_BASE_URL } from "./service";

interface SummaryData {
    id: string;
    summaryText: string;
    decisions: string;
    meetingId: string;
}

const MeetingSummary = () => {

    const { meetingId } = useParams<{ meetingId: string }>();
    const [summary, setSummary] = useState<SummaryData | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        setIsLoading(true);
        fetch(`${API_BASE_URL}/decisions-summary/${meetingId}`, {
            method: "GET",
            headers: {
                "ngrok-skip-browser-warning": "true",
            },
        })
            .then((response) => response.json())
            .then((data) => { setSummary(data); setIsLoading(false); })
            .catch((error) => { console.error("Error fetching summary:", error); setIsLoading(false); });
    }, []);

    const decisionList = summary?.decisions
        ? summary.decisions.split(" |& ").map((decision, index) => (
            <li key={index}>{decision.trim()}</li>
        ))
        : [];


    if (isLoading) {
        return <div>Loading Decisions and Summary...</div>
    }


    return (
        <div>
            <h3 className="heading-1">Decisions</h3>
            <div className="meeting-summary-container">
                <div className="decisions-section">
                    {decisionList.length > 0 ? <ul>{decisionList}</ul>
                        :
                        <div>No decisions were made in this meeting</div>}
                </div>
            </div>
            <h3 className="heading-1">Summary</h3>
            <div className="meeting-summary-container">
                <div className="summary-section">
                    <p>{summary?.summaryText}</p>
                </div>
            </div>
        </div>
    );
};

export default MeetingSummary;