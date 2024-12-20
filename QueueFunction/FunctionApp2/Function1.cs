using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System;

public static class QueueTriggerFunction
{
    [FunctionName("QueueTriggerFunction")]
    public static async Task Run(
        [ServiceBusTrigger("messagequeue", Connection = "ServiceBus_ConnectionString")] string message,
        [Blob("initial/{message}", FileAccess.Read, Connection = "Blob_ConnectionString")] BlobClient inputBlobClient,
        [Blob("modified/{message}", FileAccess.Write, Connection = "Blob_ConnectionString")] BlobClient outputBlobClient,
        ILogger log)
    {
        log.LogInformation($"Queue trigger function processed message: {message}");

        // Modifier l'image (par exemple, redimensionner et ajouter un watermark)
        using (var memoryStream = new MemoryStream())
        {
            // Télécharger le blob source
            await inputBlobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;

            // Traitement de l'image avec ImageSharp
            using (Image image = Image.Load(memoryStream))
            {
                // Resize Image
                int width = image.Width / 2;
                int height = image.Height / 2;
                image.Mutate(x => x.Resize(width, height));

                // Uploader le blob modifié
                await outputBlobClient.UploadAsync(memoryStream, true);
            }
        }

        // Supprimer l'original si nécessaire
        await inputBlobClient.DeleteIfExistsAsync();
    }
}
