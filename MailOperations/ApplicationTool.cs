using OllamaSharp.Models.Chat;

namespace MailOperations;

public class ApplicationTool : Tool
{
    ApplicationTool()
    {
        Function = new Function
        {
            Description = "Extrahiert Firmennamen und Stellenbezeichnungen aus Mail Bodies und Headern, um die Informationen in einer strukturierten Form zurückzugeben.",
            Name = "get_application_info",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["from"] = new() { Type = "string", Description = "Sender der Bewerbungsmail" },
                    ["subject"] = new() { Type = "string", Description = "Betreffszeile der Bewerbungsmail" },
                    ["body"] = new() { Type = "string", Description = "Nachrichteninhalt der Bewerbungsmail" },
                    ["attachmentid"] = new() { Type = "string", Description = "AnhangId der Bewerbungsmail" }
                },
                Required = ["from", "body"]
            }
        };
        Type = "function";
    }
}