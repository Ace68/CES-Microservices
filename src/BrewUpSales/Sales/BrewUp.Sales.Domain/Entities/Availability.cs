using BrewUp.Sales.SharedKernel.CustomTypes;
using Muflone.Core;

namespace BrewUp.Sales.Domain.Entities;

public class Availability : AggregateRoot
{
	internal ProductId _productId;
	internal ProductName _productName;
	internal Quantity _quantity;

	protected Availability()
	{
	}

	internal static Availability CreateAvailability(ProductId productId, ProductName productName, Quantity quantity, Guid correlationId)
	{
		return new Availability(productId, productName, quantity, correlationId);
	}

	private Availability(ProductId productId, ProductName productName, Quantity quantity, Guid correlationId)
	{
		// RaiseEvent(new AvailabilityUpdatedDueToWarehousesNotification(beerId, correlationId, beerName, quantity));
	}

	// private void Apply(AvailabilityUpdatedDueToWarehousesNotification @event)
	// {
	// 	Id = @event.BeerId;
	//
	// 	_productId = @event.BeerId;
	// 	_productName = @event.BeerName;
	// 	_quantity = @event.Quantity;
	// }
}