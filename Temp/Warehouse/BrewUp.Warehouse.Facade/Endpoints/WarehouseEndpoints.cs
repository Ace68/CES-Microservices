using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;

namespace BrewUp.Warehouse.Facade.Endpoints;

public static class WarehouseEndpoints
{
    public static WebApplication MapWarehouseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/warehouse")
            .WithTags("Warehouse")
            .WithOpenApi();
        
        group.MapPost("/", HandlePostCreateProduct)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Create a new product")
            .WithDescription(
                "Creates a new product. This endpoint is used to add a new product to warehouse.")
            .WithName("CreateProduct");

        return app;
    }
    
    private static async Task<IResult> HandlePostCreateProduct(
        IWarehouseFacade warehouseFacade,
        IValidator<CreateProductJson> validator,
        ValidationHandler validationHandler,
        CreateProductJson body,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await validationHandler.ValidateAsync(validator, body);
        if (!validationHandler.IsValid)
            return Results.BadRequest(validationHandler.Errors);

        try
        {
            string productId = await warehouseFacade.CreateProductAsync(body, cancellationToken);
            return Results.Created($"/v1/warehouse/{productId}", productId);
        }
        catch
        {
            return Results.BadRequest();
        }
    }
}