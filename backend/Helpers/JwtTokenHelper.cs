using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend.Constants;

namespace backend.Helpers
{
    public class JwtTokenHelper
    {
        private readonly IConfiguration _config;

        public JwtTokenHelper(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string userId, string email, string role, string name)
        {
            // Create JWT claims using AppConstants
            var claims = new[]
            {
                new Claim(AppConstants.Claims.UserId, userId),
                new Claim(AppConstants.Claims.Email, email),
                new Claim(AppConstants.Claims.Role, role),
                new Claim(AppConstants.Claims.Name, name)
            };

            // Read JWT settings from AppConstants (no more magic strings)
            string? keyString = _config[AppConstants.Jwt.Key];
            string? issuer = _config[AppConstants.Jwt.Issuer];
            string? audience = _config[AppConstants.Jwt.Audience];
            string? expiryString = _config[AppConstants.Jwt.ExpireMinutes];

            if (keyString == null)
                throw new InvalidOperationException("JWT Key is missing in configuration.");

            if (!double.TryParse(expiryString, out double expiryMinutes))
                expiryMinutes = 60; // default fallback

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
