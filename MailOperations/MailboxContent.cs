namespace MailOperations;

public record MailboxContent
{
    public List<MailContent> Mails { get; init; }
}

public record MailContent()
{
    public string From { get; init; }
    public string Subject { get; init; }
    public string Body { get; init; }
    public string Attachments { get; init; }
}