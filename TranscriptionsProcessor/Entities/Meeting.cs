using TranscriptionsProcessor.WebhookModels;

namespace TranscriptionsProcessor.Entities
{

    public class Meeting
    {
        public string Id { get; set; } // Primary key Meeting Id
        public string Fqn { get; set; }
        public string RoomName { get; set; } // Conference name
        public DateTime CreatedTimeStamp { get; set; }
        public DateTime? DestroyedTimeStamp { get; set; }
        public List<Participant> Participants { get; set; } = new();
        public List<Transcription> Transcriptions { get; set; } = new();
        public Summary? Summary { get; set; }
        public List<ActionItem> ActionItems { get; set; } = new();
    }
}
