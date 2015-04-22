using System.Collections.Generic;
using TagCache.Redis;

namespace AppFabricTest
{
    public interface ICachePoc: ICacheBenchmark
    {
        void Initialize(RedisConnectionManager endPoint, string regionName, int datasetCount, int dimensionCount, byte[] data, Dictionary<string, object> performanceTracker);
    }
}
