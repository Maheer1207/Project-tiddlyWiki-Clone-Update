using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace TWUpdate
{
    public static class CopyTWUpdates
    {
        [FunctionName("CopyTWUpdates")]

        //public static async Task<HttpResponseMessage> Run(
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var subscriptionId = System.Environment.GetEnvironmentVariable("SUBSCRIPTION_ID", EnvironmentVariableTarget.Process);
            var storageConnectionString = System.Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            var sourceShareName = System.Environment.GetEnvironmentVariable("SOURCE_SHARE_NAME", EnvironmentVariableTarget.Process);
            var destinationShareName = System.Environment.GetEnvironmentVariable("DESTINATION_SHARE_NAME", EnvironmentVariableTarget.Process);
            var resourceGroup = System.Environment.GetEnvironmentVariable("RESOURCE_GROUP", EnvironmentVariableTarget.Process);
            var appName = System.Environment.GetEnvironmentVariable("APP_NAME_RESTART", EnvironmentVariableTarget.Process);
            var clientSecret = System.Environment.GetEnvironmentVariable("CLIENT_SECRET_RESTART", EnvironmentVariableTarget.Process);
            var clientId = System.Environment.GetEnvironmentVariable("CLIENT_ID_RESTART", EnvironmentVariableTarget.Process);
            var tenantId = System.Environment.GetEnvironmentVariable("TENANT_ID", EnvironmentVariableTarget.Process);

            // Get a reference to a file shares of the "Source" and "Destination" 
            var sourceShareClient = new ShareClient(storageConnectionString, sourceShareName);
            var destinationShareClient = new ShareClient(storageConnectionString, destinationShareName);

            // Get a reference to "root" directories
            var sourceRoot = sourceShareClient.GetRootDirectoryClient();
            var destinationRoot = destinationShareClient.GetRootDirectoryClient();

            // Get a reference to "tiddlywiki.info" files and "tiddlers" directories
            var sourceTWInfo = sourceRoot.GetFileClient("tiddlywiki.info");
            var destinationTWInfo = destinationRoot.GetFileClient("tiddlywiki.info");
            var sourceTiddlersDir = sourceRoot.GetSubdirectoryClient("tiddlers");
            var destinationTiddlersDir = destinationRoot.GetSubdirectoryClient("tiddlers");

            // Checks, updates or add if any file have been updated in the source
            await foreach (var file in sourceTiddlersDir.GetFilesAndDirectoriesAsync())
            {
                if (!file.IsDirectory)
                {
                    var name = file.Name;
                    if ((name.Substring(0, 1) != "$") && (name != "CloneAllUpdates.tid") && (name != "httpTrigger.js") && (name != "httpTrigger.js.meta")) 
                    {
                        var sourceTid = sourceTiddlersDir.GetFileClient(name);
                        var destinationTid = destinationTiddlersDir.GetFileClient(name);

                        // If the same file exists, and it has been updated. Then the file is updated for the destination as well
                        if (await destinationTid.ExistsAsync())
                        {
                            if ((sourceTid.GetProperties().Value.LastModified > destinationTid.GetProperties().Value.LastModified))
                            {
                                Console.WriteLine(name + " this is being updated in the destination");
                                await destinationTid.DeleteAsync();
                                await destinationTid.StartCopyAsync(sourceTid.Uri);
                            }
                        }
                        // If any .tid file doesn't exist, then its created
                        else
                        {
                            Console.WriteLine(name + " this is being newly created in the destination");
                            await destinationTid.StartCopyAsync(sourceTid.Uri);
                        }
                    }
                }
            }

            // Checks if any file have been deleted from the source that still exist in the destination, will be deleted
            // as in terms of .tid files, both the source and destination should be identical 
            await foreach (var file in destinationTiddlersDir.GetFilesAndDirectoriesAsync())
            {
                if (!file.IsDirectory)
                {
                    var name = file.Name;

                    // Ignoring the CONFIG file, as all config files start with "$"
                    if (name.Substring(0, 1) != "$")
                    {
                        var sourceTid = sourceTiddlersDir.GetFileClient(name);
                        var destinationTid = destinationTiddlersDir.GetFileClient(name);

                        // If the same file exists, and it has been updated. Then the file is updated for the destination as well
                        if (!(await sourceTid.ExistsAsync()))
                        {
                            Console.WriteLine(name + " is deleted from the destination");
                            await destinationTid.DeleteAsync();
                        }
                    }
                }
            }

            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic).Authenticate(credentials).WithSubscription(subscriptionId);

            var webApp = await azure.WebApps.GetByResourceGroupAsync(resourceGroup, appName);
            await webApp.RestartAsync();

            return new OkObjectResult(new { responseMessage = "Destination Updated!! and Restart done!!" });
        }
    }
}
