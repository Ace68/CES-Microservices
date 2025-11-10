using BrewUp.Sales.Domain.Helpers;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Enums;
using BrewUp.Sales.SharedKernel.Messages.Events;
using BrewUp.Shared.Exceptions;
using BrewUp.Shared.ExternalContracts;
using Muflone.Core;

namespace BrewUp.Sales.Domain.Entities;

public class SalesOrder : AggregateRoot
{
	internal SalesOrderNumber SalesOrderNumber = null!;
	internal SalesOrderDate OrderDate = null!;

	internal CustomerId CustomerId = null!;
	internal CustomerName CustomerName = null!;

	internal IEnumerable<SalesOrderRow> Rows = [];
	
	internal SalesOrderDeliveryDate DeliveryDate = new (DateTime.MaxValue);
	internal OrderStateEnum OrderState = OrderStateEnum.Open;

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
		SalesOrderNumber = @event.SalesOrderNumber;
		OrderDate = @event.SalesOrderDate;
		CustomerId = @event.CustomerId;
		CustomerName = @event.CustomerName;
		Rows = @event.Rows.MapToDomainRows();
	
		DeliveryDate = @event.SalesOrderDeliveryDate;
		
		OrderState = OrderStateEnum.Open;
	}

	internal void CloseOrder(SalesOrderDeliveryDate deliveryDate, Guid correlationId)
	{
		if (Equals(OrderState, OrderStateEnum.Close))
		{
			RaiseEvent(new SalesOrderExceptionRaised(new SalesOrderId(Id.Value),
				new BrewUpAggregateException(Id.Value, GetType().FullName!, "Order Already Closed!"), correlationId));
			return;
		}
		
		RaiseEvent(new SalesOrderClosed(new SalesOrderId(Id.Value), deliveryDate, correlationId));
	}

	private void Apply(SalesOrderClosed @event)
	{
		DeliveryDate = @event.SalesOrderDeliveryDate;
		
		OrderState = OrderStateEnum.Close;
	}
	
	private void Apply(SalesOrderExceptionRaised @event)
	{
		// No state change
	}
}