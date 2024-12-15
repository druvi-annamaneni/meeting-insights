import { useEffect, useState } from "react";
import { API_BASE_URL } from "./service";
import MeetingCard from "./MeetingCard";
import { useNavigate } from "react-router-dom";

export default function MeetingsPage() {
    const [meetings, setMeetings] = useState<any[]>([]);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        var emailAdd = localStorage.getItem('userEmail') ?? null;
        if(!emailAdd)
        {
            alert("Email not found");
        }
        fetch(`${API_BASE_URL}/participant-meetings/${emailAdd}`, {
            method: "GET",
            headers: {
                "ngrok-skip-browser-warning": "true",
            },
        })
            .then((response) => response.json())
            .then((data) => { setMeetings(data);})
            .catch((error) => { console.error("Error fetching action items:", error); setError(error) });
            
    }, []);


    return (
        <div>
            <div>
                Find your meetings here
            </div>
            <div>
                {error && <p style={{ color: "red" }}>{error}</p>}

                <div style={{ display: "flex", flexWrap: "wrap", gap: "16px", padding: "16px" }}>
                    {meetings.map((meeting) => (
                        <div key={meeting.id} onClick={() => navigate(`/meeting/${meeting.id}`)}>
                            <MeetingCard {...meeting} />
                        </div>
                    ))}
                </div>
            </div>
        </div>)
}