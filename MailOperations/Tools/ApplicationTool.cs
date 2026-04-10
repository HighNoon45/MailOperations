using OllamaSharp.Models.Chat;

namespace MailOperations.Tools;

public class ApplicationTool : Tool
{
    ApplicationTool()
    {
        Function = new Function
        {
            Description = "Extrahiert Firmenname und Stellenbezeichnung aus Mail Body und Header einer Bewerbungsmail",
            Name = "get_application_info",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["firmenname"] = new() { Type = "string", Description = "Firma bei der sich beworben wurde" },
                    ["stellenbezeichnung"] = new() { Type = "string", Description = "Bezeichnung der Stelle auf die sich beworben wurde" }
                },
                Required = ["firmenname", "stellenbezeichnung"]
            }
        };
        Type = "function";
    }
}