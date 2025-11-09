using System.Text;
using BrewUp.Sales.SharedKernel.CustomTypes;
using Muflone.Core;
using Muflone.Messages.Events;
using Muflone.Persistence.Azure.Helpers;
using Muflone.Persistence.Azure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Muflone.Persistence.Azure.Tests;

public class SerializationTests
{
    private const string EventClrTypeHeader = "EventClrTypeName";
    private const string AggregateClrTypeHeader = "AggregateClrTypeName";
    private const string CommitIdHeader = "CommitId";
    private const string CommitDateHeader = "CommitDate";

    [Fact]
    public void Can_Deserialize_CloudEvent()
    {
        var metadata =
            "7B22436F6D6D69744964223A2261623834363436632D663538342D346230312D383965662D663034616135386561386237222C22436F6D6D697444617465223A22323032352D31312D30375431303A33333A32352E383731383939355A222C22416767726567617465436C72547970654E616D65223A224272657755702E53616C65732E446F6D61696E2E456E7469746965732E53616C65734F726465722C204272657755702E53616C65732E446F6D61696E2C2056657273696F6E3D312E302E302E302C2043756C747572653D6E65757472616C2C205075626C69634B6579546F6B656E3D6E756C6C222C224576656E74436C72547970654E616D65223A224272657755702E53616C65732E5368617265644B65726E656C2E4D657373616765732E4576656E74732E53616C65734F72646572437265617465642C204272657755702E53616C65732E5368617265644B65726E656C2C2056657273696F6E3D312E302E302E302C2043756C747572653D6E65757472616C2C205075626C69634B6579546F6B656E3D6E756C6C227D";
        var data =
            "7B2253616C65734F726465724E756D626572223A7B2256616C7565223A2232303235313130372D31313332227D2C2253616C65734F7264657244617465223A7B2256616C7565223A22323032352D31312D30375430303A30303A3030227D2C22437573746F6D65724964223A7B2256616C7565223A2262326234393836382D663936322D343633352D396464322D613637313930616362366362227D2C22437573746F6D65724E616D65223A7B2256616C7565223A22496C2047726F7474696E6F2064656C204D75666C6F6E65227D2C2253616C65734F7264657244656C697665727944617465223A7B2256616C7565223A22323032352D31312D31305430303A30303A3030227D2C22526F7773223A5B7B2250726F647563744964223A2238636263653565362D666530362D346131662D613762322D313437656262666664633238222C2250726F647563744E616D65223A224D75666C6F6E6520495041222C225175616E74697479223A7B225175616E74697479223A352E302C22556E69744F664D656173757265223A22426F74746C6573227D2C225072696365223A7B225072696365223A362E302C2243757272656E6379223A22455552227D7D5D2C224167677265676174654964223A7B2256616C7565223A2233666634323264642D326232362D346135302D613662352D323463393239666437313366227D2C2248656164657273223A7B225374616E6461726473223A7B22436F7272656C6174696F6E4964223A2232313030323238622D313565642D346531362D616435642D353431653136636638666132222C2241676772656761746554797065223A2253616C65734F7264657243726561746564222C224163636F756E744964223A2230303030303030302D303030302D303030302D303030302D303030303030303030303030222C2257686F223A22416E6F6E796D6F7573222C225768656E223A22363338393831303834303538353939393636227D2C22437573746F6D73223A7B7D2C22436F7272656C6174696F6E4964223A2232313030323238622D313565642D346531362D616435642D353431653136636638666132222C2257686F223A7B224964223A2230303030303030302D303030302D303030302D303030302D303030303030303030303030222C224E616D65223A22416E6F6E796D6F7573227D2C225768656E223A7B2256616C7565223A22323032352D31312D30375431303A33333A32352E38353939393636227D2C2241676772656761746554797065223A2253616C65734F7264657243726561746564227D2C2256657273696F6E223A302C224D6573736167654964223A2266373139303030302D633230302D383861342D303162662D303864653164653931353832222C225573657250726F70657274696573223A7B22436F7272656C6174696F6E4964223A2232313030323238622D313565642D346531362D616435642D353431653136636638666132227D7D";

        var eventDataResult = false;
        try
        {
            var domainEvent = RepositoryHelper.DeserializeCloudEvent(new ResolvedCloudEvent(metadata, data));
            
            eventDataResult = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        Assert.True(eventDataResult);
    }
    
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