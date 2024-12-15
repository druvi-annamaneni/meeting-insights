namespace TranscriptionsProcessor.Entities
{

    public class Participant
    {
        public string ParticipantId { get; set; } // Primary key
        public string JWTUserIdId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsModerator { get; set; }
        public List<Meeting> Meetings { get; set; } = new();
    }
}
