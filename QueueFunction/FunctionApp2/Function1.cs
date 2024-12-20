using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using System;

public static class QueueTriggerFunction
{
    [FunctionName("QueueTriggerFunction")]
    public static async Task Run(
        [ServiceBusTrigger("messagequeue", Connection = "ServiceBus_ConnectionString")] string message,
        [Blob("initial/{message}", FileAccess.ReadWrite, Connection = "Blob_ConnectionString")] Stream blobStream,
        [Blob("modified/{message}", FileAccess.Write, Connection = "Blob_ConnectionString")] CloudBlockBlob outputBlob,  // Mise à jour ici pour utiliser 'modified'
        [Blob("initial/{message}", FileAccess.Delete, Connection = "Blob_ConnectionString")] CloudBlockBlob inputBlob,
        ILogger log)
    {
        log.LogInformation($"Queue trigger function processed message: {message}");

        // Modifier l'image (par exemple, redimensionner et ajouter un watermark)
        using (MemoryStream memoryStream = new MemoryStream())
        {
            blobStream.CopyTo(memoryStream);
            memoryStream.Position = 0; // Reset stream position

            // Traitement de l'image avec ImageSharp
            using (Image image = Image.Load(memoryStream))
            {
                int width = image.Width / 2;
                int height = image.Height / 2;
                image.Mutate(x => x.Resize(width, height));

                // Ajouter le watermark "UQAC"
                var font = SystemFonts.CreateFont("Arial", 24); // Choix de la police
                var color = Color.White; // Couleur du texte
                var position = new PointF(image.Width - 150, image.Height - 50); // Position du watermark

                image.Mutate(x => x.DrawText("UQAC", font, color, position));

                // Sauvegarder l'image modifiée
                using (var outputMemoryStream = new MemoryStream())
                {
                    image.Save(outputMemoryStream, new JpegEncoder());
                    outputMemoryStream.Position = 0;
                    await outputBlob.UploadFromStreamAsync(outputMemoryStream);
                }
            }
        }

        // Supprimer l'ancien fichier dans le conteneur 'initial'
        if (inputBlob.Exists())
        {
            log.LogInformation($"Deleting the original blob: {message}");
            await inputBlob.DeleteIfExistsAsync();
        }
    }
}
