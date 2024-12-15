namespace TranscriptionsProcessor.DTOs
{
    public class ActionItemDTO
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string? Deadline { get; set; }
        public string? ParticipantId { get; set; }
        public string? ParticipantName { get; set; }
        public string MeetingId { get; set; }
    }
}
