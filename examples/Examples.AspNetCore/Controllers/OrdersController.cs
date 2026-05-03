using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Examples.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace Examples.AspNetCore.Controllers;

/// <summary>
/// Orders API demonstrating Result chaining across service boundaries.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// POST /api/v1/orders
    /// Place an order. Demonstrates how business-rule failures (insufficient stock,
    /// price limit exceeded) flow through the Result chain and become ProblemDetails.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        // -------------------------------------------------------------------
        // The service already chains Results with Then.
        // We just convert the final Result to an HTTP response.
        // -------------------------------------------------------------------
        return _orderService.PlaceOrder(request.ProductId, request.Quantity).Match(
            onSuccess: order => CreatedAtAction(
                nameof(GetOrder),
                new { id = order.Id, version = "1.0" },
                order
            ),
            onError: errors => errors.ToActionResult()
        );
    }

    /// <summary>
    /// GET /api/v1/orders/{id}
    /// Retrieve a single order.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetOrder(Guid id)
    {
        // Simulated lookup for demo purposes
        return Error.NotFound($"Order with id '{id}' was not found.").ToActionResult();
    }
}

public sealed record PlaceOrderRequest(Guid ProductId, int Quantity);
