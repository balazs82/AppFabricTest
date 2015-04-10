using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppFabricTest;
using Microsoft.ApplicationServer.Caching;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CacheTest
{
    [TestClass]
    public class CacheTests
    {
        private DataCacheServerEndpoint Endpoint
        {
            get { return new DataCacheServerEndpoint("localhost", 22233); }
        }

        [TestMethod]
        public void CachePocCompareTest()
        {
            int datasetCount = 100;
            int dimensionCount = 10;

            Dictionary<string, Dictionary<string, object>> performanceTracker = new Dictionary<string, Dictionary<string, object>>();
            CacheBenchmark benchmark1 = new CacheBenchmark(new CachePocNamedRegionForAllDatasetsUsingTags(), Endpoint, datasetCount, dimensionCount, performanceTracker);
            benchmark1.DoWork();

            CacheBenchmark benchmark2 = new CacheBenchmark(new CachePocNamedRegionUsingTags(), Endpoint, datasetCount, dimensionCount, performanceTracker);
            benchmark2.DoWork();

            CacheBenchmark benchmark3 = new CacheBenchmark(new CachePocKeysDatasetCodesStoredInDb(), Endpoint, datasetCount, dimensionCount, performanceTracker);
            benchmark3.DoWork();

            CacheBenchmark benchmark4 = new CacheBenchmark(new CachePocChangeTracking(), Endpoint, datasetCount, dimensionCount, performanceTracker);
            benchmark4.DoWork();

            ComparedResultsToOutput(performanceTracker);
        }

        [TestMethod]
        public void CachePocTest_NamedRegionForAllDatasetsUsingTags()
        {
            Dictionary<string, Dictionary<string, object>> performanceTracker = new Dictionary<string, Dictionary<string, object>>();
            CacheBenchmark benchmark = new CacheBenchmark(new CachePocNamedRegionForAllDatasetsUsingTags(), Endpoint, 1000, 10, performanceTracker);
            benchmark.DoWork();

            ResultsToOutput(performanceTracker);
        }

        [TestMethod]
        public void CachePocTest_NamedRegionUsingTags()
        {
            Dictionary<string, Dictionary<string, object>> performanceTracker = new Dictionary<string, Dictionary<string, object>>();
            CacheBenchmark benchmark = new CacheBenchmark(new CachePocNamedRegionUsingTags(), Endpoint, 10, 10, performanceTracker);
            benchmark.DoWork();

            ResultsToOutput(performanceTracker);
        }

        [TestMethod]
        public void CachePocTest_KeysDatasetCodesStoredInDb()
        {
            Dictionary<string, Dictionary<string, object>> performanceTracker = new Dictionary<string, Dictionary<string, object>>();
            CacheBenchmark benchmark = new CacheBenchmark(new CachePocKeysDatasetCodesStoredInDb(), Endpoint, 1000, 10, performanceTracker);
            benchmark.DoWork();

            ResultsToOutput(performanceTracker);
        }

        [TestMethod]
        public void CachePocTest_ChangeTracking()
        {
            Dictionary<string, Dictionary<string, object>> performanceTracker = new Dictionary<string, Dictionary<string, object>>();
            CacheBenchmark benchmark = new CacheBenchmark(new CachePocChangeTracking(), Endpoint, 10, 10, performanceTracker);
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
