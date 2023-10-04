using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
//using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace IntegrationGatewayProcessor.Entity
{


    [JsonObject(MemberSerialization.OptIn)]
    public class FileChunkEntity
    {
        [JsonProperty("chunks")]
        public List<byte[]> Chunks { get; set; } = new List<byte[]>();

        [JsonProperty("lastSentChunkSequence")]
        public int LastSentChunkSequence { get; set; } = -1;

        // Method to set the LastSentChunkSequence
        public Task SetLastSentChunkSequenceAsync(int sequence)
        {
            LastSentChunkSequence = sequence;
            return Task.CompletedTask;
        }

        // Method to get the LastSentChunkSequence
        public Task<int> GetLastSentChunkSequenceAsync()
        {
            return Task.FromResult(LastSentChunkSequence);
        }

        // You can add other methods and logic as needed.
    }

    // TODO: Durable entitites are not yet implemented in Isokated process .net 6 once available rewrite it
    //[FunctionName("FileChunkEntity")]
    //public static Task Run(
    //    [EntityTrigger] IDurableEntityContext context)
    //{
    //    return context.DispatchAsync<FileChunkEntity>();
    //}

}
