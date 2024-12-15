namespace TranscriptionsProcessor.Entities
{

    public class IdempotencyKey
    {
        public string Key { get; set; } // Primary key
        public string? MeetingId { get; set; } // Optional association with a meeting
        public Meeting? Meeting { get; set; } // Nullable navigation property
    }
}
