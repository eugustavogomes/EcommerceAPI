using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ECommerceAPI.Domain;

namespace ECommerceAPI.API;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> Post([FromBody] UserSignUpDto dto)
    {
        //await _authService.SignUpAsync(dto);
        return Ok();
    }
    
    [HttpPost("signin")]
    public async Task <IActionResult> Post([FromBody] UserSignInDto dto)
    {
        //await _authService.SignInAsync(dto);
        //return OK(new {Token})
        
        return Ok();
    }
}