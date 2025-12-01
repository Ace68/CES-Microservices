using BrewUp.Shared.Domain;

namespace BrewUp.Sales.Entities.Dtos;

public class SalesForProduct : BrewUpAggregateRoot
{
    public string ProductName { get; private set; } = string.Empty;
    public double Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;
    
    public double Price { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    
    protected SalesForProduct()
    {}
}