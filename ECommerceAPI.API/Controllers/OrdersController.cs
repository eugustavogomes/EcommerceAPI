using ECommerceAPI.API.Extensions;
using ECommerceAPI.Application.Services;
using ECommerceAPI.Domain;
using ECommerceAPI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(OrderService orderService) : ControllerBase
{
    private Guid UserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User ID not found in token."));

    private bool IsAdmin => User.IsInRole("Admin");

    [HttpPost("checkout")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Checkout()
    {
        try
        {
            var order = await orderService.CheckoutAsync(UserId);
            return CreatedAtAction(nameof(GetById), new { id = order.Id },
                new ApiResponse<OrderDto> { Success = true, Data = order, Message = "Order placed successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<OrderDto> { Success = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<OrderDto> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetMyOrders(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var (orders, total) = await orderService.GetUserOrdersAsync(UserId, pageNumber, pageSize);
        Response.AddPaginationHeaders(total, pageNumber, pageSize);
        return Ok(new ApiResponse<List<OrderDto>> { Success = true, Data = orders });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id)
    {
        try
        {
            var order = await orderService.GetOrderByIdAsync(id, UserId, IsAdmin);
            return Ok(new ApiResponse<OrderDto> { Success = true, Data = order });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<OrderDto> { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Cancel(Guid id)
    {
        try
        {
            var order = await orderService.CancelOrderAsync(id, UserId, IsAdmin);
            return Ok(new ApiResponse<OrderDto> { Success = true, Data = order, Message = "Order cancelled." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<OrderDto> { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<OrderDto> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAll(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var (orders, total) = await orderService.GetAllOrdersAsync(pageNumber, pageSize);
        Response.AddPaginationHeaders(total, pageNumber, pageSize);
        return Ok(new ApiResponse<List<OrderDto>> { Success = true, Data = orders });
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var order = await orderService.UpdateOrderStatusAsync(id, dto.Status);
            return Ok(new ApiResponse<OrderDto> { Success = true, Data = order });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<OrderDto> { Success = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<OrderDto> { Success = false, Message = ex.Message });
        }
    }
}
