using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WarehouseApp.Models;
using StorageManager.Models;

namespace WarehouseApp.Services
{
    public class TokenService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly byte[] _key;

        public TokenService()
        {
            _issuer = ConfigurationManager.AppSettings["Jwt:Issuer"];
            _audience = ConfigurationManager.AppSettings["Jwt:Audience"];
            var secret = ConfigurationManager.AppSettings["Jwt:Key"];
            _key = Encoding.UTF8.GetBytes(secret);
        }

        public string GenerateJwt(User user, TimeSpan validFor)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            }.ToList();

            // Add roles
            var roles = user.UserRoles?.Select(ur => ur.Role?.Name).Where(r => !string.IsNullOrEmpty(r)) ?? Enumerable.Empty<string>();
            foreach (var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }

            var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(validFor),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}