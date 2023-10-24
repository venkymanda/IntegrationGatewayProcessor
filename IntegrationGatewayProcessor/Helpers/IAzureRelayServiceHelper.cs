using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.Helpers
{
    public interface IAzureRelayServiceHelper
    {
        Task<Stream> GetBlobStreamAsync(string blobContainerName, string blobName);
        void ConfigureHttpClient(HttpClient httpClient);

        string GetSasTokenAsync();

        int GetTotalChunks(string blobContainerName, string blobName, int chunkSize);
    }
}
