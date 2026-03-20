using System.Text.Json;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace MailOperations;

// Schreib mir aus folgenden Json Daten den Firmennamen der Ablehnenden Firma und die Begründung für die Ablehnung heraus im Json Format: 

public static class LlmService
{
    private static Uri _uri = new Uri("http://localhost:11434");
    
    public static async Task ProcessMails(MailboxContent content, Model model, string query , Tool ollamaTool)
    {
        var ollama = new OllamaApiClient(_uri, model.Name);
        
        var chat = new Chat(ollama);
        
        var jsonRequest = JsonSerializer.Serialize<MailContent>(content.Mails[0]);
        
        string returnedJson = string.Empty;
        
        await foreach (var answerToken in chat.SendAsync(query +jsonRequest, [ollamaTool], format:"json"))
            returnedJson += answerToken;
        
        Console.WriteLine("Returned JSON: " + returnedJson);
    }
    
    public static async Task<IEnumerable<Model>> GetAiModels(){
        var ollama = new OllamaApiClient(_uri);
        return await ollama.ListLocalModelsAsync();
    }
}