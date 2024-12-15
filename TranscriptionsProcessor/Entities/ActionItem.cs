namespace TranscriptionsProcessor.Entities
{

    public class ActionItem
    {
        public string Id { get; set; } // Primary key
        public string Description { get; set; }
        public string? Deadline { get; set; }
        public string? ParticipantId { get; set; } // Nullable, if not always tied to a participant
        public string MeetingId { get; set; } // Foreign key
        public Meeting Meeting { get; set; } // Navigation property
        public Participant? Participant { get; set; } // Nullable navigation property
    }

}
