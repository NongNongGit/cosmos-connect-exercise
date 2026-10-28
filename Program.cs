using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

string endpoint = "https://dp420lab03yl07sa.documents.azure.com:443/"; //"https://localhost:8081/";
//string key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

//  // ==== CLIENT ==== Task 6 replaces this section.
//  // Keep the emulator-only transport settings while adding the logging handler.
//  CosmosClient client = new CosmosClientBuilder(endpoint, key)
//      .WithConnectionModeGateway()
//      .WithHttpClientFactory(() =>
//      {
//          var handler = new System.Net.Http.HttpClientHandler
//          {
//              ServerCertificateCustomValidationCallback =
//                  System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//          };
//          return new System.Net.Http.HttpClient(handler);
//      })
//      .AddCustomHandlers(new LogHandler())
//      .Build();
//  // ==== END CLIENT ====
  // Read account metadata from the service, not an item in a container.
  
   // ==== CLIENT ====
 // Explicitly use az login and normal certificate validation for the cloud account.
 // Retain the logging handler while removing the emulator's transport overrides.
 CosmosClient client = new CosmosClientBuilder(endpoint, new AzureCliCredential())
     .AddCustomHandlers(new LogHandler())
     .Build();
 // ==== END CLIENT ====
 
 AccountProperties account = await client.ReadAccountAsync();
 Console.WriteLine($"Account name:     {account.Id}");
 Console.WriteLine($"Primary region:   {account.WritableRegions.FirstOrDefault()?.Name}");
 Console.WriteLine($"Consistency:      {account.Consistency.DefaultConsistencyLevel}");

  // These calls build references; ReadContainerAsync makes the service request.
 Database database = client.GetDatabase("cosmicworks");
 Container container = client.GetContainer("cosmicworks", "product");

 ContainerProperties properties = await container.ReadContainerAsync();

 Console.WriteLine($"Database:         {database.Id}");
 Console.WriteLine($"Container:        {properties.Id}");
  try
 {
     // A nonexistent ID and partition key deliberately exercise error handling.
     var response = await container.ReadItemAsync<dynamic>(
         id: "nonexistent-id",
         partitionKey: new PartitionKey("nonexistent-category"));
 }
 catch (CosmosException ex)
 {
     // Inspect the exception's status code.
     Console.WriteLine($"\nCaught CosmosException:");
     Console.WriteLine($"  Status code:    {(int)ex.StatusCode} ({ex.StatusCode})");
     Console.WriteLine($"  Is retryable:   " +
         $"{ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
            ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable}");
 }

  public class LogHandler : RequestHandler
 {
     public override async Task<ResponseMessage> SendAsync(
         RequestMessage request, CancellationToken cancellationToken)
     {
         Console.WriteLine($"  [REQ]  {request.Method.Method} {request.RequestUri}");
         // Forward the request through the SDK pipeline.
         ResponseMessage response = await base.SendAsync(request, cancellationToken);
         Console.WriteLine($"  [RES]  {(int)response.StatusCode} {response.StatusCode}" +
                           $"  RU: {response.Headers.RequestCharge:0.00}");
         return response;
     }
 }