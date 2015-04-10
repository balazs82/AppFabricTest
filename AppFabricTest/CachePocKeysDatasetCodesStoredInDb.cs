using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFabricTest
{
    public class CachePocKeysDatasetCodesStoredInDb : CachePocBase
    {
        public override void DoWork()
        {
            List<long> addList = new List<long>();
            List<long> getAndRemoveList = new List<long>();
            List<long> memUsageList = new List<long>();

            List<string> datasets = new List<string>();

            try
            { 
                _myDefaultCache.ClearRegion(_regionName);
            }
            catch { }
            _myDefaultCache.CreateRegion(_regionName);

            Task.Factory.StartNew(() => InsertValue(datasets, addList)).Wait();

            foreach (var dataset in datasets)
            {
                long bytes1 = GC.GetTotalMemory(false);
                _stopWatch = Stopwatch.StartNew();

                foreach (var key in SelectAndDeleteFromDb(dataset))
                {
                    _myDefaultCache.Remove(key);
                }

                _stopWatch.Stop();
                memUsageList.Add(GC.GetTotalMemory(false) - bytes1);
                getAndRemoveList.Add(_stopWatch.ElapsedMilliseconds);
            }

            RecordStatistics(addList, getAndRemoveList, memUsageList, _regionName);
        }

        private void InsertValue(List<string> datasets, List<long> addList)
        {
            for (var i = 0; i < _datasetCount; i++)
            {
                string datasetCode = "Dataset" + i;
                datasets.Add(datasetCode);

                for (var j = 0; j < _dimensionCount; j++)
                {
                    string key = datasets[i] + "DimensionName" + j;

                    _stopWatch = Stopwatch.StartNew();

                    var addResult = _myDefaultCache.Add(key, _data);
                    Task.Run(() => InsertIntoDb(datasetCode, key));

                    _stopWatch.Stop();
                    addList.Add(_stopWatch.ElapsedMilliseconds);

                    if (addResult == null)
                    {
                        Console.WriteLine("**FAIL----->Add-Object did not add to cache - FAIL");
                    }
                }
            }
        }

        private async Task InsertIntoDb(string datasetCode, string cacheKey)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Dev"].ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("INSERT INTO Test (DatasetCode, CacheKey) VALUES (@datasetCode, @cacheKey)", connection))
                {
                    command.Parameters.Add("@datasetCode", SqlDbType.VarChar, 50).Value = datasetCode;
                    command.Parameters.Add("@cacheKey", SqlDbType.VarChar, 50).Value = cacheKey;
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private IEnumerable<string> SelectAndDeleteFromDb(string datasetCode)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Dev"].ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("DELETE FROM Test OUTPUT DELETED.CacheKey WHERE DatasetCode = @datasetCode", connection))
                {
                    command.Parameters.Add("@datasetCode", SqlDbType.VarChar, 50).Value = datasetCode;
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        yield return reader["CacheKey"].ToString();
                    }
                }
            }
        }
    }
}
