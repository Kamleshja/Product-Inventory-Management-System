namespace PIMS.Application.DTOs.Auth;

public class RegisterRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = "User"; // default role
}