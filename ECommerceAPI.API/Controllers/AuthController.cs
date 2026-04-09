using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ECommerceAPI.Domain;

namespace ECommerceAPI.API;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    
    
    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> Post([FromBody] SignUpResponseDto responseDto)
    {
        //await _authService.SignUpAsync(dto);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<IActionResult> Post([FromBody] SignInResponseDto responseDto)
    {
        //await _authService.SignInAsync(dto);
        //return OK(new {Token})

        return Ok();
    }
};