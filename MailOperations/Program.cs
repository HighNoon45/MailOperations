using System.Text.Json;
using System.Web;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Util.Store;
using Google.Apis.Requests;
using Google.Apis.Util;
using HtmlAgilityPack;
using OllamaSharp;

namespace MailOperations;

class Program
{
    static async Task Main(string[] args)
    {
        var msgs = await RequestMessages();
        var content = RawMessagesToMailboxContent(msgs);
        
        await ProcessMails(content);
        
        Console.ReadLine();
    }

    private static async Task<List<Message>> RequestMessages()
    {
        string clientSecretPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "client_secret.json");
        
        UserCredential credential;
        
        await using (var stream = new FileStream(clientSecretPath, FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStreamAsync(stream).Result.Secrets,
                new[] { GmailService.Scope.GmailReadonly },
                "me", CancellationToken.None, new FileDataStore("Mails.Labels"));
        }
        
        var service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "MailOperations",
        });
        
        BatchRequest batchRequest = new BatchRequest(service);
        
        var parameters = new Repeatable<string>(enumeration: new[] { "Label_1217953867591923163" });
        var mailsResponse = service.Users.Messages.List("me");
        mailsResponse.LabelIds = parameters;
        mailsResponse.MaxResults = 10;
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
            batchRequest.Queue(service.Users.Messages.Get("me", mail.Id),responseCallback);
        
        await batchRequest.ExecuteAsync();
        
        return messages;
    }
    
    private static MailboxContent RawMessagesToMailboxContent(List<Message> messages)
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

            MailContent mailContent = new() { From = from, Subject = subject, Body = GetParagraphText(cleanBody), Attachments = attachmentId };
            
            content.Add(mailContent);
        }
        return new MailboxContent() { Mails = content };
    }
    
    private static string GetParagraphText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Select all <p> nodes
        var pNodes = doc.DocumentNode.SelectNodes("//p");

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
    
    private static async Task ProcessMails(MailboxContent content)
    {
        var uri = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri, "mistral");
        
        var chat = new Chat(ollama);
        
        var jsonRequest = JsonSerializer.Serialize<MailContent>(content.Mails[0]);
        
        string returnedJson = string.Empty;
        
         await foreach (var answerToken in chat.SendAsync("Schreib mir aus folgenden Json Daten den Firmennamen der Ablehnenden Firma und die Begründung für die Ablehnung heraus im Json Format: "
                                                          +jsonRequest, [new RejectionTool()], format:"json"))
             returnedJson += answerToken;
        
        Console.WriteLine("Returned JSON: " + returnedJson);
    }
}