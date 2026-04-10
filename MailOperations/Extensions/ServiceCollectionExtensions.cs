using MailOperations.Services;
using MailOperations.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MailOperations.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddMailOperationsServices(this IServiceCollection collection)
    {
        collection.AddSingleton<IMailService, MailService>();
        collection.AddSingleton<ILlmService, LlmService>();
    }
}