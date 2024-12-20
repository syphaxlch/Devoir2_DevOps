using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;

public class BlobTriggerFunction
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _queueName = Environment.GetEnvironmentVariable("messagequeue");

    public BlobTriggerFunction(ServiceBusClient serviceBusClient)
    {
        _serviceBusClient = serviceBusClient;
    }

    [Function("BlobTriggerFunction")]
    public async Task Run(
        [BlobTrigger("initialcontainer/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob,
        string name,
        FunctionContext context)
    {
        var log = context.GetLogger("BlobTriggerFunction");
        log.LogInformation($"Blob Trigger détecté : Nom du fichier : {name}, Taille : {myBlob.Length} bytes");

        try
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);
            ServiceBusMessage message = new ServiceBusMessage(name);

            await sender.SendMessageAsync(message);
            log.LogInformation($"Nom du fichier envoyé dans la queue : {name}");
        }
        catch (Exception ex)
        {
            log.LogError($"Erreur lors de l'envoi du message : {ex.Message}");
        }
    }
}
