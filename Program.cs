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
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
{
    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    
    // add dbcontext
    builder.Services
        .AddDbContextFactory<OnlineCoursePlatformDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection String is not found"),
            sqlOptions => sqlOptions.EnableRetryOnFailure());
        });

    // add identity
    builder.Services
        .AddIdentity<AppUser, IdentityRole>()
        .AddEntityFrameworkStores<OnlineCoursePlatformDbContext>()
        .AddSignInManager()
        .AddRoles<IdentityRole>();


    // add jwt, google authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.SaveToken = true; // Thêm dòng này
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
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

                // Thêm mã xử lý tại đây nếu cần

                return Task.CompletedTask;
            }
        };
    });


    


    // Configuration swagger doc
    {
        
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "Please enter your token with this format: ''Bearer YOUR_TOKEN''",
                Type = SecuritySchemeType.ApiKey,
            });
            
            options.OperationFilter<SecurityRequirementsOperationFilter>();

            // options.AddSecurityRequirement(new OpenApiSecurityRequirement
            // {
            // {
            //     new OpenApiSecurityScheme
            //     {
            //         Name = "Bearer",
            //         In = ParameterLocation.Header,
            //         Reference = new OpenApiReference
            //         {
            //             Id = "Bearer",
            //             Type = ReferenceType.SecurityScheme
            //         }
            //     },
            //     new List<string>()
            // }
            // });

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
}

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
    builder.Services.AddScoped<ILogOutService, LogOutService>();
}

// add signalR
builder.Services.AddSignalR();

var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

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