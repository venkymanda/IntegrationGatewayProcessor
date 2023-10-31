using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.Models
{
    public class SuccessCallbackDTO
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int ProcessedChunks { get; set; }
        public long TotalBytesProcessed { get; set; }
        public string TransactionId { get; set; }
        public string DocumentId { get; set; }
    }

}
