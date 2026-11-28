using System.ComponentModel.DataAnnotations;
using CSharpEssentials.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Examples.AspNetCore.Controllers;

[ApiController]
[Route("mvc/orders")]
public sealed class MvcOrdersController(OrderStore store) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ConditionalGet]
    public IActionResult Get(Guid id) =>
        store.Find(id).Match<IActionResult>(order => Ok(order), error => this.Problem(error.ToError()));

    [HttpPost]
    public IActionResult Create(MvcCreateOrder request) =>
        Created($"/mvc/orders/{Guid.Empty}", request);
}

public sealed record MvcCreateOrder([Required] string? Customer, [Range(1, 100)] int? Quantity);
