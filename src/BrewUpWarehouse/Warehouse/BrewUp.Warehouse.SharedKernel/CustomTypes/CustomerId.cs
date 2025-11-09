using Muflone.Core;

namespace BrewUp.Warehouse.SharedKernel.CustomTypes;

public sealed class CustomerId(string value) : DomainId(value);