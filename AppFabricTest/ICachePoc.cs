using System.Collections.Generic;
using StackExchange.Redis;

namespace AppFabricTest
{
    public interface ICachePoc: ICacheBenchmark
    {
        void Initialize(ConnectionMultiplexer redisConnectionMultiplexer, string redisConnectionString, string regionName, int datasetCount, int dimensionCount, byte[] data, Dictionary<string, object> performanceTracker);
    }
}
