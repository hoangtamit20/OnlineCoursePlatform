using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineCoursePlatform.Configurations;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Midlewares.Auth;
using OnlineCoursePlatform.Repositories.AuthRepositories;
using OnlineCoursePlatform.Services.AuthServices;
using OnlineCoursePlatform.Services.AuthServices.IAuthServices;
using OnlineCoursePlatform.Services.EmailServices;

var builder = WebApplication.CreateBuilder(args);
{
    // Add services to the container.
    builder.Services.AddControllers();


    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    // Configuration swagger doc
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "Please enter your token with this format: ''Bearer YOUR_TOKEN''",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "bearer",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
            {
                new OpenApiSecurityScheme
                {
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                new List<string>()
            }
            });

            options.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "v1",
                Title = "Online course platform service api",
                Description = "Sample .NET api by ",
                Contact = new OpenApiContact()
                {
                    Name = "Hoang Trong Tam",
                    Url = new Uri("https://www.youtube.com")
                }
            });

            options.EnableAnnotations();

            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var path = Path.Combine(AppContext.BaseDirectory, xmlFileName);
            options.IncludeXmlComments(path);
        });
    }

    // add jwt, google authentication
    {
        // builder.Services.AddAuthentication(options =>
        // {
        //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        // })
        // .AddJwtBearer(jwt =>
        // {
        //     var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtConfig:SecretKey").Value!);
        //     jwt.SaveToken = true;
        //     jwt.TokenValidationParameters = new TokenValidationParameters()
        //     {
        //         ValidateIssuerSigningKey = true,
        //         IssuerSigningKey = new SymmetricSecurityKey(key),
        //         ValidateIssuer = false, //for dev
        //         ValidateAudience = false, // for dev
        //         RequireExpirationTime = false, // for dev --- needs to be update when refresh token is
        //         ValidateLifetime = true,

        //     };
        // });


        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtConfig:SecretKey").Value!);
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = builder.Configuration.GetSection("JwtConfig:ValidIssuer").Value,
                ValidAudience = builder.Configuration.GetSection("JwtConfig:ValidAudience").Value,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var jwt = context.SecurityToken as JsonWebToken;
                    if (jwt == null)
                    {
                        context.Fail("Invalid token");
                    }
                    return Task.CompletedTask;
                }
            };
        });


        // // This line adds authentication services to the application.
        // builder.Services.AddAuthentication(options =>
        // {
        //     // The DefaultAuthenticateScheme sets the scheme that will be used by default to authenticate the user.
        //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

        //     // The DefaultScheme sets the scheme that will be used by default for User and SignInAsync(HttpContext, String, ClaimsPrincipal, AuthenticationProperties).
        //     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

        //     // The DefaultChallengeScheme sets the scheme that will be used by default to challenge the user.
        //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        // })
        // .AddJwtBearer(jwt =>
        // {
        //     // SaveToken is set to true to store the token in the AuthenticationProperties after a successful authorization.
        //     jwt.SaveToken = true;
        //     var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtConfig:SecretKey").Value!);
        //     // TokenValidationParameters is the parameters used to validate a token.
        //     jwt.TokenValidationParameters = new TokenValidationParameters()
        //     {
        //         // ValidateIssuerSigningKey is set to true to ensure the signing key is valid.
        //         ValidateIssuerSigningKey = true,

        //         // IssuerSigningKey sets the key used to validate the token signature.
        //         IssuerSigningKey = new SymmetricSecurityKey(key),

        //         // ValidateIssuer is set to false, meaning it won't check if the token issuer is trusted.
        //         ValidateIssuer = true,

        //         // ValidateAudience is set to false, meaning it won't validate if the token was meant for this audience.
        //         ValidateAudience = true,

        //         // RequireExpirationTime is set to false, meaning tokens don't need to have an expiry date.
        //         RequireExpirationTime = true,

        //         ValidIssuer = builder.Configuration.GetSection("JwtConfig:ValidIssuer").Value,

        //         ValidAudience = builder.Configuration.GetSection("JwtConfig:ValidAudience").Value,

        //         // ValidateLifetime is set to true to validate the token has not expired.
        //         ValidateLifetime = true,

                // // LifetimeValidator is a delegate that can be used to implement custom lifetime validation logic.
                // LifetimeValidator = (before, expires, token, parameters) =>
                // {
                //     // The token is cast to a JwtSecurityToken.
                //     var jwtToken = token as JwtSecurityToken;

                //     // If the token is null, return false to fail the validation.
                //     if (jwtToken == null)
                //     {
                //         System.Console.WriteLine("=========== ERROR : JWTToken is NULL");
                //         return false;
                //     }

                //     // Try to get the claim for the user id.
                //     var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                //     // If the user id claim is null, return false to fail the validation.
                //     if (userIdClaim == null)
                //     {
                //         System.Console.WriteLine("=========== ERROR : userIdClaim is NULL");
                //         return false;
                //     }

                //     // Check if the token is not yet valid or has already expired.
                //     if (DateTime.UtcNow < before || DateTime.UtcNow > expires)
                //     {
                //         var a = DateTime.UtcNow < before ? "DATETIME.UTCNOW : " + "'"+DateTime.UtcNow.ToString()+"' < " + " BEFORE : '" +before.ToString()+ "'"
                //         : "DATETIME.UTCNOW : " + "'"+DateTime.UtcNow.ToString()+"' > " + " BEFORE : '" +expires.ToString()+ "'";
                //         System.Console.WriteLine(a);
                //         return false;
                //     }

                //     // If everything is okay, return true to pass the validation.
                //     return true; // Temporarily return true here
                // }
            // };
        // });

    }

    builder.Services.AddAuthorization();

    // add dbcontext
    builder.Services
        .AddDbContextFactory<OnlineCoursePlatformDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure());
        });

    // add identity
    builder.Services
        .AddIdentity<AppUser, IdentityRole>()
        .AddEntityFrameworkStores<OnlineCoursePlatformDbContext>()
        .AddApiEndpoints()
        .AddDefaultTokenProviders();


    // add sendmail service
    builder.Services.AddTransient<IEmailSender, EmailSender>();

    // add configure the behavior of API responses when the model state is invalid
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    // add repository service
    {
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
    }


    // add service
    {
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<ILoginService, LoginService>();
        builder.Services.AddScoped<IRegisterService, RegisterService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IResetPasswordService, ResetPasswordService>();
    }

    // add signalR
    builder.Services.AddSignalR();

    // add service jwt
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

}


var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseCors(options =>
    {
        options
            .AllowAnyHeader()
            .AllowAnyOrigin()
            .AllowAnyMethod();
    });
    app.MapControllers();


    {
        // register middleware
        app.UseMiddleware<JwtRevocationMiddleware>();
    }
    app.Run();
}