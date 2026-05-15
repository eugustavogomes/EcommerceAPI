using ECommerceAPI.API.Extensions;
using ECommerceAPI.API.Responses;
using ECommerceAPI.Application;
using ECommerceAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var (products, total) = await productService.GetProductsAsync(pageNumber, pageSize);
        Response.AddPaginationHeaders(total, pageNumber, pageSize);
        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> SearchProducts(
        [FromQuery] string? name = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var (products, total) = await productService.SearchProductsAsync(name, minPrice, maxPrice, pageNumber, pageSize);
        Response.AddPaginationHeaders(total, pageNumber, pageSize);
        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        try
        {
            var product = await productService.GetByIdAsync(id);
            return Ok(new ApiResponse<ProductDto> { Success = true, Data = product });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<ProductDto> { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var product = await productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id },
                new ApiResponse<ProductDto> { Success = true, Data = product });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse<ProductDto> { Success = false, Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<ProductDto> { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = await productService.UpdateProductAsync(id, dto);
            return Ok(new ApiResponse<ProductDto> { Success = true, Data = product });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<ProductDto> { Success = false, Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<ProductDto> { Success = false, Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await productService.DeleteProductAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }
}
