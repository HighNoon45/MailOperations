namespace MailOperations;

public record MailboxContent
{
    public required List<MailContent> Mails { get; init; }
}

public record MailContent
{
    public string From { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Attachments { get; init; } = string.Empty;
}