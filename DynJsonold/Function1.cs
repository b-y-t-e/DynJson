using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using DynJson.Helpers;
using DynJson.Helpers.DatabaseHelpers;

namespace DynJson
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{subscription}/{app}/query/{query}")] HttpRequest req,
            string subscription,
            string app,
            string query,
            ILogger log)
        {
            //using(var con 

            /*CloudStorageAccount storageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                    SettingsHelper.Get("storageAccountName"), SettingsHelper.Get("storageAccountKey")), true);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            string name = req.Query["table"];

            CloudTable peopleTable = tableClient.GetTableReference("peopleTable");
            await peopleTable.CreateIfNotExistsAsync();

    */
            using (MyConnection con = MyConnectionCreator.Create())
            {
                var items = con.I.SelectManyAsDict(query);
                return new JsonResult(items);
            }

           /* log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");*/
        }
    }
}
