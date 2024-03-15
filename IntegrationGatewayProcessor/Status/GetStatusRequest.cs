
using DurableTask.Core;
using Microsoft.DurableTask.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.Status
{
    public class GetStatusRequest
    {
        public GetStatusRequest()
        {
            StatussesToMatch = new List<OrchestrationRuntimeStatus>();
        }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedTo { get; set; }

        public List<OrchestrationRuntimeStatus> StatussesToMatch { get; set; }
    }
}
