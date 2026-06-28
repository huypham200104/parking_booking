using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace parking_booking_backend.Services;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse> VerifyAsync(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                PhoneNumber = request.PhoneNumber,
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.PhoneNumber : request.FullName.Trim(),
                Role = Role.Driver
            };

            _dbContext.Users.Add(user);
            _dbContext.Wallets.Add(new Wallet { UserId = user.Id, Balance = 0 });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.FullName) && user.FullName != request.FullName.Trim())
        {
            user.FullName = request.FullName.Trim();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        return new AuthResponse(jwtString, user.Role, user.Id);
    }
}
