using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElevenNote.Data;
using ElevenNote.Data.Entities;
using ElevenNote.Models.Config;
using ElevenNote.Models.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ElevenNote.Services.Token;

public class TokenService : ITokenService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly UserManager<UserEntity> _userManager;

    public TokenService(ApplicationDbContext context, IConfiguration configuration, UserManager<UserEntity> userManager)
    {
        _context = context;
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task<TokenResponse?> GetTokenAsync(TokenRequest model)
    {
        var userEntity = await GetValidUserAsync(model);
        if (userEntity is null)
            return null;

        return await GenerateToken(userEntity);
    }

    private async Task<UserEntity?> GetValidUserAsync(TokenRequest model)
    {
        var userEntity = await _userManager.FindByNameAsync(model.UserName);

        if (userEntity is null)
            return null;

        var isValidPassword = await _userManager.CheckPasswordAsync(userEntity, model.Password);
        if (isValidPassword == false)
            return null;

        return userEntity;
    }

    private async Task<TokenResponse> GenerateToken(UserEntity entity)
    {
        List<Claim> claims = await GetUserClaims(entity);
        SecurityTokenDescriptor tokenDescriptor = GetTokenDescriptor(claims);

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        TokenResponse response = new()
        {
            Token = tokenHandler.WriteToken(token),
            IssuedAt = token.ValidFrom,
            Expires = token.ValidTo
        };

        return response;
    }

    private async Task<List<Claim>> GetUserClaims(UserEntity entity)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, entity.Id.ToString()),
            new Claim(ClaimTypes.Name, entity.UserName!),
            new Claim(ClaimTypes.Email, entity.Email!)
        };

        var roles = await _userManager.GetRolesAsync(entity);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private SecurityTokenDescriptor GetTokenDescriptor(List<Claim> claims)
    {
        JwtConfig config = new();
        _configuration.GetSection("Jwt").Bind(config);

        var key = Encoding.UTF8.GetBytes(config.Key);
        var secret = new SymmetricSecurityKey(key);
        var signingCredentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Issuer = config.Issuer,
            Audience = config.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(14),
            SigningCredentials = signingCredentials
        };
        return tokenDescriptor;
    }
}
