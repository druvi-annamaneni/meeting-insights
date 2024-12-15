import { getFormattedDateTime } from "./utility";

export default function MeetingCard({ roomName, startTime, endTime, participantsCount }: Meeting) {
  return (
    <div
      style={{
        border: "1px solid #ddd",
        borderRadius: "8px",
        padding: "16px",
        cursor: "pointer",
        width: "200px",
        backgroundColor: "#f9f9f9",
      }}
    >
      <h3 style={{
        whiteSpace: "normal",
        wordWrap: "break-word",
        overflow: "hidden",
        textOverflow: "ellipsis"
      }}>{roomName.split("@")[0]}</h3>
      <p>
        <strong>Start:</strong> 
        <div>{getFormattedDateTime(startTime)}</div>
      </p>
      <p>
        <strong>End:</strong> 
        <div>{getFormattedDateTime(endTime)}</div>
      </p>
      <p>
        <strong>Participants:</strong> {participantsCount}
      </p>
    </div>
  );
};

interface Meeting {
  id: string;
  roomName: string;
  startTime: string;
  endTime: string;
  participantsCount: number;
}
