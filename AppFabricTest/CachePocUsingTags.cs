using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AppFabricTest
{
    public class CachePocUsingTags : CachePocBase
    {    
        public override void DoWork()
        {
            List<long> addList = new List<long>();
            List<long> getAndRemoveList = new List<long>();
            List<long> memUsageList = new List<long>();

            List<string> datasetTags = new List<string>();

            for (var i = 0; i < _datasetCount; i++)
            {
                datasetTags.Add("Dataset" + i);
                
                for (var j = 0; j < _dimensionCount; j++)
                {
                    string key = datasetTags[i] + "DimensionName" + j;

                    _stopWatch = Stopwatch.StartNew();

                    _myDefaultCache.StringSet(key, _data);
                    var addResult = _myDefaultCache.StringGet(key);

                    _stopWatch.Stop();
                    addList.Add(_stopWatch.ElapsedMilliseconds);

                    if (addResult.IsNull)
                    {
                        Console.WriteLine("**FAIL----->Add-Object did not add to cache - FAIL");
                    }
                    else
                    {
                        _myDefaultCache.SetAdd(datasetTags[i], key);
                    }
                }
            }

            var v = _myDefaultCache.StringGet("Dataset0DimensionName0");

            foreach (string dataCacheTag in datasetTags)
            {
                long bytes1 = GC.GetTotalMemory(false);
                _stopWatch = Stopwatch.StartNew();

                foreach (var key in _myDefaultCache.SetMembers(dataCacheTag))
                {
                    _myDefaultCache.KeyDeleteAsync((string)key);
                }
                _myDefaultCache.KeyDeleteAsync(dataCacheTag);

                _stopWatch.Stop();
                getAndRemoveList.Add(_stopWatch.ElapsedMilliseconds);
                memUsageList.Add(GC.GetTotalMemory(false) - bytes1);
            }

            RecordStatistics(addList, getAndRemoveList, memUsageList);
        }
    }
}
