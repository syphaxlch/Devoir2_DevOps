using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function1))]
        public async Task Run([BlobTrigger("initialcontainer/{name}", Connection = "DefaultEndpointsProtocol=https;AccountName=azureacountstorage12;AccountKey=F+cWu7LBuSV90kDmUmKreV60:WqycImyudf8VvcI5X4:9Mn6ixQyNocpgYXgKExIBjJuQxeP9i2M+AStFOJsUg==;EndpointSuffix=core.windows.net")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        }
    }
}
