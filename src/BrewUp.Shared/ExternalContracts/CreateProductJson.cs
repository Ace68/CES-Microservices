namespace BrewUp.Shared.ExternalContracts;

public class CreateProductJson
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    
    public IEnumerable<AvailabilityJson> Availabilities { get; set; } = [];
}