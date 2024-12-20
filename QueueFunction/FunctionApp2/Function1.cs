// Importation des bibliothèques nécessaires
using Azure.Storage.Blobs; // Pour interagir avec Azure Blob Storage
using Azure.Storage.Blobs.Specialized; // Pour utiliser les clients spécifiques de Blob (BlobClient)
using SixLabors.ImageSharp; // Pour travailler avec des images (traitement et manipulation)
using SixLabors.ImageSharp.Formats.Jpeg; // Pour supporter le format JPEG lors de l'enregistrement
using SixLabors.ImageSharp.Processing; // Pour appliquer des traitements sur les images
using Microsoft.Azure.WebJobs; // Pour utiliser les déclencheurs et l'infrastructure des fonctions Azure
using Microsoft.Extensions.Logging; // Pour gérer la journalisation
using System.IO; // Pour gérer les flux de données (par exemple, MemoryStream)
using System.Threading.Tasks; // Pour utiliser les tâches asynchrones
using System; // Pour utiliser les types de base comme Exception et les fonctions de système

// Définition de la fonction déclenchée par un message dans la queue
public static class QueueTriggerFunction
{
    // Attribut de la fonction Azure, déclarant un déclencheur de ServiceBus
    [FunctionName("QueueTriggerFunction")]
    public static async Task Run(
        // Paramètre pour recevoir le message de la queue ServiceBus
        [ServiceBusTrigger("messagequeue", Connection = "ServiceBus_ConnectionString")] string message,
        
        // Paramètre pour obtenir le blob d'entrée à partir de son nom
        [Blob("initial/{message}", FileAccess.Read, Connection = "Blob_ConnectionString")] BlobClient inputBlobClient,

        // Paramètre pour écrire le blob modifié dans un autre conteneur
        [Blob("modified/{message}", FileAccess.Write, Connection = "Blob_ConnectionString")] BlobClient outputBlobClient,

        // Paramètre pour la journalisation
        ILogger log)
    {
        // Journalisation du traitement du message de la queue
        log.LogInformation($"Queue trigger function processed message: {message}");

        // Création d'un MemoryStream pour traiter le fichier Blob en mémoire
        using (var memoryStream = new MemoryStream())
        {
            // Télécharger le contenu du blob d'entrée dans le MemoryStream
            await inputBlobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0; // Réinitialiser la position pour que l'image puisse être lue à partir du début

            // Traitement de l'image à l'aide de ImageSharp
            using (Image image = Image.Load(memoryStream))
            {
                // Redimensionner l'image (réduire de moitié)
                int width = image.Width / 2;
                int height = image.Height / 2;
                image.Mutate(x => x.Resize(width, height)); // Appliquer le redimensionnement

                // Sauvegarder le fichier modifié dans le Blob de sortie
                await outputBlobClient.UploadAsync(memoryStream, true); // 'true' pour écraser le fichier si déjà existant
            }
        }

        // Supprimer le blob d'origine, si nécessaire, une fois qu'il a été traité
        await inputBlobClient.DeleteIfExistsAsync();
    }
}
