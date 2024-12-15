namespace TranscriptionsProcessor.Entities
{

    public class Summary
    {
        public string Id { get; set; } // Primary key
        public string SummaryText { get; set; }
        public string Decisions { get; set; }
        public string MeetingId { get; set; } // Foreign key
        public Meeting Meeting { get; set; } // Navigation property
    }

}
