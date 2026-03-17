using OllamaSharp.Models.Chat;

namespace MailOperations;


public class RejectionTool : Tool
{
    public RejectionTool()
    {
        Function = new Function
        {
            Description = "Extrahiert Firmennamen und Ablehnungs-Begründungen aus Mail Bodies und Headern, um die Informationen in einer strukturierten Form zurückzugeben.",
            Name = "get_rejection_info",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["from"] = new() { Type = "string", Description = "Sender der Ablehnungsmail" },
                    ["subject"] = new() { Type = "string", Description = "Betreffszeile der Ablehnungsmail" },
                    ["body"] = new() { Type = "string", Description = "Nachrichteninhalt der Ablehnungsmail" },
                    ["attachmentid"] = new() { Type = "string", Description = "AnhangId der Ablehnungsmail" }
                },
                Required = ["from", "body"]
            }
        };
        Type = "function";
    }
}

