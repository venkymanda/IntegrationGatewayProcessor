using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.Services
{
    public interface IAzureRelaySenderService
    {
        Task<bool> SendFileAsync(string blobName);
    }
}
