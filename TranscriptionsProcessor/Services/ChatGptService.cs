using OpenAI;
using OpenAI.Chat;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using TranscriptionsProcessor.DTOs;
using TranscriptionsProcessor.Entities;

namespace TranscriptionsProcessor.Services
{
    public class ChatGptService
    {
        private readonly OpenAIClient _openAiClient;

        public ChatGptService()
        {
            var apiKeyy = "set-the-chatGpt-api-key-here";
            _openAiClient = new OpenAIClient(apiKeyy);
        }

        public readonly IConfiguration _configuration;
        public async Task<string> TriggerOpenAI(Meeting meeting, List<object> conversation)
        {
            var conversationData = JsonSerializer.Serialize(conversation);

            var prompt = $"Act as a meeting/ class transcription analyst. Analyze the transcription and extract action items, decisions and meeting summary. " +
                $"For each action item, include the task description, the name of the assigned participant " +
                $"(if provided, give a meaningful name for the group incase the task is for many people like incase of a professor giving an assignment to the class), " +
                $"and their corresponding Id (ParticipantId, keep it empty if you can't find the ParticipantId as it is a foreging key in my schema). If deadlines are mentioned, include them as well (use the meeting time to calculate the deadline if possible. Example format: Next Wednesday (11 Dec, 2024). \n" +
                $" Meeting date time: {meeting.CreatedTimeStamp} to {meeting.DestroyedTimeStamp} \n" +
                $"Here is the meeting transcription: \n" +
                $"{conversationData}\n\n" +
                $"Provide the response in this JSON format:\n" +
                $"{{" +
                $"  \"actionItems\": [" +
                $"  {{" +
                $"      \"task\": \"string\"," +
                $"      \"assignedTo\": {{" +
                $"          \"name\": \"string (optional)\"," +
                $"          \"id\": \"string (optional)\"" +
                $"      }}," +
                $"      \"deadline\": \"string (optional)\"" +
                $"  }}" +
                $"]," +
                $"\"decisions\": [" +
                $"    \"string\"" +
                $"  ]" +
                $"\"summary\": \"string\"" +
                $"}}";



            var apiKey = "set-the-chatGpt-api-key-here"; 
            var baseUrl = "https://api.openai.com/v1/chat/completions";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var request = new OpenAIRequestDto
            {
                Model = "gpt-3.5-turbo",
                Messages = new List<OpenAIMessageRequestDto>{
                    new OpenAIMessageRequestDto
                    {
                        Role = "user",
                        Content = prompt
                    }
                },
                MaxTokens = 500
            };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(baseUrl, content);
            var resjson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = JsonSerializer.Deserialize<OpenAIErrorResponseDto>(resjson);
                throw new System.Exception(errorResponse.Error.Message);
            }
            var data = JsonSerializer.Deserialize<OpenAIResponseDto>(resjson);
            var responseText = data.choices[0].message.content;

            return responseText;
        }

    }
}
