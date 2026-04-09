namespace ECommerceAPI.Domain;

public record SignInResponseDto(string email, string password, string token);