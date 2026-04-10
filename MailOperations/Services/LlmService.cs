using System.Text.Json;
using MailOperations.Services.Interfaces;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace MailOperations.Services;

// Standard LLM Query: Schreib mir aus folgenden Json Daten den Firmennamen der Ablehnenden Firma und die Begründung für die Ablehnung heraus im Json Format: 

// TODO: Look into static class task disposal
public class LlmService : ILlmService
{
    private static readonly Uri Uri = new ("http://localhost:11434");
    
    public async Task ProcessMails(MailboxContent content, Model model, string query , Tool ollamaTool)
    {
        var ollama = new OllamaApiClient(Uri, model.Name);
        
        var chat = new Chat(ollama);
        
        var jsonRequest = JsonSerializer.Serialize<MailContent>(content.Mails[0]);
        
        string returnedJson = string.Empty;
        
        await foreach (var answerToken in chat.SendAsync(query + jsonRequest, [ollamaTool], format:"json"))
            returnedJson += answerToken;
        
        Console.WriteLine("Returned JSON: " + returnedJson);
    }
    
    public async Task<IEnumerable<Model>> GetAiModels(){
        var ollama = new OllamaApiClient(Uri);
        return await ollama.ListLocalModelsAsync();
    }
}