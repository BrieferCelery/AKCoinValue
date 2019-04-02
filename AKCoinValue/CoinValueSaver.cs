using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace AKCoinValue
{
    public static class CoinValueSaver
    {
        private const string Symbol = "btc";
        private const string Url = "https://api.coinmarketcap.com/v1/ticker/";
        public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=akcoinvalue;AccountKey=kdogAVznwvQ4Qgk4Jul9CIf+T8IeCls9Nc8N1iJe/hk4+I0grstMoUqsHz7DW2hW4VIUfztHmv/Ir+HJsc0Y1A==;BlobEndpoint=https://akcoinvalue.blob.core.windows.net/;QueueEndpoint=https://akcoinvalue.queue.core.windows.net/;TableEndpoint=https://akcoinvalue.table.core.windows.net/;FileEndpoint=https://akcoinvalue.file.core.windows.net/;";
        public const string TableName = "coins";

        [FunctionName("CoinValueSaver")]
        public static async System.Threading.Tasks.Task Run([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            // Create account, client and table
            var account = CloudStorageAccount.Parse(ConnectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference(TableName);
            await table.CreateIfNotExistsAsync();

            // Get coin value (JSON)
            var client = new HttpClient();
            var json = await client.GetStringAsync(Url);

            var price = 0.0;

            try
            {
                var array = JArray.Parse(json);

                var priceString = array.Children<JObject>()
                    .FirstOrDefault(c => c.Property("symbol")
                                             .Value
                                             .ToString()
                                             .ToLower() == Symbol)?
                    .Property("price_usd").Value.ToString();

                if (priceString != null)
                {
                    double.TryParse(priceString, out price);
                }
            }
            catch
            {
                // Do nothing here for demo purposes
            }

            if (price < 0.1)
            {
                log.Info("Something went wrong");
                return; // Do some logging here
            }

            var coin = new CoinEntity
            {
                Symbol = Symbol,
                TimeOfReading = DateTime.Now,
                RowKey = "row" + DateTime.Now.Ticks,
                PartitionKey = "partition",
                PriceUsd = price
            };

            // Insert new value in table
            table.Execute(TableOperation.Insert(coin));
        }
    }
}
