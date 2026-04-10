using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace MailOperations.Services.Interfaces;

public interface ILlmService
{
    public Task ProcessMails(MailboxContent content, Model model, string query, Tool ollamaTool);
    public Task<IEnumerable<Model>> GetAiModels();
}