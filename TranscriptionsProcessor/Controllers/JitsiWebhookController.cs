using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TranscriptionsProcessor.DTOs;
using TranscriptionsProcessor.Entities;
using TranscriptionsProcessor.WebhookModels;

namespace TranscriptionsProcessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JitsiWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public JitsiWebhookController(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("transcriptions")]
        public async Task<IActionResult> ProcessWebhookPayload(object payload)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Convert the object to a JSON string
            var jsonString = payload.ToString();

            var idempotencyKeyAdded = false;

            var webhookPayload = new WebhookPayload();

            try
            {
                // Deserialize the JSON string into WebhookPayload class
                webhookPayload = JsonSerializer.Deserialize<WebhookPayload>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                });

                if (webhookPayload == null)
                {
                    Console.WriteLine("Invalid payload received.");
                    return Ok();
                }

                // Check for duplicate processing using IdempotencyKey
                var existingKey = await dbContext.IdempotencyKeys.FindAsync(webhookPayload.IdempotencyKey);
                if (existingKey != null)
                {
                    Console.WriteLine("Duplicate request ignored.");
                    return Ok();
                }

                var meeting = await dbContext.Meetings.FindAsync(webhookPayload.SessionId);
                if (meeting is not null)
                {
                    // Add the IdempotencyKey
                    dbContext.IdempotencyKeys.Add(new IdempotencyKey
                    {
                        Key = webhookPayload.IdempotencyKey,
                        MeetingId = webhookPayload.SessionId 
                    });

                    // Save changes to the database
                    await dbContext.SaveChangesAsync();

                    idempotencyKeyAdded = true;
                }


                // Process the event based on the event type
                switch (webhookPayload.EventType)
                {
                    case "ROOM_CREATED":
                        await HandleRoomCreatedAsync(webhookPayload, dbContext);
                        break;

                    case "ROOM_DESTROYED":
                        await HandleRoomDestroyedAsync(webhookPayload, dbContext);
                        break;

                    case "TRANSCRIPTION_CHUNK_RECEIVED":
                        await HandleTranscriptionChunkAsync(webhookPayload, dbContext);
                        break;

                    default:
                        Console.WriteLine($"Unhandled event type: {webhookPayload.EventType}");
                        break;
                }

                // Save changes to the database
                await dbContext.SaveChangesAsync();

                if(idempotencyKeyAdded == false)
                {
                    meeting = await dbContext.Meetings.FindAsync(webhookPayload.SessionId);
                    if (meeting is not null)
                    {
                        dbContext.IdempotencyKeys.Add(new IdempotencyKey
                        {
                            Key = webhookPayload.IdempotencyKey,
                            MeetingId = webhookPayload.SessionId 
                        });

                        // Save changes to the database
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing webhook payload: {ex.Message}");
            }

            return Ok();
        }

        [HttpGet("participant-meetings/{email}")]
        public async Task<IActionResult> GetMeetingsByParticipant(string email)
        {
            try
            {

                var participants = await _context.Participants.Where(p => p.Email.ToLower() == email.Trim().ToLower())
                                                                .Select(p => p.ParticipantId)
                                                                .ToListAsync();

                var participantMeetings = await _context.Meetings.Where(m => m.Participants.Any(p => participants.Contains(p.ParticipantId)))
                                                                .Select(m => new MeetingDTO
                                                                {
                                                                    Id = m.Id,
                                                                    RoomName = m.RoomName,
                                                                    StartTime = m.CreatedTimeStamp,
                                                                    EndTime = m.DestroyedTimeStamp,
                                                                    ParticipantsCount = m.Participants.Count()
                                                                })
                                                                .ToListAsync();

                participantMeetings = participantMeetings.OrderByDescending(m => m.StartTime).ToList();

                return Ok(participantMeetings);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("meeting/{meetingId}")]
        public async Task<IActionResult> GetMeetingDetails(string meetingId)
        {
            try
            {
                var meeting = await _context.Meetings.Where(m => m.Id == meetingId)
                                                                .Select(m => new MeetingDTO
                                                                {
                                                                    Id = m.Id,
                                                                    RoomName = m.RoomName,
                                                                    StartTime = m.CreatedTimeStamp,
                                                                    EndTime = m.DestroyedTimeStamp,
                                                                    ParticipantsCount = m.Participants.Count()
                                                                })
                                                                .FirstOrDefaultAsync();

                if(meeting is null)
                {
                    throw new Exception("Meeting not found");
                }

                return Ok(meeting);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("transcriptions/{meetingId}")]
        public async Task<IActionResult> GetTranscriptions(string meetingId)
        {
            var speechList = await GetMeetingTranscription(meetingId);

            return Ok(speechList);
        }

        [HttpGet("action-items/{meetingId}")]
        public async Task<IActionResult> GetActionItems(string meetingId)
        {
            var summary = await _context.Summaries.FirstOrDefaultAsync(a => a.MeetingId == meetingId);

            if (summary is null)
            {
                try
                {
                    await GenerateActionItemsAndSummary(meetingId);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + "  \n\n " + ex.InnerException?.Message);
                }
            }

            var actionItems = await _context.ActionItems.Where(a => a.MeetingId == meetingId)
                                                        .Select(a => new ActionItemDTO
                                                        {
                                                            Id = a.Id,
                                                            Description = a.Description,
                                                            Deadline = a.Deadline,
                                                            ParticipantId = a.ParticipantId,
                                                            ParticipantName = a.Participant != null ? a.Participant.Name : null,
                                                            MeetingId = a.MeetingId
                                                        }).ToListAsync();

            return Ok(actionItems);
        }


        [HttpGet("decisions-summary/{meetingId}")]
        public async Task<IActionResult> GetSummaryAndDecisions(string meetingId)
        {
            var summary = await _context.Summaries.FirstOrDefaultAsync(a => a.MeetingId == meetingId);

            if (summary is null)
            {
                try
                {
                    await GenerateActionItemsAndSummary(meetingId);
                }
                catch(Exception ex)
                {
                    throw new Exception(ex.Message + "  \n\n " + ex.InnerException?.Message);
                }
            }

            summary = await _context.Summaries.FirstOrDefaultAsync(a => a.MeetingId == meetingId);

            return Ok(summary);
        }

        private async Task GenerateActionItemsAndSummary(string meetingId)
        {
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting is null)
            {
                throw new Exception("Meeting not found");
            }

            var speechList = await GetMeetingTranscription(meetingId);

            var chatGptService = new Services.ChatGptService();

            var res = await chatGptService.TriggerOpenAI(meeting, speechList);

            // Deserialize the ChatGPT response
            var gptResponse = JsonNode.Parse(res);

            // Extract Summary
            var summaryText = gptResponse["summary"]?.ToString();
            var decisions = gptResponse["decisions"]?.AsArray()
                .Select(d => d.ToString())
                .ToList();
            string decisionsString = decisions != null ? string.Join(" |& ", decisions) : string.Empty;

            // Create and save Summary
            var summary = new Summary
            {
                Id = Guid.NewGuid().ToString(),
                SummaryText = summaryText ?? "Error occurred",
                Decisions = decisionsString,
                MeetingId = meetingId
            };
            _context.Summaries.Add(summary);

            // Extract and save Action Items
            var actionItems = gptResponse["actionItems"]?.AsArray()
                .Select(item => new ActionItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = item?["task"]?.ToString() ?? "None",
                    ParticipantId = string.IsNullOrWhiteSpace(item?["assignedTo"]?["id"]?.ToString()) ? null : item?["assignedTo"]?["id"]?.ToString(),
                    Deadline = item?["deadline"]?.ToString(),
                    MeetingId = meetingId,
                }).ToList();

            if (actionItems != null)
            {
                _context.ActionItems.AddRange(actionItems);
            }

            // Save changes to database
            await _context.SaveChangesAsync();
        }

        private async Task<List<object>> GetMeetingTranscription(string meetingId)
        {
            var transcriptions = await _context.Transcriptions
                .Where(t => t.MeetingId == meetingId)
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            if (transcriptions == null || !transcriptions.Any())
                throw new Exception("No transcriptions found for the specified meeting.");

            var speechList = new List<object>();
            string? lastSpeakerName = null;
            string? lastSpeakerId = null;
            var currentSpeech = new List<string>();

            foreach (var transcription in transcriptions)
            {
                if (transcription.ParticipantId != lastSpeakerId)
                {
                    if (lastSpeakerId != null)
                    {
                        speechList.Add(new
                        {
                            SpeakerId = lastSpeakerId,
                            Speaker = lastSpeakerName,
                            Speech = string.Join(" ", currentSpeech)
                        });
                    }

                    currentSpeech.Clear();
                    lastSpeakerName = transcription.ParticipantName;
                    lastSpeakerId = transcription.ParticipantId;
                }

                currentSpeech.Add(transcription.FinalText);
            }

            // Add the final speaker's transcription
            if (lastSpeakerId != null)
            {
                speechList.Add(new
                {
                    Speaker = lastSpeakerName,
                    Speech = string.Join(" ", currentSpeech)
                });
            }

            return speechList;
        }


        private async Task HandleRoomCreatedAsync(WebhookPayload payload, ApplicationDbContext dbContext)
        {
            var meeting = await dbContext.Meetings.FindAsync(payload.SessionId);
            if(meeting is not null)
            {
                return;
            }

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(payload.Timestamp);

            meeting = new Meeting
            {
                Id = payload.SessionId,
                Fqn = payload.Fqn,
                RoomName = payload.Fqn.Split("/").Last(),
                CreatedTimeStamp = dateTimeOffset.LocalDateTime,
            };

            dbContext.Meetings.Add(meeting);
            Console.WriteLine("Room created and saved.");
        }

        private async Task HandleRoomDestroyedAsync(WebhookPayload payload, ApplicationDbContext dbContext)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(payload.Timestamp);
            var meeting = await dbContext.Meetings.FindAsync(payload.SessionId);
            if (meeting != null)
            {
                meeting.DestroyedTimeStamp = dateTimeOffset.LocalDateTime;
                Console.WriteLine("Room destroyed and timestamp updated.");
            }
            else
            {
                Console.WriteLine("Room not found for destruction event.");
            }
        }

        private async Task HandleTranscriptionChunkAsync(WebhookPayload payload, ApplicationDbContext dbContext)
        {
            var meeting = await dbContext.Meetings.FindAsync(payload.SessionId);
            if (meeting == null)
            {
                Console.WriteLine("Meeting not found for transcription event.");
                return;
            }

            // Extract the participant details
            var participantData = payload.Data.Participant;

            //Check if the participant already exists
           var participant = await dbContext.Participants
                .Include(p => p.Meetings)
               .FirstOrDefaultAsync(p => p.ParticipantId == participantData.Id);

            if (participant is null)
            {
                participant = new Participant
                {
                    ParticipantId = participantData?.Id ?? "",
                    JWTUserIdId = participantData?.UserId ?? "",
                    Name = participantData?.Name ?? "",
                    Email = participantData?.Email ?? "",
                    IsModerator = false, // Not supporting this for now
                    Meetings = new List<Meeting> { meeting }
                };

                dbContext.Participants.Add(participant);
                await dbContext.SaveChangesAsync();
            }
            else if(!participant.Meetings.Any(m => m.Id == meeting.Id))
            {
                participant.Meetings.Add(meeting);

                await dbContext.SaveChangesAsync();
            }

            // Add transcription entry
            var transcription = new Transcription
            {
                MessageId = payload.Data.MessageId ?? "",
                MeetingId = payload.SessionId,
                ParticipantId = participant?.ParticipantId ?? "",
                ParticipantName = participant?.Name ?? "",
                FinalText = payload.Data.Final ?? "",
                Timestamp = payload.Timestamp,
                Language = payload.Data.Language ?? "",
                Meeting = meeting,
                Participant = participant
            };

            dbContext.Transcriptions.Add(transcription);

            Console.WriteLine("Transcription chunk saved.");
            await dbContext.SaveChangesAsync();
        }
    }
}
