using Telegram.Bot;
using Telegram.Bot.Types;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

var client = new TelegramBotClient(""); // insert here your TTG bot apikey
List<Message> messages = new List<Message>();
string endpoint = "https://api.openai.com/v1/chat/completions";
string apiKey = ""; // insert here your chatGPT apikey

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

client.StartReceiving(Update, Error);

async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
{
    var message = update.Message;
    while (true)
    {
        string content = update.Message.Text;
        var aiMessage = new Message() { Role = "user", Content = content };
        messages.Add(aiMessage);
        var requestData = new Request()
        {
            ModelId = "gpt-3.5-turbo",
            Messages = messages
        };
        using var response = await httpClient.PostAsJsonAsync(endpoint, requestData);
        ResponseData? responseData = await response.Content.ReadFromJsonAsync<ResponseData>();

        var choices = responseData?.Choices ?? new List<Choice>();
        if (choices.Count == 0)
        {
            Console.WriteLine("No choices were returned by the API");
        }
        var choice = choices[0];
        var responseMessage = choice.Message;
        messages.Add(responseMessage);
        var responseText = responseMessage.Content.Trim();
        await botClient.SendTextMessageAsync(message.Chat.Id, responseText);
        break;
    }
}

Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
{
    throw new NotImplementedException();
}

class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}
class Request
{
    [JsonPropertyName("model")]
    public string ModelId { get; set; } = "";
    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();
}

class ResponseData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    [JsonPropertyName("object")]
    public string Object { get; set; } = "";
    [JsonPropertyName("created")]
    public ulong Created { get; set; }
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();
    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = new();
}

class Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    [JsonPropertyName("message")]
    public Message Message { get; set; } = new();
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = "";
}

class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}