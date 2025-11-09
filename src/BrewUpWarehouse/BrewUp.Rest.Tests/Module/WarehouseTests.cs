using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ReadModel;

namespace BrewUp.Rest.Tests.Module;

[ExcludeFromCodeCoverage]
[Collection( "Integration Fixture" )]
public class WarehouseTests (AppHttpClientFixture integrationFixture)
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
    
    [Fact]
    public async Task Can_Create_Product()
    {
        CreateProductJson body = new()
        {
            ProductName = "Muflone Beer",
            ProductDescription = "A special beer brewed in the heart of the mountains.",
            ProductType = "IPA"
        };

        var stringJson = JsonSerializer.Serialize(body);
        var httpContent = new StringContent(stringJson, Encoding.UTF8, "application/json");
        var postResult = await integrationFixture.Client.PostAsync("/v1/warehouse", httpContent);
        
        Assert.Equal(HttpStatusCode.Created, postResult.StatusCode);
    }
    
    [Fact]
    public async Task Can_Get_Products()
    {
        var getResult = await integrationFixture.Client.GetAsync("/v1/warehouse");
        var content = await getResult.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<ProductJson>>(content, _options);

        Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
        Assert.NotNull(pagedResult);
        Assert.NotNull(pagedResult.Results);
    }
}