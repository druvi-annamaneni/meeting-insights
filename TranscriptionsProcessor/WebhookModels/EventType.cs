using System.Text.Json.Serialization;

namespace TranscriptionsProcessor.WebhookModels
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EventType
    {
        ROOM_CREATED,
        PARTICIPANT_LEFT,
        PARTICIPANT_LEFT_LOBBY,
        TRANSCRIPTION_UPLOADED,
        CHAT_UPLOADED,
        ROOM_DESTROYED,
        PARTICIPANT_JOINED,
        PARTICIPANT_JOINED_LOBBY,
        RECORDING_STARTED,
        RECORDING_ENDED,
        RECORDING_UPLOADED,
        LIVE_STREAM_STARTED,
        LIVE_STREAM_ENDED,
        SETTINGS_PROVISIONING,
        SIP_CALL_IN_STARTED,
        SIP_CALL_IN_ENDED,
        SIP_CALL_OUT_STARTED,
        SIP_CALL_OUT_ENDED,
        FEEDBACK,
        DIAL_IN_STARTED,
        DIAL_IN_ENDED,
        DIAL_OUT_STARTED,
        DIAL_OUT_ENDED,
        USAGE,
        SPEAKER_STATS,
        POLL_CREATED,
        POLL_ANSWER,
        REACTIONS,
        AGGREGATED_REACTIONS,
        SCREEN_SHARING_HISTORY,
        VIDEO_SEGMENT_UPLOADED,
        ROLE_CHANGED,
        RTCSTATS_UPLOADED,
        TRANSCRIPTION_CHUNK_RECEIVED,
        UNKNOWN
    }
}
