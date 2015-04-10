using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFabricTest
{
    public class CachePocChangeTracking : CachePocBase
    {
        private const string _regionChangeTracking = "RegionChangeTracking";

        public override void DoWork()
        {
            List<long> addList = new List<long>();
            List<long> getList = new List<long>();
            List<long> deleteList = new List<long>();
            List<long> memUsageList = new List<long>();

            List<string> datasets = new List<string>();
            for (var i = 0; i < _datasetCount; i++)
            {
                string datasetCode = "Dataset" + i;
                datasets.Add(datasetCode);
            }

            try
            { 
                _myDefaultCache.ClearRegion(_regionName);
            }
            catch { }
            try
            { 
                _myDefaultCache.ClearRegion(_regionChangeTracking);
            }
            catch { }
            _myDefaultCache.CreateRegion(_regionName);
            _myDefaultCache.CreateRegion(_regionChangeTracking);

            Task.Factory.StartNew(() => PrefillCache(datasets, addList)).Wait();
                                    
            GetObject(datasets, getList);

            RecordObjectModified(datasets, deleteList, memUsageList);

            RecordStatistics(addList, getList, memUsageList, _regionName);
        }

        private void PrefillCache(List<string> datasets, List<long> addList)
        {
            for (var i = 0; i < _datasetCount; i++)
            {
                string datasetKey = datasets[i];

                for (var j = 0; j < _dimensionCount; j++)
                {
                    string key = datasetKey + "DimensionName" + j;

                    _stopWatch = Stopwatch.StartNew();
                    var addResult = _myDefaultCache.Add(key, _data, _regionName);

                    _stopWatch.Stop();
                    addList.Add(_stopWatch.ElapsedMilliseconds);

                    if (addResult == null)
                    {
                        throw new Exception("Cache exception, adding item to the cache failed.");
                    }

                    // DEVNOTE: add the key into the tracking repository to have data loaded into that repo to simulate the get operation with a non empty repo
                    // normally we do not add the key to the cahnge tracking repo when adding an item to the cache
                    addResult = _myDefaultCache.Add(key, _data, _regionChangeTracking);
                }
            }
        }

        private void RecordObjectModified(List<string> datasets, List<long> deleteList, List<long> memUsageList)
        {
            //When deleting object:
            //1. Record the key of the changed object into the change tracking repository with the time of change

            // record the delete event (change date and time) into the tracking repository

            for (var i = 0; i < _datasetCount; i++)
            {
                string datasetKey = datasets[i];

                for (var j = 0; j < _dimensionCount; j++)
                {
                    string key = datasetKey + "DimensionName" + j;
                    
                    long bytes1 = GC.GetTotalMemory(false);
                    _stopWatch = Stopwatch.StartNew();

                    _myDefaultCache.Put(key, _regionChangeTracking);

                    _stopWatch.Stop();
                    deleteList.Add(_stopWatch.ElapsedMilliseconds);
                    memUsageList.Add(GC.GetTotalMemory(false) - bytes1);
                }
            }
        }

        private void GetObject(List<string> datasets, List<long> getList)
        {
            //When getting objects from the cache:
            //1. Object is in the cache (means not expired). Check whether it has been modified since added to the cache (object key is in the list of modified items). 
            //1.a If unchanged then retrieve from the cache
            //1.b If changed than retrieve from the data repository and also put into the cache (makes sense to delete the modified key entry from the change tracking repository)
            //2. Object is not in the cache (expired, not cached yet).
            //2.a Retrieve from the data repository and also put into the cache
            
            for (var i = 0; i < _datasetCount; i++)
            {
                string datasetKey = datasets[i];

                for (var j = 0; j < _dimensionCount; j++)
                {
                    string key = datasetKey + "DimensionName" + j;

                    _stopWatch = Stopwatch.StartNew();
                    var trackCacheItem = _myDefaultCache.GetCacheItem(key, _regionChangeTracking);

                    // DEVNOTE: in the final implementation if the trackCacheItem is not null then we may have to reload the data from the data repository
                    // as part of this task we only measure the worst case scenario retrieving an item from the cache which means having the above extra round-trip to check if the item is expired or not
                    var cacheItem = _myDefaultCache.GetCacheItem(key, _regionName);

                    _stopWatch.Stop();
                    getList.Add(_stopWatch.ElapsedMilliseconds);

                    if (cacheItem == null)
                    {
                        throw new Exception("Cache exception, getting item from the cache failed.");
                    }
                }
            }
        }
    }
}
