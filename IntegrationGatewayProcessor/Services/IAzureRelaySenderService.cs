using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegrationGatewayProcessor.Models;

namespace IntegrationGatewayProcessor.Services
{
    public interface IAzureRelaySenderService
    {
        Task<bool> SendFileAsync(BlobDTO blobName);
    }
}
