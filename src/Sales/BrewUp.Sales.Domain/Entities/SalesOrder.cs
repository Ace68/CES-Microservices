using BrewUp.Sales.Domain.Helpers;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Events;
using BrewUp.Shared.ExternalContracts;
using Muflone.Core;

namespace BrewUp.Sales.Domain.Entities;

public class SalesOrder : AggregateRoot
{
	internal SalesOrderNumber _salesOrderNumber;
	internal SalesOrderDate _orderDate;

	internal CustomerId _customerId;
	internal CustomerName _customerName;

	internal IEnumerable<SalesOrderRow> _rows;
	
	internal SalesOrderDeliveryDate _deliveryDate;
	// internal OrderState _orderState;

	protected SalesOrder()
	{
	}

	internal static SalesOrder CreateSalesOrder(SalesOrderId salesOrderId, SalesOrderNumber salesOrderNumber,
		SalesOrderDate salesOrderDate, CustomerId customerId, CustomerName customerName,
		SalesOrderDeliveryDate deliveryDate, IEnumerable<SalesOrderRowJson> rows, Guid correlationId)
	{
		// Check SalesOrder invariants

		return new SalesOrder(salesOrderId, salesOrderNumber, salesOrderDate, customerId, customerName, deliveryDate,
			rows, correlationId);
	}

	private SalesOrder(SalesOrderId salesOrderId, SalesOrderNumber salesOrderNumber, SalesOrderDate salesOrderDate,
		CustomerId customerId, CustomerName customerName, SalesOrderDeliveryDate deliveryDate,
		IEnumerable<SalesOrderRowJson> rows, Guid correlationId)
	{
		RaiseEvent(new SalesOrderCreated(salesOrderId, salesOrderNumber, salesOrderDate, customerId, customerName,
			deliveryDate, rows, correlationId));
	}

	private void Apply(SalesOrderCreated @event)
	{
		Id = @event.AggregateId;
		_salesOrderNumber = @event.SalesOrderNumber;
		_orderDate = @event.SalesOrderDate;
		_customerId = @event.CustomerId;
		_customerName = @event.CustomerName;
		_rows = @event.Rows.MapToDomainRows();
	
		_deliveryDate = @event.SalesOrderDeliveryDate;
	}
	
	// private void Apply(SalesOrderDomainException @event)
	// {
	// 	// do nothing
	// }
}