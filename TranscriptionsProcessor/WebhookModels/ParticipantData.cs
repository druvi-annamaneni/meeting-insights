namespace TranscriptionsProcessor.WebhookModels
{
    public class ParticipantData
    {
        public string Id { get; set; } // Participant's unique ID
        public string Name { get; set; } // Participant's name
        public string UserId { get; set; } // Participant's user ID
        public string Email { get; set; } // Participant's email
        public string AvatarUrl { get; set; } // Participant's avatar URL
    }
}
