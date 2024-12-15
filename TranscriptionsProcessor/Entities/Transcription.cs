namespace TranscriptionsProcessor.Entities
{
    public class Transcription
    {
        public string MessageId { get; set; } // Primary key
        public string ParticipantId { get; set; } // Foreign key
        public string ParticipantName { get; set; } // Redundant for simplicity
        public string MeetingId { get; set; } // Foreign key
        public string FinalText { get; set; }
        public long Timestamp { get; set; }
        public string Language { get; set; }
        public Meeting Meeting { get; set; } // Navigation property
        public Participant Participant { get; set; } // Navigation property
    }
}
