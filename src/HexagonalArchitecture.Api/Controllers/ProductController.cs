using HexagonalArchitecture.Application.Products.Commands.CreateProduct;
using HexagonalArchitecture.Application.Products.Commands.DeleteProduct;
using HexagonalArchitecture.Application.Products.Commands.UpdateProduct;
using HexagonalArchitecture.Application.Products.Queries.GetProducts;
using HexagonalArchitecture.Domain.Configurations.Localization;
using HexagonalArchitecture.Domain.Exceptions;
using HexagonalArchitecture.Domain.Permissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HexagonalArchitecture.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<OneResource> _localizer;


    public ProductController(IMediator mediator, IStringLocalizer<OneResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        var query = new GetProductsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = OnePermissions.AdminPolicy)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = OnePermissions.AdminPolicy)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = OnePermissions.AdminPolicy)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var command = new DeleteProductCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
    [HttpGet("test-exception")]
    public IActionResult TestException()
    {
        throw new BusinessException("Customer:NotFound");
    }
    [HttpGet("welcome")]
    public IActionResult Welcome()
    {
        return Ok(_localizer["Welcome"].Value);
    }
}
