using BrewUp.Sales.Domain.Entities;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.ExternalContracts;

namespace BrewUp.Sales.Domain.Helpers;

public static class DomainHelper
{
	internal static SalesOrderRow MapToDomainRow(this SalesOrderRowJson json)
	{
		return SalesOrderRow.CreateSalesOrderRow(new ProductId(json.ProductId), new ProductName(json.ProductName), 
			new Quantity(json.Quantity.Quantity, json.Quantity.UnitOfMeasure),
			new Price(json.Price.Price, json.Price.Currency));
	}

	internal static IEnumerable<SalesOrderRow> MapToDomainRows(this IEnumerable<SalesOrderRowJson> json)
	{
		return json.Select(r =>
			SalesOrderRow.CreateSalesOrderRow(new ProductId(r.ProductId), new ProductName(r.ProductName), 
				new Quantity(r.Quantity.Quantity, r.Quantity.UnitOfMeasure), 
				new Price(r.Price.Price, r.Price.Currency)));
	}

	// internal static BrewUp.Sales.Entities.Dtos.SalesOrder MapToReadModel(this SalesOrder salesOrder)
	// {
	// 	return BrewUp.Sales.Entities.Dtos.SalesOrder.Create((SalesOrderId)salesOrder.Id, salesOrder._salesOrderNumber,
	// 					salesOrder._orderDate, salesOrder._customerId, salesOrder._customerName,
	// 					salesOrder._deliveryDate, salesOrder._rows.MapToReadModelRows(), Guid.NewGuid());
	// }
}