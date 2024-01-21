using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
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


        private async Task<string> GenerateAccessTokenAsync(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:SecretKey").Value!));
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

            
            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("JwtConfig:ValidIssuer").Value,
                audience: _configuration.GetSection("JwtConfig:ValidAudience").Value,
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // This method generates a new JWT (JSON Web Token) for a user.
        public async Task<TokenModel> GenerateJwtTokenAsync(AppUser user, string? ipAddress)
        {
            // // Create a new JWT token handler.
            // var jwtTokenHandler = new JwtSecurityTokenHandler();

            // // Get the secret key from the configuration.
            // var key = _configuration.GetSection("JwtConfig:SecretKey").Value!;

            // // Create the claims for the JWT token.
            // var claims = await CreateClaimsAsync(user);

            // // Create the token descriptor for the JWT token.
            // var tokenDescriptor = CreateTokenDescriptor(claims, DateTime.UtcNow.AddHours(1), key);

            // // Create the JWT token.
            // var accessToken = CreateJwtToken(jwtTokenHandler, tokenDescriptor);

            // Return a new TokenModel with the access token and a new refresh token.
            return new TokenModel()
            {
                AccessToken = await GenerateAccessTokenAsync(user: user),
                RefreshToken = ""
            };
        }

        // This method generates a new refresh token for a user.
        private async Task<string> GenerateRefreshTokenAsync(AppUser user, string? ipAddress)
        {
            // Create a new refresh token with the user's ID, the current IP address, and an expiry date 7 days in the future.
            var refreshToken = new UserRefreshToken
            {
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7), // Set expiry date 7 days in the future
                RemoteIpAddress = ipAddress
            };

            // Add the new refresh token to the user's collection of refresh tokens.
            user.UserRefreshTokens.Add(refreshToken);

            // Save the changes to the database.
            await _userManager.UpdateAsync(user);

            // Return the value of the new refresh token.
            return refreshToken.Token;
        }

        // This method creates the claims for a JWT token.
        private async Task<List<Claim>> CreateClaimsAsync(AppUser user)
        {
            // Create a new list of claims.
            var listClaim = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
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
            DateTime expires,
            string secretKey)
        {
            // Convert the secret key to a byte array.
            var key = Encoding.UTF8.GetBytes(secretKey);
            // Return a new SecurityTokenDescriptor.
            return new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _configuration.GetSection("JwtConfig:ValidIssuer").Value,
                Audience = _configuration.GetSection("JwtConfig:ValidAudience").Value,
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
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