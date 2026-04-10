using Google.Apis.Gmail.v1.Data;

namespace MailOperations.Services.Interfaces;

public interface IMailService
{
    public Task<List<Label>> RequestLabels();
    public Task<MailboxContent> RequestMessages(Label label, int maxResults);
}