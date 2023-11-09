using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.Models
{
    public class BlobDTO
    {
        public string? BlobContainerName { get; set; }
        public string? BlobName { get; set; }
        public int CurrentChunkSequence { get; set; }
        public int ChunkSize { get; set; }
        public long TotalChunks { get; set; }
        public byte[]? Data { get; set; }
        public long TotalSize { get; set; }
        public string? TransactionId { get; set; }
        public string? InputRequest { get; set; }
    }

}
