// Importation des bibliothèques nécessaires
using System; // Pour accéder aux fonctionnalités de base comme Environment
using System.IO; // Pour gérer les flux de données, ici pour les fichiers
using System.Threading.Tasks; // Pour permettre l'utilisation de tâches asynchrones
using Microsoft.Azure.Functions.Worker; // Pour accéder aux fonctions Azure
using Microsoft.Extensions.Logging; // Pour ajouter des capacités de journalisation
using Azure.Messaging.ServiceBus; // Pour interagir avec Azure Service Bus

// Définition de la classe principale de la fonction
public class BlobTriggerFunction
{
    // Déclaration de la variable client ServiceBus et du nom de la queue
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _queueName = Environment.GetEnvironmentVariable("messagequeue"); // Récupération du nom de la queue depuis les variables d'environnement

    // Constructeur de la classe, injection du ServiceBusClient pour envoyer des messages
    public BlobTriggerFunction(ServiceBusClient serviceBusClient)
    {
        _serviceBusClient = serviceBusClient; // Initialisation du client Service Bus
    }

    // Définition de la fonction Azure déclenchée par un fichier Blob
    [Function("BlobTriggerFunction")] // Attribut indiquant que c'est une fonction Azure avec un déclencheur Blob
    public async Task Run(
        // Définition des paramètres du déclencheur Blob
        [BlobTrigger("initialcontainer/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob, // Déclencheur qui capte le fichier Blob et le flux associé
        string name, // Le nom du fichier Blob capturé
        FunctionContext context) // Le contexte de la fonction (utile pour la journalisation)
    {
        // Récupération du logger pour cette fonction
        var log = context.GetLogger("BlobTriggerFunction"); 

        // Enregistrement dans les logs du nom du fichier et de sa taille
        log.LogInformation($"Blob Trigger détecté : Nom du fichier : {name}, Taille : {myBlob.Length} bytes");

        try
        {
            // Création du sender pour envoyer des messages dans la queue Service Bus
            ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName); 
            
            // Création du message à envoyer dans la queue Service Bus avec le nom du fichier
            ServiceBusMessage message = new ServiceBusMessage(name);

            // Envoi du message de façon asynchrone dans la queue
            await sender.SendMessageAsync(message);
            
            // Enregistrement dans les logs de l'envoi réussi
            log.LogInformation($"Nom du fichier envoyé dans la queue : {name}");
        }
        catch (Exception ex) // Gestion des exceptions en cas d'erreur
        {
            // Enregistrement de l'erreur dans les logs
            log.LogError($"Erreur lors de l'envoi du message : {ex.Message}");
        }
    }
}
