using System.Text;
using BrewUp.Sales.SharedKernel.CustomTypes;
using Muflone.Core;
using Muflone.Messages.Events;
using Muflone.Persistence.Azure.Helpers;
using Muflone.Persistence.Azure.Models;
using Newtonsoft.Json;

namespace Muflone.Persistence.Azure.Tests;

public class SerializationTests
{
    private const string EventClrTypeHeader = "EventClrTypeName";
    private const string AggregateClrTypeHeader = "AggregateClrTypeName";
    private const string CommitIdHeader = "CommitId";
    private const string CommitDateHeader = "CommitDate";
    
    [Fact]
    public void Can_Serialize_An_Event()
    {
        TestOrderCreated @event = new(
            new SalesOrderId(Guid.NewGuid().ToString()),
            new SalesOrderNumber("SO-001"),
            new SalesOrderDate(DateTime.UtcNow),
            new CustomerId("CUST-001"),
            new CustomerName("John Doe"),
            new SalesOrderDeliveryDate(DateTime.UtcNow.AddDays(7)),
            Guid.NewGuid());

        var serializedEvent = JsonConvert.SerializeObject(@event);
        var deserializedEvent = JsonConvert.DeserializeObject<TestOrderCreated>(serializedEvent);

        Assert.Equal(@event.AggregateId, deserializedEvent!.AggregateId);
    }
    
    [Fact]
    public void Can_Serialize_And_Deserialize_An_Event()
    {
        TestOrder aggregate = TestOrder.CreateSalesOrder(new SalesOrderId(Guid.NewGuid().ToString()),
            new SalesOrderNumber("SO-001"),
            new SalesOrderDate(DateTime.UtcNow),
            new CustomerId("CUST-001"),
            new CustomerName("John Doe"),
            new SalesOrderDeliveryDate(DateTime.UtcNow.AddDays(7)),
            Guid.NewGuid());
        
        var commitHeaders = new Dictionary<string, object>
        {
            { CommitIdHeader, Guid.NewGuid() },
            { CommitDateHeader, DateTime.UtcNow},
            { AggregateClrTypeHeader, aggregate.GetType().AssemblyQualifiedName! }
        };
        
        var newEvents = ((IAggregate) aggregate).GetUncommittedEvents().Cast<object>().ToList();
        var eventsToSave = newEvents.Select(e => RepositoryHelper.ToEventData(Guid.NewGuid(), e, commitHeaders)).ToList();
        
        foreach (var eventData in eventsToSave)
        {
            var deserializedEvent = RepositoryHelper.DeserializeEvent(
                new ResolvedEvent(aggregate.Id.Value, Encoding.UTF8.GetBytes(eventData.Metadata), Encoding.UTF8.GetBytes(eventData.Data)));
            Assert.IsType<TestOrderCreated>(deserializedEvent);
        }
        
    }
}

public class TestOrder : AggregateRoot
{
    internal SalesOrderNumber _salesOrderNumber;
    internal SalesOrderDate _orderDate;

    internal CustomerId _customerId;
    internal CustomerName _customerName;

    internal SalesOrderDeliveryDate _deliveryDate;
    
    internal static TestOrder CreateSalesOrder(SalesOrderId salesOrderId, SalesOrderNumber salesOrderNumber,
        SalesOrderDate salesOrderDate, CustomerId customerId, CustomerName customerName,
        SalesOrderDeliveryDate deliveryDate, Guid correlationId)
    {
        // Check SalesOrder invariants

        return new TestOrder(salesOrderId, salesOrderNumber, salesOrderDate, customerId, customerName, deliveryDate,
            correlationId);
    }

    private TestOrder(SalesOrderId aggregateId, SalesOrderNumber salesOrderNumber, SalesOrderDate salesOrderDate,
        CustomerId customerId, CustomerName customerName, SalesOrderDeliveryDate deliveryDate,
        Guid correlationId)
    {
        RaiseEvent(new TestOrderCreated(aggregateId, salesOrderNumber, salesOrderDate, customerId, customerName,
            deliveryDate, correlationId));
    }

    private void Apply(TestOrderCreated @event)
    {
        Id = @event.AggregateId;
        _salesOrderNumber = @event.SalesOrderNumber;
        _orderDate = @event.SalesOrderDate;
        _customerId = @event.CustomerId;
        _customerName = @event.CustomerName;
        _deliveryDate = @event.SalesOrderDeliveryDate;
    }
}

public sealed class TestOrderCreated(
    SalesOrderId aggregateId,
    SalesOrderNumber salesOrderNumber,
    SalesOrderDate salesOrderDate,
    CustomerId customerId,
    CustomerName customerName,
    SalesOrderDeliveryDate salesOrderDeliveryDate,
    Guid correlationId) : DomainEvent(aggregateId, correlationId)
{
    public SalesOrderNumber SalesOrderNumber { get; private set; } = salesOrderNumber;
    public SalesOrderDate SalesOrderDate { get; private set; } = salesOrderDate;
    
    public CustomerId CustomerId { get; private set; } = customerId;
    public CustomerName CustomerName { get; private set; } = customerName;
    
    public SalesOrderDeliveryDate SalesOrderDeliveryDate { get; private set; } = salesOrderDeliveryDate;
}