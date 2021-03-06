﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StackExchange.Redis;

namespace AppFabricTest
{
    public abstract class CachePocBase : ICachePoc
    {
        protected string _regionName;
        protected int _datasetCount;
        protected int _dimensionCount;
        protected byte[] _data;
        protected Dictionary<string, object> _performanceTracker;
        protected IDatabase _myDefaultCache;
        protected ConnectionMultiplexer _redisConnectionMultiplexer;
        protected string _redisConnectionString;

        protected Stopwatch _stopWatch;

        protected const string _keyRegionName = "Region name";
        protected const string _keyDatasetCount = "Dataset region count";
        protected const string _keyDimensionCount = "Dimension count";
        protected const string _keyDataLength = "Data length";
        protected const string _keyMaxAdd = "Max add ms";
        protected const string _keyMinAdd = "Min add ms";
        protected const string _keyAvgAdd = "Avg add ms";
        protected const string _keySumAdd = "Sum add ms";
        protected const string _keyMaxGet = "Max get ms";
        protected const string _keyMinGet = "Min get ms";
        protected const string _keyAvgGet = "Avg get ms";
        protected const string _keySumGet = "Sum get ms";
        protected const string _keyMaxDel = "Max del ms";
        protected const string _keyMinDel = "Min del ms";
        protected const string _keyAvgDel = "Avg del ms";
        protected const string _keySumDel = "Sum del ms";
        protected const string _keyMemoryUsage = "Max bytes used during a dataset delete";

        public virtual void Initialize(ConnectionMultiplexer redisConnectionMultiplexer, string redisConnectionString, string regionName, int datasetCount, int dimensionCount, byte[] data, Dictionary<string, object> performanceTracker)
        {
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
            _redisConnectionString = redisConnectionString;
            _regionName = regionName;
            _datasetCount = datasetCount;
            _dimensionCount = dimensionCount;
            _data = data;
            _performanceTracker = performanceTracker;

            PrepareClient();
            FlushCache();
        }

        public abstract void DoWork();

        protected void FlushCache()
        {
            foreach (var endPoint in _redisConnectionMultiplexer.GetEndPoints())
            {
                _redisConnectionMultiplexer.GetServer(endPoint).FlushAllDatabases();
            }
        }

        public void PrepareClient()
        {
            _myDefaultCache = _redisConnectionMultiplexer.GetDatabase();
        }

        protected void RecordStatistics(List<long> addList, List<long> getAndRemoveList, List<long> memUsageList, string regionName = null)
        {
            _performanceTracker[_keyRegionName] = regionName;
            _performanceTracker[_keyDatasetCount] = _datasetCount;
            _performanceTracker[_keyDimensionCount] = _dimensionCount;
            _performanceTracker[_keyDataLength] = _data.Length;
            _performanceTracker[_keyMaxAdd] = addList.Max();
            _performanceTracker[_keyMinAdd] = addList.Min();
            _performanceTracker[_keyAvgAdd] = addList.Average();
            _performanceTracker[_keySumAdd] = addList.Sum();
            _performanceTracker[_keyMaxGet] = getAndRemoveList.Max();
            _performanceTracker[_keyMinGet] = getAndRemoveList.Min();
            _performanceTracker[_keyAvgGet] = getAndRemoveList.Average();
            _performanceTracker[_keySumGet] = getAndRemoveList.Sum();
            _performanceTracker[_keyMemoryUsage] = memUsageList.Max();
        }
    }
}
