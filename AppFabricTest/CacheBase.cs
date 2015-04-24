using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace AppFabricTest
{
    public class CacheBenchmark : ICacheBenchmark
    {
        private const string _regionName = "Region0";

        private readonly int _datasetCount = 1;
        private readonly int _dimensionCount = 1;
        private readonly byte[] _data = new byte[300000];
        private readonly ICachePoc _cacheStrategy;
        private readonly ConnectionMultiplexer _redisConnectionMultiplexer;
        private readonly string _redisConnectionString;
                
        private Dictionary<string, Dictionary<string, object>> _performanceTracker;

        public CacheBenchmark(ICachePoc cacheStrategy, ConnectionMultiplexer redisConnectionMultiplexer, string redisConnectionString, int datasetCount, int dimensionCount, Dictionary<string, Dictionary<string, object>> performanceTracker)
        {
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
            _redisConnectionString = redisConnectionString;
            _datasetCount = datasetCount;
            _dimensionCount = dimensionCount;
            _cacheStrategy = cacheStrategy;
            _performanceTracker = performanceTracker;
            
            Dictionary<string, object> perfTracker;
            if (_performanceTracker.Any(t => t.Key == cacheStrategy.GetType().Name))
                perfTracker = _performanceTracker[cacheStrategy.GetType().Name];
            else
            {
                _performanceTracker.Add(cacheStrategy.GetType().Name, new Dictionary<string, object>());
                perfTracker = _performanceTracker[cacheStrategy.GetType().Name];
            }

            _cacheStrategy.Initialize(redisConnectionMultiplexer, redisConnectionString, _regionName, _datasetCount, _dimensionCount, _data, perfTracker);
        }

        public void DoWork()
        {
            _cacheStrategy.DoWork();
        }
    }
}
