using BrewUp.Sales.SharedKernel.CustomTypes;
using Muflone.Core;

namespace BrewUp.Sales.Domain.Entities;

public class SalesOrderRow : Entity
{
	internal ProductId _productId;
	internal ProductName _productName;

	internal Quantity _quantity;
	internal Price _beerPrice;

	protected SalesOrderRow()
	{
	}

	internal static SalesOrderRow CreateSalesOrderRow(ProductId productId, ProductName productName, Quantity quantity,
		Price price)
	{
		return new SalesOrderRow(productId, productName, quantity, price);
	}

	private SalesOrderRow(ProductId productId, ProductName productName, Quantity quantity, Price price)
	{
		_productId = productId;
		_productName = productName;
		_quantity = quantity;
		_beerPrice = price;
	}
}