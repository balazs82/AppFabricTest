using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppFabricTest;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TagCache.Redis;

namespace CacheTest
{
    [TestClass]
    public class CacheTests
    {
        private RedisConnectionManager Endpoint
        {
            get { return new RedisConnectionManager("127.0.0.1:6379"); }
        }

        [TestMethod]
        public void CachePocCompareTest()
        {
            int datasetCount = 100;
            int dimensionCount = 10;

            Dictionary<string, Dictionary<string, object>> performanceTracker = new Dictionary<string, Dictionary<string, object>>();

            CacheBenchmark benchmark1 = new CacheBenchmark(new CachePocUsingTags(), Endpoint, datasetCount, dimensionCount, performanceTracker);
            benchmark1.DoWork();

            ComparedResultsToOutput(performanceTracker);
        }

        [TestMethod]
        public void CachePocTest_NamedRegionUsingTags()
        {
            Dictionary<string, Dictionary<string, object>> performanceTracker = new Dictionary<string, Dictionary<string, object>>();
            CacheBenchmark benchmark = new CacheBenchmark(new CachePocUsingTags(), Endpoint, 10, 10, performanceTracker);
            benchmark.DoWork();

            ResultsToOutput(performanceTracker);
        }

        private void ResultsToOutput(Dictionary<string, Dictionary<string, object>> performanceTracker)
        {
            foreach (var pocResult in performanceTracker)
            {
                Debug.WriteLine("");
                Debug.WriteLine("--- POC results - " + pocResult.Key);
                foreach (var resultItem in pocResult.Value)
                {
                    Debug.WriteLine(" " + resultItem.Key + "\t\t" + resultItem.Value);
                }
                Debug.WriteLine("---------------------");
            }
        }

        private void ComparedResultsToOutput(Dictionary<string, Dictionary<string, object>> performanceTracker)
        {
            string delimiter = "\t";
            var pocResult = performanceTracker.First();
            {
                Debug.WriteLine("");
                Debug.WriteLine("--- POC results - " + performanceTracker.Select(p => p.Key).Aggregate((i, j) => i + delimiter + j));
                foreach (var resultItem in pocResult.Value)
                {
                    string values = string.Empty;
                    for (int i = 0; i < performanceTracker.Count(); i++)
                    {
                        values += delimiter + performanceTracker.ElementAt(i).Value.Where(v => v.Key == resultItem.Key).Select(s => s.Value == null ? string.Empty : s.Value.ToString()).FirstOrDefault();
                    }
                    Debug.WriteLine(" " + resultItem.Key + " " + values);
                }
                Debug.WriteLine("---------------------");
            }
        }
    }
}
