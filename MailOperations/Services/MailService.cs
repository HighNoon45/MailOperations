using System.Web;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Google.Apis.Requests;
using Google.Apis.Util;
using HtmlAgilityPack;
using Google.Apis.Gmail.v1.Data;
using MailOperations.Services.Interfaces;

namespace MailOperations.Services;

public class MailService : IMailService
{
    private static readonly string ClientSecretPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "client_secret.json");
    private static UserCredential? _credential;
    private static readonly string[] Scopes = { GmailService.Scope.GmailReadonly };
    private static GmailService _service = new();

    private MailService()
    {
        InitializeService(ClientSecretPath).GetAwaiter().GetResult();
    }

    public async Task<List<Label>> RequestLabels()
    {
        var labels = new List<Label>();
        var response = await _service.Users.Labels.List("me").ExecuteAsync();
        foreach (var label in response.Labels)
            labels.Add(label);
        return labels;
    }
    
    public async Task<MailboxContent> RequestMessages(Label label,int maxResults)
    {
        BatchRequest batchRequest = new BatchRequest(_service);
        
        var parameters = new Repeatable<string>(enumeration: new[] { label.Id ?? "Label_1217953867591923163" });
        var mailsResponse = _service.Users.Messages.List("me");
        mailsResponse.LabelIds = parameters;
        mailsResponse.MaxResults = maxResults;
        var mails = await mailsResponse.ExecuteAsync();
        List<Message> messages = new();

        var responseCallback = new BatchRequest.OnResponse<Message>((message, error, index, httpResponse) =>
        {
         if (error != null)
             Console.WriteLine($"Error fetching message at index {index}: {error.Message}");
         else 
             messages.Add(message);
        });

        foreach (var mail in mails.Messages)
            batchRequest.Queue(_service.Users.Messages.Get("me", mail.Id),responseCallback);
        
        await batchRequest.ExecuteAsync();
        
        return await HtmlMessagesToMailboxContent(messages);
    }
    
    private async Task<MailboxContent> HtmlMessagesToMailboxContent(List<Message> messages)
    {
        List<MailContent> content = new List<MailContent>();
        foreach (var message in messages)
        {
            string from = message.Payload.Headers.FirstOrDefault(h => h.Name == "From")?.Value ?? string.Empty;
            string subject = message.Payload.Headers.FirstOrDefault(h => h.Name == "Subject")?.Value ?? string.Empty;
            string body = message.Payload.Body?.Data ?? string.Empty;
            string attachmentId = message.Payload.Body?.AttachmentId ?? string.Empty;
            
            string base64DecodedBody = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(body.Replace('-', '+').Replace('_', '/')));
            
            StringWriter htmlStringWriter = new StringWriter();
            HttpUtility.HtmlDecode(base64DecodedBody, htmlStringWriter);
            
            string cleanBody = htmlStringWriter.ToString();

            MailContent mailContent = new() { From = from, Subject = subject, Body = await GetHtmlInnerText(cleanBody), Attachments = attachmentId };
            
            content.Add(mailContent);
        }
        
        return new MailboxContent { Mails = content };
    }
    private async Task<string> GetHtmlInnerText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Select all <p>,<div> nodes
        var pNodes = doc.DocumentNode.SelectNodes("//p | //div");

        string result = string.Empty;

        if (pNodes == null)
        {
            return html;
        }
        
        // Extract InnerText from each node
        result = pNodes.Select(node => node.InnerText).ToList().Aggregate((current, next) => current + "\n" + next);

        // None of the nodes had <p> tags, return the original HTML
        if (string.IsNullOrEmpty(result))
        {
            return html;
        }
        
        return result;
    }
    private async Task InitializeService(string clientSecretPath)
    {
        await using (var stream = new FileStream(clientSecretPath, FileMode.Open, FileAccess.Read))
        {
            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStreamAsync(stream).Result.Secrets,
                Scopes,
                "me", CancellationToken.None, new FileDataStore("MailOperations.DataStore"));
        }
        
        _service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential,
            ApplicationName = "MailOperations",
        });
    }
}