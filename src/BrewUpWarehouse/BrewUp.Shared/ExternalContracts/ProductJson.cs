namespace BrewUp.Shared.ExternalContracts;

public record ProductJson(string ProductId, 
    string ProductName, 
    string ProductDescription, 
    string ProductType, 
    IEnumerable<AvailabilityJson> Availabilities);