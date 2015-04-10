using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFabricTest
{
    public class CachePocNamedRegionForAllDatasetsUsingTags : CachePocBase
    {
        public override void DoWork()
        {
            List<long> addList = new List<long>();
            List<long> getAndRemoveList = new List<long>();
            List<long> memUsageList = new List<long>();

            List<string> datasetRegions = new List<string>();
                       

            for (var i = 0; i < _datasetCount; i++)
            {
                string regionName = "Region" + i;
                datasetRegions.Add(regionName);
                try
                {
                    _myDefaultCache.ClearRegion(regionName);
                }
                catch { }
                _myDefaultCache.CreateRegion(regionName);

                for (var j = 0; j < _dimensionCount; j++)
                {
                    string key = datasetRegions[i] + "DimensionName" + j;

                    _stopWatch = Stopwatch.StartNew();

                    var addResult = _myDefaultCache.Add(key, _data, regionName);

                    _stopWatch.Stop();
                    addList.Add(_stopWatch.ElapsedMilliseconds);

                    if (addResult == null)
                    {
                        Console.WriteLine("**FAIL----->Add-Object did not add to cache - FAIL");
                    }
                }
            }

            foreach (string region in datasetRegions)
            {
                long bytes1 = GC.GetTotalMemory(false);
                _stopWatch = Stopwatch.StartNew();

                _myDefaultCache.ClearRegion(region);

                _stopWatch.Stop();
                getAndRemoveList.Add(_stopWatch.ElapsedMilliseconds);
                memUsageList.Add(GC.GetTotalMemory(false) - bytes1);
            }

            RecordStatistics(addList, getAndRemoveList, memUsageList);
        }
    }
}
