namespace BrewUp.Sales.SharedKernel.Enums;

public class OrderStateEnum : Enumeration
{
    public static OrderStateEnum Open = new(1, "Open", "Open");
    public static OrderStateEnum Close = new(2, "Close", "Close");
    public static OrderStateEnum Sent = new(3, "Sent", "Sent");
    
    public static IEnumerable<OrderStateEnum> List() =>
    [
        Open, Close, Sent
    ];
    
    public OrderStateEnum(int id, string code, string name) : base(id, code, name)
    {
    }
    
    public static OrderStateEnum FromName(string name)
    {
        var valueType = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (valueType == null)
            throw new Exception($"Possible values for OrderStateEnum: {string.Join(",", List().Select(s => s.Name))}");

        return valueType;
    }
    
    public static OrderStateEnum FromCode(string code)
    {
        var valueType = List().SingleOrDefault(s => string.Equals(s.Code, code, StringComparison.CurrentCultureIgnoreCase));

        if (valueType == null)
            throw new Exception($"Possible values for OrderStateEnum: {string.Join(",", List().Select(s => s.Code))}");

        return valueType;
    }
    
    public static OrderStateEnum From(int id)
    {
        var valueType = List().SingleOrDefault(s => s.Id == id);

        if (valueType == null)
            throw new Exception($"Possible values for OrderStateEnum: {string.Join(",", List().Select(s => s.Name))}");

        return valueType;
    }
}