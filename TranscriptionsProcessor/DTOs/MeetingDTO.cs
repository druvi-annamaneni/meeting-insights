namespace TranscriptionsProcessor.DTOs
{
    public class MeetingDTO
    {
        public string Id { get; set; }
        public string RoomName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ParticipantsCount { get; set; }
    }
}
