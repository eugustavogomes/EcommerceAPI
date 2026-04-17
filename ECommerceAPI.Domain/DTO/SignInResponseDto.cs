namespace ECommerceAPI.Domain;

public record LoginRequestDto(string Email, string Password);

public record LoginResponseDto(string Token);
