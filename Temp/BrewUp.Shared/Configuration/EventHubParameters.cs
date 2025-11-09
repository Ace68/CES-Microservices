namespace BrewUp.Shared.Configuration;

public record EventHubParameters(string EventHubConnectionString, string EventHubName, 
    string BlobStorageConnectionString, string BlobStorageContainerName);