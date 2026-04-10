using OllamaSharp.Models.Chat;

namespace MailOperations.Tools;


public class RejectionTool : Tool
{
    public RejectionTool()
    {
        Function = new Function
        {
            Description = "Extrahiert Firmennamen und Ablehnungs-Begründungen aus Mail Body und Header einer Ablehnungsmail",
            Name = "get_rejection_info",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["firmenname"] = new() { Type = "string", Description = "Bezeichnung der ablehnenden Firma" },
                    ["grund"] = new() { Type = "string", Description = "Begründung der Ablehnung" }
                },
                Required = ["firmenname", "grund"]
            }
        };
        Type = "function";
    }
}

