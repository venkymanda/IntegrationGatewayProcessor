using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.Maintenance
{
   
    public class TerminateModel
    {
        public string CallbackUrl { get; set; }
        public string Reason { get; set; }
    }
}
