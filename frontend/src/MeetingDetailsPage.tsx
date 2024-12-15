import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { API_BASE_URL } from "./service";
import { getFormattedDateTime } from "./utility";
import ActionItemsList from "./ActionItemsList";
import MeetingSummary from "./MeetingSummary";

interface Meeting {
  id: string;
  roomName: string;
  startTime: string;
  endTime: string;
  participantsCount: number;
}

const MeetingDetailsPage: React.FC = () => {
  const { meetingId } = useParams<{ meetingId: string }>();
  const [meeting, setMeeting] = useState<Meeting | null>(null);
  const [activeTab, setActiveTab] = useState<string>("Transcription");
  const [transcriptions, setTranscriptions] = useState<Array<{ speaker: string; speech: string }>>([]);

  useEffect(() => {
    // Fetch meeting details
    fetch(`${API_BASE_URL}/meeting/${meetingId}`, {
      method: "GET",
      headers: {
          "ngrok-skip-browser-warning": "true",
      },
  })
      .then((res) => res.json())
      .then((data) => setMeeting(data))
      .catch((err) => console.error(err));

    // Fetch transcriptions
    fetch(`${API_BASE_URL}/transcriptions/${meetingId}`, {
      method: "GET",
      headers: {
          "ngrok-skip-browser-warning": "true",
      },
  })
      .then((res) => res.json())
      .then((data) => setTranscriptions(data))
      .catch((err) => console.error(err));
  }, [meetingId]);

  if (!meeting) {
    return <p>Loading meeting details...</p>;
  }

  return (
    <div className="meeting-details" style={styles.container}>
      <div style={styles.meetingDetails}>
        <h2 style={styles.roomName}>{meeting.roomName.split("@")[0]}</h2>
        <p>
          <strong>Start:</strong> {getFormattedDateTime(meeting.startTime)}
        </p>
        <p>
          <strong>End:</strong> {getFormattedDateTime(meeting.endTime)}
        </p>
        <p>
          <strong>Participants:</strong> {meeting.participantsCount}
        </p>
      </div>

      <div className="tabs" style={styles.tabs}>
        {["Transcription", "Action Items", "Decisions & Summary"].map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            style={{
              marginRight: "16px",
              padding: "8px 16px",
              border: "none",
              borderBottom: activeTab === tab ? "2px solid #007bff" : "2px solid white",
              background: "none",
              cursor: "pointer",
              color: activeTab === tab ? "#007bff" : "#333",
            }}
          >
            {tab}
          </button>
        ))}
      </div>

      <div style={styles.tabContent}>
        {activeTab === "Transcription" && (
          <div>
            <h3>Transcription</h3>
            {transcriptions.map((t, index) => (
              <p key={index}>
                <strong>{t.speaker}:</strong> {t.speech}
              </p>
            ))}
          </div>
        )}
        {activeTab === "Action Items" && <div style={styles.tabContent}> <ActionItemsList /> </div>}
        {activeTab === "Decisions & Summary" && <div style={styles.tabContent}> <MeetingSummary /> </div>}
      </div>

    </div>
  );
};

export default MeetingDetailsPage;


const styles = {
  container: {
    display: "flex",
    flexDirection: "column" as const,
    height: "90vh",
    width: "100%",
  },
  meetingDetails: {
    padding: "0, 16px, 16px, 16px",
    borderBottom: "1px solid #ddd",
    backgroundColor: "#fff",
  },
  roomName: {
    fontSize: "1.5rem",
    wordWrap: "break-word" as const,
    marginTop: 0
  },
  tabs: {
    display: "flex",
    borderBottom: "1px solid #ddd",
    backgroundColor: "#f9f9f9",
    padding: "8px 16px",
    position: "sticky" as const,
    top: 0,
    zIndex: 1,
  },
  tabButton: {
    marginRight: "16px",
    padding: "8px 16px",
    background: "transparent",
    border: "none",
    cursor: "pointer",
    fontSize: "1rem",
    color: "#333",
  },
  activeTab: {
    borderBottom: "2px solid #007BFF",
    color: "#007BFF",
    fontWeight: "bold",
  },
  tabContent: {
    flex: 1,
    overflowY: "auto" as const,
    backgroundColor: "#fff",
  },
};