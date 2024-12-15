import{ useEffect, useState } from "react";
import { API_BASE_URL } from "./service";
import { useParams } from "react-router-dom";

interface ActionItem {
    id: string;
    description: string;
    deadline: string;
    participantId: string;
    participantName: string;
    meetingId: string;
}

const ActionItemsList = () => {
    const { meetingId } = useParams<{ meetingId: string }>();
    const [actionItems, setActionItems] = useState<ActionItem[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        setIsLoading(true);
        fetch(`${API_BASE_URL}/action-items/${meetingId}`, {
            method: "GET",
            headers: {
                "ngrok-skip-browser-warning": "true",
            },
        })
            .then((response) => response.json())
            .then((data) => { setActionItems(data); setIsLoading(false); })
            .catch((error) => { console.error("Error fetching action items:", error); setIsLoading(false); });
    }, []);

    if (isLoading) {
        return <div>Loading Action items...</div>
    }

    return (
        <div className="action-items-container">
            <h3>Action Items</h3>
            <div className="action-items-grid">
                {actionItems.length > 0 ? (actionItems.map((item) => (
                    <div key={item.id} className="action-item-card">
                        <p className="description">
                            <strong>Description:</strong> {item.description}
                        </p>
                        <p className="deadline">
                            <strong>Deadline:</strong> {item.deadline}
                        </p>
                        <p className="assigned-to">
                            <strong>Assigned To:</strong> {item.participantName}
                        </p>
                    </div>
                ))) : <div>No actions in this meeting</div>}
            </div>
        </div>
    );
};

export default ActionItemsList;