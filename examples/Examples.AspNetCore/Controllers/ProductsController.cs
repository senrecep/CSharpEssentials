using CSharpEssentials.AspNetCore;
using CSharpEssentials.ResultPattern;
using Examples.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace Examples.AspNetCore.Controllers;

/// <summary>
/// Products API demonstrating Result pattern integration with ASP.NET Core.
/// No try/catch blocks needed — failures are expressed as Results and
/// automatically converted to RFC 7807 ProblemDetails responses.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// GET /api/v1/products
    /// Search products by name. Returns all products when name is omitted.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Product>), StatusCodes.Status200OK)]
    public IActionResult Search([FromQuery] string? name)
    {
        return _productService.Search(name).Match(
            onSuccess: products => Ok(products),
            onError: errors => errors.ToActionResult()
        );
    }

    /// <summary>
    /// GET /api/v1/products/{id}
    /// Retrieve a single product by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        return _productService.GetById(id).Match(
            onSuccess: product => Ok(product),
            onError: errors => errors.ToActionResult()
        );
    }

    /// <summary>
    /// POST /api/v1/products
    /// Create a new product. Returns validation errors as ProblemDetails
    /// when the input is invalid.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateProductRequest request)
    {
        return _productService.Create(request).Match(
            onSuccess: product => CreatedAtAction(
                nameof(GetById),
                new { id = product.Id, version = "1.0" },
                product
            ),
            onError: errors => errors.ToActionResult()
        );
    }

    /// <summary>
    /// PUT /api/v1/products/{id}
    /// Update an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        return _productService.Update(id, request).Match(
            onSuccess: product => Ok(product),
            onError: errors => errors.ToActionResult()
        );
    }

    /// <summary>
    /// DELETE /api/v1/products/{id}
    /// Delete a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        return _productService.Delete(id).Match(
            onSuccess: () => NoContent(),
            onFailure: errors => errors.ToActionResult()
        );
    }

    /// <summary>
    /// GET /api/v1/products/problem/{id}
    /// Demonstrates ProblemDetails extension methods.
    /// </summary>
    [HttpGet("problem/{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetProblemDetails(Guid id)
    {
        var result = _productService.GetById(id);

        // Using ToProblemDetails for direct ProblemDetails creation
        var problemDetails = result.ToProblemDetails();
        Console.WriteLine($"ProblemDetails Title: {problemDetails.Title}");

        // Using ControllerBase.Problem extension
        return result.IsSuccess
            ? Ok(result.Value)
            : this.Problem(result);
    }
}
