namespace TranscriptionsProcessor.WebhookModels
{
    public class WebhookData
    {
        public string? Final { get; set; } // Final transcription phrase
        public string? Language { get; set; } // Language of the transcription
        public string? MessageId { get; set; } // Unique message identifier
        public string? Conference { get; set; }
        public ParticipantData? Participant { get; set; } // Participant information
    }
}
