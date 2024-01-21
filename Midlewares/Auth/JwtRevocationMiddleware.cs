using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using OnlineCoursePlatform.Data.Entities;

namespace OnlineCoursePlatform.Midlewares.Auth
{
    public class JwtRevocationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtRevocationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<AppUser> userManager)
        {
            var authResult = await context.AuthenticateAsync();
            if (authResult.Succeeded)
            {
                var accessToken = context.Request.Headers["Authorization"];
                if (accessToken.ToString().StartsWith("Bearer "))
                {
                    accessToken = accessToken.ToString().Substring("Bearer ".Length).Trim();
                }

                var handler = new JsonWebTokenHandler();
                var jsonToken = handler.ReadJsonWebToken(accessToken);
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim is null)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Internal server error");
                    return;
                }
                var user = await userManager.FindByIdAsync(userIdClaim.Value);
                if (user != null && user.LastRevoked > jsonToken.ValidFrom)
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    await context.Response.WriteAsync("Token is revoked");
                    return;
                }
            }

            await _next(context);
        }
    }
}