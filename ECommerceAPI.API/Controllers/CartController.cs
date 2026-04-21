using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Application.Services;
using System.Security.Claims;
using ECommerceAPI.Domain;

namespace ECommerceAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token");

            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new InvalidOperationException("Invalid user ID format");

            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.GetCartAsync(userId);

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Cart retrieved successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<CartDto>>> AddToCart(
            [FromBody] AddToCartDto addToCartDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<CartDto>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }

                var userId = GetUserId();
                var cart = await _cartService.AddToCartAsync(
                    userId,
                    addToCartDto.ProductId,
                    addToCartDto.Quantity
                );

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Product added to cart"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("remove")]
        public async Task<ActionResult<ApiResponse<CartDto>>> RemoveFromCart(
            [FromBody] RemoveFromCartDto removeFromCartDto
        )
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.RemoveFromCartAsync(
                    userId,
                    removeFromCartDto.ProductId
                );

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Product removed from cart"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        
        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItem(
            [FromBody] UpdateCartItemDto updateCartItemDto
        )
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.UpdateItemQuantityAsync(
                    userId,
                    updateCartItemDto.ProductId,
                    updateCartItemDto.Quantity
                );

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Cart updated successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete]
        public async Task<ActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                await _cartService.ClearCartAsync(userId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}