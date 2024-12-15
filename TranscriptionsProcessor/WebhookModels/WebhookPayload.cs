using System.Text.Json.Serialization;

namespace TranscriptionsProcessor.WebhookModels
{
    public class WebhookPayload
    {
        public string IdempotencyKey { get; set; }
        public string CustomerId { get; set; } 
        public string AppId { get; set; } 
        public string EventType { get; set; } 
        public string SessionId { get; set; } 
        public string Fqn { get; set; } 
        public long Timestamp { get; set; } 
        public WebhookData Data { get; set; }
    }
}
