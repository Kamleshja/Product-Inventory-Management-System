using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PIMS.Application.DTOs.Auth;
using PIMS.Application.Exceptions;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PIMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new BadRequestException("Invalid email or password.");

        var isPasswordValid = await _userManager
            .CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
            throw new BadRequestException("Invalid email or password.");

        return await GenerateJwtTokenAsync(user);
    }
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new BadRequestException("User already exists.");

        if (request.Role == "Administrator")
        {
            var isAdmin = _httpContextAccessor
                .HttpContext?
                .User?
                .IsInRole("Administrator");

            if (isAdmin != true)
                throw new UnauthorizedAccessException("Only Admin can create Administrator users.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var roleExists = await _roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
            throw new BadRequestException("Invalid role.");

        await _userManager.AddToRoleAsync(user, request.Role);

        return await GenerateJwtTokenAsync(user);
    }
    private async Task<AuthResponseDto> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        foreach (var role in roles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtSettings = _configuration.GetSection("JwtSettings");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["DurationInMinutes"]!)),
            claims: authClaims,
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo
        };
    }
}