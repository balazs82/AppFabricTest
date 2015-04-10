using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFabricTest
{
    public interface ICachePoc: ICacheBenchmark
    {
        void Initialize(DataCacheServerEndpoint endPoint, string regionName, int datasetCount, int dimensionCount, byte[] data, Dictionary<string, object> performanceTracker);
    }
}
