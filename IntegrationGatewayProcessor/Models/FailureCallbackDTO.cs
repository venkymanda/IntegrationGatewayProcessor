using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.Models
{
    public class FailureCallbackDTO
    {
        public string? Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetails { get; set; }
        public string? TransactionId { get; set; }
        public string? DocumentId { get; set; }
    }

}
