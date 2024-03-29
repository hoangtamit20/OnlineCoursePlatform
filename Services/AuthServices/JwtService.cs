using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Configurations;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Models.AuthModels;
using OnlineCoursePlatform.Services.AuthServices.IAuthServices;

namespace OnlineCoursePlatform.Services.AuthServices
{
    public class JwtService : IJwtService
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        public JwtService(
            UserManager<AppUser> userManager,
            IConfiguration configuration)
        => (_userManager, _configuration) = (userManager, configuration);


        public async Task<string> GenerateAccessTokenAsync(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[AppSettingsConfig.JWT_SECRETKEY]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email!)
            };
            // Add a claim for each role the user has.
            foreach (var role in await _userManager.GetRolesAsync(user))
                userClaims.Add(new Claim(ClaimTypes.Role, role));

            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                    issuer: _configuration[AppSettingsConfig.JWT_ISSUER],
                    audience: _configuration[AppSettingsConfig.JWT_AUDIENCE],
                    claims: userClaims,
                    notBefore: now,
                    expires: now.AddHours(1),
                    signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // This method generates a new JWT (JSON Web Token) for a user.
        public async Task<TokenModel> GenerateJwtTokenAsync(AppUser user, string? ipAddress)
        {
            var accessToken = await GenerateAccessTokenAsync(user: user);

            // Return a new TokenModel with the access token and a new refresh token.
            return new TokenModel()
            {
                AccessToken = accessToken,
                RefreshToken = await GenerateRefreshTokenAsync(
                    user: user, 
                    ipAddress: ipAddress, 
                    accessToken: accessToken)
            };
        }

        // This method generates a new refresh token for a user.
        private async Task<string> GenerateRefreshTokenAsync(AppUser user, string? ipAddress, string accessToken)
        {
            // Create a new refresh token with the user's ID, the current IP address, and an expiry date 7 days in the future.
            var refreshToken = new UserRefreshToken
            {
                UserId = user.Id,
                AccessToken = accessToken,
                Expires = DateTime.UtcNow.AddDays(7), // Set expiry date 7 days in the future
                RemoteIpAddress = ipAddress
            };

            // Add the new refresh token to the user's collection of refresh tokens.
            user.UserRefreshTokens.Add(refreshToken);

            // Save the changes to the database.
            await _userManager.UpdateAsync(user);

            // Return the value of the new refresh token.
            return refreshToken.RefreshToken;
        }

        // This method creates the claims for a JWT token.
        private async Task<List<Claim>> CreateClaimsAsync(AppUser user)
        {
            // Create a new list of claims.
            var listClaim = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email!),
            };

            // Add a claim for each role the user has.
            foreach (var role in await _userManager.GetRolesAsync(user))
                listClaim.Add(new Claim(ClaimTypes.Role, role));

            // Return the list of claims.
            return listClaim;
        }

        // This method creates a token descriptor for a JWT token.
        private SecurityTokenDescriptor CreateTokenDescriptor(
            List<Claim> claims,
            string secretKey)
        {
            // Convert the secret key to a byte array.
            var key = Encoding.UTF8.GetBytes(secretKey);
            // Return a new SecurityTokenDescriptor.
            var now = DateTime.UtcNow;
            return new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _configuration[AppSettingsConfig.JWT_ISSUER],
                Audience = _configuration[AppSettingsConfig.JWT_AUDIENCE],
                NotBefore = now,
                Expires = now.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            };
        }

        // This method creates a JWT token.
        private string CreateJwtToken(
            JwtSecurityTokenHandler tokenHandler,
            SecurityTokenDescriptor tokenDescriptor)
        {
            // Create a new JWT token.
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the JWT token as a string.
            return tokenHandler.WriteToken(token);
        }

    }
}