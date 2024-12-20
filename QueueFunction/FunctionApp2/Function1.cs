using System;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

public static class QueueFunctionWithMemoryStream
{
    [FunctionName("QueueFunctionWithMemoryStream")]
    public static void Run(
        [QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")] string queueItem,
        ILogger log)
    {
        log.LogInformation($"C# Queue trigger function processed: {queueItem}");

        try
        {
            // Convert the queue item to a MemoryStream
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(queueItem)))
            {
                // Read from the MemoryStream
                using (var reader = new StreamReader(memoryStream))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin); // Reset the position to start
                    string content = reader.ReadToEnd();
                    log.LogInformation($"Content from MemoryStream: {content}");
                }

                // Simulate writing processed data to the MemoryStream
                memoryStream.SetLength(0); // Clear the MemoryStream for new data
                using (var writer = new StreamWriter(memoryStream))
                {
                    writer.Write("Processed: " + queueItem);
                    writer.Flush(); // Ensure data is written to the stream
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    log.LogInformation($"Processed data stored in MemoryStream: {Encoding.UTF8.GetString(memoryStream.ToArray())}");
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError($"An error occurred: {ex.Message}");
        }
    }
}
